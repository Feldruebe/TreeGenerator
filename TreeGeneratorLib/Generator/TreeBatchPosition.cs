using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TreeGeneratorLib.Tree;
using TreeGeneratorLib.Wrappers;

namespace TreeGeneratorLib.Generator
{
    public class TreeBatchPosition<T> where T : TreeVisual
    {
        public int XPosition { get; set; }

        public TreeParameters TreeParameter { get; internal set; }
        public TreeModel<T> Tree { get; internal set; }
    }
}
