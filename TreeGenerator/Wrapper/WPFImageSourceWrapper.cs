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
    using System.Windows.Media.Imaging;

    public class WpfTreeVisualWrapper : TreeVisual
    {
        public new BitmapSource TreeIamge
        {
            get
            {
                return (BitmapSource)base.TreeIamge;
            }

            set
            {
                base.TreeIamge = value;
            }
        }

        public new BitmapSource TreeSkeletonIamge
        {
            get
            {
                return (BitmapSource)base.TreeSkeletonIamge;
            }

            set
            {
                base.TreeSkeletonIamge = value;
            }
        }

        public override void DrawTree<T>(TreeModel<T> tree, TreeParameters treeParameters)
        {
            var xOffset = -tree.ContourPoints.Min(point => (int)point.X);
            var yOffset = -tree.ContourPoints.Min(point => (int)point.Y);
            var imageWidth = tree.ContourPoints.Max(point => (int)point.X) - tree.ContourPoints.Min(point => (int)point.X) + 1;
            var imageHeight = tree.ContourPoints.Max(point => (int)point.Y) - tree.ContourPoints.Min(point => (int)point.Y) + 1;

            using (var treeBitmap = new Bitmap(imageWidth, imageHeight, PixelFormat.Format32bppPArgb))
            using (var skelettonBitmap = new Bitmap(imageWidth, imageHeight, PixelFormat.Format32bppPArgb))
            {
                var dataTree = treeBitmap.LockBits(new Rectangle(0, 0, imageWidth, imageHeight), ImageLockMode.WriteOnly, treeBitmap.PixelFormat);
                var dataSkeletton = skelettonBitmap.LockBits(new Rectangle(0, 0, imageWidth, imageHeight), ImageLockMode.WriteOnly, skelettonBitmap.PixelFormat);
                using (var surfaceTree = SKSurface.Create(imageWidth, imageHeight, SKImageInfo.PlatformColorType, SKAlphaType.Premul, dataTree.Scan0, imageWidth * 4))
                using (var surfaceSkeletton = SKSurface.Create(imageWidth, imageHeight, SKImageInfo.PlatformColorType, SKAlphaType.Premul, dataSkeletton.Scan0, imageWidth * 4))
                {
                    MoveOriginToLeftBottom(surfaceTree, imageHeight, surfaceSkeletton);

                    SKPaint paint = new SKPaint() { Color = SKColors.Black, StrokeWidth = 1, Style = SKPaintStyle.Stroke };
                    var treeBranches = tree.Branches.Concat(new[] { tree.Trunk }).ToList();
                    var skeletonPoints = treeBranches.Select(branch => branch.SkeletonPoints).SelectMany(points => points).ToArray();
                    foreach (var point in skeletonPoints)
                    {
                        surfaceSkeletton.Canvas.DrawPoint((float)point.Position.X + xOffset, (float)point.Position.Y + yOffset, paint);
                    }

                    this.DrawBranch(surfaceTree.Canvas, tree.Trunk, xOffset, yOffset, treeParameters.TrunkColor, treeParameters.OutlineColor);

                    var reversedBranches = tree.Branches.ToList();
                    reversedBranches.Reverse();
                    foreach (Branch branch in reversedBranches)
                    {
                        this.DrawBranch(surfaceTree.Canvas, branch, xOffset, yOffset, treeParameters.TrunkColor, treeParameters.OutlineColor);
                    }
                }

                treeBitmap.UnlockBits(dataTree);
                this.TreeIamge = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(treeBitmap.GetHbitmap(), IntPtr.Zero, System.Windows.Int32Rect.Empty, BitmapSizeOptions.FromWidthAndHeight(imageWidth, imageHeight));
                skelettonBitmap.UnlockBits(dataTree);
                this.TreeSkeletonIamge = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(skelettonBitmap.GetHbitmap(), IntPtr.Zero, System.Windows.Int32Rect.Empty, BitmapSizeOptions.FromWidthAndHeight(imageWidth, imageHeight));
            }
        }

        private static void MoveOriginToLeftBottom(SKSurface surfaceTree, int imageHeight, SKSurface surfaceSkeletton)
        {
            surfaceTree.Canvas.Translate(0, imageHeight);
            surfaceTree.Canvas.Scale(1, -1);
            surfaceSkeletton.Canvas.Translate(0, imageHeight);
            surfaceSkeletton.Canvas.Scale(1, -1);
        }

        private void DrawBranch(SKCanvas canvas, Branch branch, int xOffset, int yOffset, IColor color, IColor outlineColor)
        {
            var outlinePoints = branch.PolygonPoints.Select(point => new SKPoint((int)point.X + xOffset, (int)point.Y + yOffset)).ToList();
            var polygonPoints = outlinePoints.ToList();
            polygonPoints.Add(polygonPoints[0]);

            var maxDistance = branch.SDF.Values.Max();
            float vReduce = 0.8f;
            for (int i = 1; i <= maxDistance; i++)
            {
                float h, s, v;
                color.ToHsv(out h, out s, out v);
                var reduceFactor = 1 - (vReduce * ((maxDistance - i) / (float)maxDistance));

                SKColor colorForDistance = SKColor.FromHsv(h, s, v * reduceFactor);
                SKPaint fillForDistance = new SKPaint { Color = colorForDistance, Style = SKPaintStyle.Fill };
                var pointsWithDistance = branch.SDF.Where(sdfPoint => sdfPoint.Value == i).Select(sdfPoint => new SKPoint((float)sdfPoint.Key.X + xOffset, (float)sdfPoint.Key.Y + yOffset)).ToArray();
                canvas.DrawPoints(SKPointMode.Points, pointsWithDistance, fillForDistance);
            }

            SKColor skiaOutlineColor = new SKColor(outlineColor.R, outlineColor.G, outlineColor.B, outlineColor.A);
            SKPaint outline = new SKPaint { Color = skiaOutlineColor, Style = SKPaintStyle.Stroke };
            canvas.DrawPoints(SKPointMode.Points, branch.ContourPointsWithoutBot.Select(point => new SKPoint((float)point.X + xOffset, (float)point.Y + yOffset)).ToArray(), outline);
            //canvas.DrawPoints(SKPointMode.Points, branch.BotContourPoints.Select(point => new SKPoint((float)point.X + xOffset, (float)point.Y + yOffset)).ToArray(), outline);
        }
    }
}
