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
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.Win32;

namespace SourceMerger
{
    public class SourceMergerOptionPageGrid : DialogPage
    {
        public const string SourceMergerCategoryName = "Source Merger";

        [Category(SourceMergerCategoryName)]
        [DisplayName("Merge Path")]
        [Description("The merged file will be stored in this folder")]
        public string MergePath { get; set; }

        [Category(SourceMergerCategoryName)]
        [DisplayName("Merged File Name")]
        [Description("The name of the merged file")]
        public string MergedFileName { get; set; }
    }

    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)] // Info on this package for Help/About
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(PackageGuidString)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
    [ProvideOptionPage(typeof(SourceMergerOptionPageGrid), SourceMergerOptionPageGrid.SourceMergerCategoryName, "Settings", 0, 0, true)]
    [ProvideAutoLoad(UIContextGuids.SolutionExists)]
    public sealed class SourceMergerPackage : Package
    {
        public const string PackageGuidString = "3dbfe381-278c-44b1-a18e-e946ac39b2e6";

        private DTE2 dte;
        private Events dteEvents;
        private DocumentEvents documentEvents;

        protected override void Initialize()
        {
            SourceMerger.Initialize(this);
            base.Initialize();

            //Save strong Reference to the DTE Object and Events
            dte = (DTE2)GetGlobalService(typeof(DTE));
            dteEvents = dte.Events;
            documentEvents = dteEvents.DocumentEvents;
            documentEvents.DocumentSaved += OnDocumentSaved;
        }

        private static void OnDocumentSaved(Document document)
        {
            SourceMerger.Instance.MergeActiveProjectSources();
            MessageBox.Show("Merged Sources");
        }
    }
}
