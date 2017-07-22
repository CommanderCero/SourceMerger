//------------------------------------------------------------------------------
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
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace SourceMerger
{
    internal sealed class SourceMerger
    {
        #region Useless Stuff that was here before... I swear!
        public const int CommandId = 0x0100;
        public static readonly Guid CommandSet = new Guid("fdf110a6-3b0a-4b06-b079-14fd9d3d1afd");

        private readonly Package package;
        private IServiceProvider ServiceProvider => package;

        public static SourceMerger Instance
        {
            get;
            private set;
        }

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

        #endregion

        public static void Initialize(Package package)
        {
            Instance = new SourceMerger(package);
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

        public static Regex UsingRegex = new Regex(@"using ([a-zA-Z\.]*);", RegexOptions.Compiled);
        public static Regex NamespaceContentRegex = new Regex("namespace .*?{(?<content>.*)}", RegexOptions.Singleline | RegexOptions.Compiled);
        public string MergeSources(IEnumerable<string> sources)
        {
            var usings = new HashSet<string>();
            var content = new List<string>();

            foreach (var source in sources)
            {
                foreach (var match in UsingRegex.Matches(source))
                    usings.Add(match.ToString());

                content.Add(NamespaceContentRegex.Match(source).Groups["content"].Value);
            }

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
