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
        public IList<BatchTreeParameter> BatchTreeParameters { get; set; }

        public int BatchWidth { get; set; }
        public int TreeCount { get; set; }
        public int BatchTreeDistance { get; set; }
    }
}
