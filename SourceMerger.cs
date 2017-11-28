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
            var sourceCollector = new SourceCollector();
            sourceCollector.CollectSources(projectPath, Settings.AdditionalSources);

            var mergedContent = MergeSources(sourceCollector.SystemImports, sourceCollector.Declarations);
            File.WriteAllText($@"{Settings.MergeFolderPath}\{Settings.MergeFileName}.cs", mergedContent);
        }

        public string MergeSources(IEnumerable<string> imports, IEnumerable<string> declarations)
        {
            var builder = new StringBuilder();
            foreach (var import in imports)
            {
                builder.Append(import);
                builder.AppendLine();
            }

            builder.AppendLine();

            foreach (var declaration in declarations)
            {
                builder.Append(declaration);
                builder.AppendLine();
                builder.AppendLine();
            }

            return builder.ToString();
        }

        #endregion
    }
}
