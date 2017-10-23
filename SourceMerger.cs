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

namespace SourceMerger
{
    public sealed class SourceMerger
    {
        public SourceMergerSettings Settings { get; set; } = new SourceMergerSettings();

        #region Source Mergin

        public void MergeProject(string projectPath)
        {
            var files = Directory.GetFiles(projectPath, "*.cs", SearchOption.AllDirectories);
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
