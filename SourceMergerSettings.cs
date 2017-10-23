using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SourceMerger
{
    public class SourceMergerSettings
    {
        public string MergeFolderPath { get; set; }
        public string MergeFileName { get; set; }

        public List<string> AdditionalSources { get; set; }
    }
}
