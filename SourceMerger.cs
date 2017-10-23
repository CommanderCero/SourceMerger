﻿//------------------------------------------------------------------------------
// <copyright file="SourceMerger.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace SourceMerger
{
    [ProvideAutoLoad("f1536ef8-92ec-443c-9ed7-fdadf150da82")]
    internal sealed class SourceMerger
    {
        public const int CommandId = 0x0100;
        public static readonly Guid CommandSet = new Guid("fdf110a6-3b0a-4b06-b079-14fd9d3d1afd");

        private readonly Package package;
        private IServiceProvider ServiceProvider => package;

        public static SourceMerger Instance{ get; private set; }

        public static SourceMergerSettings Settings { get; private set; }

        private SourceMerger(Package package)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            var commandService = ServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (commandService == null)
                return;

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new MenuCommand(MenuItemCallback, menuCommandID);
            commandService.AddCommand(menuItem);
        }

        public static void Initialize(Package package)
        {
            var dte = Package.GetGlobalService(typeof(DTE)) as DTE2;
            var settingsPath = Path.Combine(Path.GetDirectoryName(dte.Solution.FullName), "SourceMergerSettings.xml");

            if (!File.Exists(settingsPath))
                return;

            Instance = new SourceMerger(package);
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
                        Settings = (SourceMergerSettings)serializer.Deserialize(xmlReader);
                    }
                }
            }
            catch (Exception e)
            {
                VsShellUtilities.ShowMessageBox(
                    Instance.ServiceProvider,
                    e.Message,
                    "An Error occured while reading SourceMergerSettings.xml. SourceMerger will be disabled during this Session",
                    OLEMSGICON.OLEMSGICON_WARNING,
                    OLEMSGBUTTON.OLEMSGBUTTON_OK,
                    OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
            }
        }

        public void MenuItemCallback(object sender, EventArgs e)
        {
            MergeActiveProjectSources();
        }

        #region Source Mergin
        public void MergeActiveProjectSources()
        {
            var dte = Package.GetGlobalService(typeof(DTE)) as DTE2;
            var startupProject = dte.Solution.Item(((Array)dte.Solution.SolutionBuild.StartupProjects).GetValue(0));
            var startupPath = Path.GetDirectoryName(startupProject.FullName);

            var files = Directory.GetFiles(startupPath, "*.cs", SearchOption.AllDirectories);
            var resultContent = MergeSources(files.Select(File.ReadAllText));

            var settings = (SourceMergerOptionPageGrid)package.GetDialogPage(typeof(SourceMergerOptionPageGrid));
            File.WriteAllText($@"{settings.MergePath}\{settings.MergedFileName}.cs", resultContent);
        }

        public string MergeSources(IEnumerable<string> sources)
        {
            var usingsCollector = new UsingsCollector();
            (var usings, var content) = usingsCollector.CollectUsings(sources);

            var builder = new StringBuilder();
            foreach (var u in usings)
                builder.Append(u + "\n");
            foreach (var c in content)
                builder.Append(c + "\n");

            return builder.ToString();
        }

        #endregion
    }
}
