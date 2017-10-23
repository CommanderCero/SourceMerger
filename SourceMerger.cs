//------------------------------------------------------------------------------
// <copyright file="SourceMerger.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;

namespace SourceMerger
{
    public sealed class SourceMerger
    {
        public SourceMergerSettings Settings { get; set; } = new SourceMergerSettings();

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
            
            File.WriteAllText($@"{Settings.MergeFolderPath}\{Settings.MergeFileName}.cs", resultContent);
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
