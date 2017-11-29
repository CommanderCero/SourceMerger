using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SourceMerger
{
    public class SourceMergerException : Exception
    {
        public SourceMergerException(string message)
            : base(message)
        {
        }
    }
}
