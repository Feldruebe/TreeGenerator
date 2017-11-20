using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TreeGeneratorLib.Generator;
using TreeGeneratorLib.Tree;

namespace TreeGeneratorLib.Wrappers
{
    public abstract class TreeVisual
    {
        public object TreeIamge { get; protected set; }

        public object TreeSkeletonIamge { get; protected set; }

        public abstract void DrawTree<T>(TreeModel<T> tree, TreeParameters treeParameters) where T : TreeVisual;
    }
}
