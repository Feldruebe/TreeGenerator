using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TreeGeneratorLib.Generator;
using TreeGeneratorLib.Tree;
using TreeGeneratorLib.Wrappers;

namespace TreeGeneratorWPF.Wrapper
{
    class WPFImageSourceWrapper : ImageSource
    {
        public WPFImageSourceWrapper()
        {

        }

        SKCanvas Canvas { get; }

        public override void DrawTree(TreeGenerator treeGenerator)
        {
            var tree = treeGenerator.Tree;
            var xOffset = -tree.ContourPoints.Min(point => (int)point.X);
            var yOffset = -tree.ContourPoints.Min(point => (int)point.Y);
            var imageWidth = tree.ContourPoints.Max(point => (int)point.X) - tree.ContourPoints.Min(point => (int)point.X) + 1;
            var imageHeight = tree.ContourPoints.Max(point => (int)point.Y) - tree.ContourPoints.Min(point => (int)point.Y) + 1;

            using (var treeBitmap = new Bitmap(imageWidth, imageHeight, PixelFormat.Format32bppPArgb))
            using (var skelettonBitmap = new Bitmap(imageWidth, imageHeight, PixelFormat.Format32bppPArgb))
            {
            }
        }

        protected override void DrawPoints(IPoint[] points, IColor color)
        {
            var skcolor = new SKColor(color.R, color.G, color.B, color.A);
            var paint = new SKPaint { Color = skcolor, Style = SKPaintStyle.Stroke };
            this.Canvas.DrawPoints(SKPointMode.Points, points.Select(point => new SKPoint(point.X, point.Y)).ToArray(), paint);
        }
    }
}
