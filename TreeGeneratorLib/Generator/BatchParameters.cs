using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TreeGeneratorLib.Wrappers;

namespace TreeGeneratorLib.Generator
{
    public class BatchParameters
    {
        public IList<TreeParameters> TreeParameters { get; set; }

        public int BatchWidth { get; set; }
        public int TreeCount { get; internal set; }
    }
}
