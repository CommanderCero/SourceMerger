//------------------------------------------------------------------------------
// <copyright file="SourceMergerPackage.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.Win32;
using IServiceProvider = System.IServiceProvider;

namespace SourceMerger
{
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)] // Info on this package for Help/About
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(PackageGuidString)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
    [ProvideAutoLoad(UIContextGuids.SolutionExists)]
    public sealed class SourceMergerPackage : Package
    {
        public const string PackageGuidString = "3dbfe381-278c-44b1-a18e-e946ac39b2e6";
        public const int CommandId = 0x0100;
        public static readonly Guid CommandSet = new Guid("fdf110a6-3b0a-4b06-b079-14fd9d3d1afd");

        public SourceMerger Instance { get; set; }

        private DTE2 dte;
        private Events dteEvents;
        private DocumentEvents documentEvents;

        protected override void Initialize()
        {
            base.Initialize();
            //Save strong Reference to the DTE Object and Events
            dte = (DTE2)GetGlobalService(typeof(DTE));
            dteEvents = dte.Events;
            documentEvents = dteEvents.DocumentEvents;

            Instance = new SourceMerger();
            if (!LoadSettings())
            {
                return;
            }
                
            InitializeCommands();
        }

        private void InitializeCommands()
        {
            var commandService = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (commandService == null)
                return;
            
            var menuItem = new MenuCommand(Instance.MenuItemCallback, new CommandID(CommandSet, CommandId)) { Visible = true};
            commandService.AddCommand(menuItem);
            documentEvents.DocumentSaved += OnDocumentSaved;
        }

        private bool LoadSettings()
        {
            var settingsPath = Path.Combine(Path.GetDirectoryName(dte.Solution.FullName), "SourceMergerSettings.xml");
            try
            {
                var xmlDocument = new XmlDocument();
                xmlDocument.Load(settingsPath);
                var xmlString = xmlDocument.OuterXml;

                using (var stringReader = new StringReader(xmlString))
                {
                    var serializer = new XmlSerializer(typeof(SourceMergerSettings));
                    using (var xmlReader = new XmlTextReader(stringReader))
                    {
                        Instance.Settings = (SourceMergerSettings)serializer.Deserialize(xmlReader);
                    }
                }

                return true;
            }
            catch (Exception e)
            {
                VsShellUtilities.ShowMessageBox(
                    this,
                    e.Message,
                    "An Error occured while reading SourceMergerSettings.xml. SourceMerger will be disabled during this Session",
                    OLEMSGICON.OLEMSGICON_WARNING,
                    OLEMSGBUTTON.OLEMSGBUTTON_OK,
                    OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);

                return false;
            }
        }

        private void OnDocumentSaved(Document document)
        {
            Instance.MergeActiveProjectSources();
        }
    }
}
