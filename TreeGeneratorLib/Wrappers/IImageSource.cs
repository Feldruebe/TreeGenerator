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
    public interface IImageSource
    {
        //void DrawTree(TreeModel tree);
    }

    public abstract class ImageSource : IImageSource
    {
        public abstract void DrawTree(TreeGenerator tree);

        protected abstract void DrawPoints(IPoint[] points, IColor color);
    }
}
