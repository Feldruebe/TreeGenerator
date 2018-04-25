using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Spatial.Euclidean;
using TreeGeneratorLib.Generator;
using TreeGeneratorLib.Tree;
using TreeGeneratorLib.Wrappers;
using TreeGeneratorWPF.ViewModels;

namespace TreeGeneratorWPF.Wrapper
{
    using System.Windows.Media.Imaging;

    public class WpfTreeVisualWrapper : TreeVisual
    {
        public new BitmapSource TreeIamge
        {
            get { return (BitmapSource)base.TreeIamge; }

            set { base.TreeIamge = value; }
        }

        public new BitmapSource TreeSkeletonIamge
        {
            get { return (BitmapSource)base.TreeSkeletonIamge; }

            set { base.TreeSkeletonIamge = value; }
        }

        public override void DrawTree<T>(TreeModel<T> tree, TreeParameters treeParameters)
        {
            var xOffset = -tree.ContourPoints.Min(point => (int)point.X) + 20;
            var yOffset = -tree.ContourPoints.Min(point => (int)point.Y) + 20;
            var imageWidth = tree.ContourPoints.Max(point => (int)point.X) - tree.ContourPoints.Min(point => (int)point.X) + 41;
            var imageHeight = tree.ContourPoints.Max(point => (int)point.Y) - tree.ContourPoints.Min(point => (int)point.Y) + 41;

            using (var treeBitmap = new Bitmap(imageWidth, imageHeight, PixelFormat.Format32bppPArgb))
            using (var skelettonBitmap = new Bitmap(imageWidth, imageHeight, PixelFormat.Format32bppPArgb))
            {
                var dataTree = treeBitmap.LockBits(new Rectangle(0, 0, imageWidth, imageHeight), ImageLockMode.WriteOnly, treeBitmap.PixelFormat);
                var dataSkeletton = skelettonBitmap.LockBits(new Rectangle(0, 0, imageWidth, imageHeight), ImageLockMode.WriteOnly, skelettonBitmap.PixelFormat);
                using (var surfaceTree = SKSurface.Create(imageWidth, imageHeight, SKImageInfo.PlatformColorType, SKAlphaType.Premul, dataTree.Scan0, imageWidth * 4))
                using (var surfaceSkeletton = SKSurface.Create(imageWidth, imageHeight, SKImageInfo.PlatformColorType, SKAlphaType.Premul, dataSkeletton.Scan0, imageWidth * 4))
                {
                    MoveOriginToLeftBottom(surfaceTree, imageHeight, surfaceSkeletton);

                    SKBitmap leafBMP = null;
                    leafBMP = SKBitmap.Decode(tree.Leafparameters[0].ImageBuffer);

                    SKPaint paint = new SKPaint() { Color = SKColors.Black, StrokeWidth = 1, Style = SKPaintStyle.Stroke };
                    var treeBranches = tree.Branches.Concat(new[] { tree.Trunk }).ToList();
                    var skeletonPoints = treeBranches.Select(branch => branch.SkeletonPoints).SelectMany(points => points).ToArray();
                    foreach (var point in skeletonPoints)
                    {
                        surfaceSkeletton.Canvas.DrawPoint((float)point.Position.X + xOffset, (float)point.Position.Y + yOffset, paint);
                    }

                    this.DrawBranch(surfaceTree.Canvas, tree.Trunk, xOffset, yOffset, treeParameters.TrunkColor, treeParameters.OutlineColor);

                    var reversedBranches = tree.Branches.ToList();
                    foreach (Branch branch in reversedBranches)
                    {
                        this.DrawBranch(surfaceTree.Canvas, branch, xOffset, yOffset, treeParameters.TrunkColor, treeParameters.OutlineColor, leafBMP);
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

        private void DrawBranch(SKCanvas canvas, Branch branch, int xOffset, int yOffset, IColor color, IColor outlineColor, SKBitmap leafBMP = null)
        {
            var outlinePoints = branch.PolygonPoints.Select(point => new SKPoint((int)point.X + xOffset, (int)point.Y + yOffset)).ToList();
            var polygonPoints = outlinePoints.ToList();
            polygonPoints.Add(polygonPoints[0]);

            var maxDistance = branch.SDF.Values.Max();
            float vReduce = 0.8f;
            for (int i = 1; i <= maxDistance; i++)
            {
                SKColor trunkColor = new SKColor(color.R, color.G, color.B, color.A);
                float h, s, v;
                trunkColor.ToHsv(out h, out s, out v);
                var reduceFactor = 1 - (vReduce * ((maxDistance - i) / (float)maxDistance));

                SKColor colorForDistance = SKColor.FromHsv(h, s, v * reduceFactor);
                SKPaint fillForDistance = new SKPaint { Color = colorForDistance, Style = SKPaintStyle.Fill };
                var pointsWidthDistanceWithoutBlend = branch.SDF.Where(sdfPoint => sdfPoint.Value == i)
                    .Select(sdfPoint => new SKPoint((float)sdfPoint.Key.X + xOffset, (float)sdfPoint.Key.Y + yOffset)).ToArray();
                canvas.DrawPoints(SKPointMode.Points, pointsWidthDistanceWithoutBlend, fillForDistance);
            }

            SKColor skiaOutlineColor = new SKColor(outlineColor.R, outlineColor.G, outlineColor.B, outlineColor.A);
            SKPaint outline = new SKPaint { Color = skiaOutlineColor, Style = SKPaintStyle.Stroke };
            canvas.DrawPoints(SKPointMode.Points, branch.ContourPointsWithoutBot.Select(point => new SKPoint((float)point.X + xOffset, (float)point.Y + yOffset)).ToArray(), outline);
            //canvas.DrawPoints(SKPointMode.Points, branch.BotContourPoints.Select(point => new SKPoint((float)point.X + xOffset, (float)point.Y + yOffset)).ToArray(), outline);

            SKColor leafColorInner = new SKColor(10, 220, 10);
            SKColor leafColorOuter = new SKColor(0, 150, 0);
            SKPaint leafPaint = new SKPaint { Style = SKPaintStyle.Fill };
            SKPaint leafOutlinePaint = new SKPaint { Color = SKColors.Black, Style = SKPaintStyle.Stroke };
            var endPoint = branch.SkeletonPoints.Last();
            leafPaint.Shader = SKShader.CreateRadialGradient(new SKPoint((float)endPoint.Position.X + xOffset, (float)endPoint.Position.Y + yOffset), 20, new[] { leafColorInner, leafColorOuter }, null, SKShaderTileMode.Clamp);
//            canvas.DrawCircle((float)endPoint.Position.X + xOffset, (float)endPoint.Position.Y + yOffset, 20, leafPaint);
//            canvas.DrawCircle((float)endPoint.Position.X + xOffset, (float)endPoint.Position.Y + yOffset, 20, leafOutlinePaint);


            if (leafBMP != null)
            {
                for (int index = 0; index < branch.SkeletonPoints.Count; index += 15)
                {
                    var skeletonPoint = branch.SkeletonPoints[index];
                    var angle = skeletonPoint.GrowDirection.AngleTo(Vector2D.YAxis);
                    SKPaint bmpPaint = new SKPaint() { IsAntialias = false, FilterQuality = SKFilterQuality.None, IsDither = true };

                    canvas.RotateDegrees((float)-angle.Degrees, (float)skeletonPoint.Position.X + xOffset, (float)skeletonPoint.Position.Y + yOffset);

                    canvas.RotateDegrees(-90, (float)skeletonPoint.Position.X + xOffset, (float)skeletonPoint.Position.Y + yOffset);
                    canvas.Translate(-leafBMP.Width, -leafBMP.Height);
                    canvas.DrawBitmap(leafBMP, new SKRect((float)skeletonPoint.Position.X + xOffset, (float)skeletonPoint.Position.Y + yOffset, (float)skeletonPoint.Position.X + xOffset + leafBMP.Width, (float)skeletonPoint.Position.Y + yOffset + leafBMP.Height), bmpPaint);
                    canvas.Translate(leafBMP.Width, leafBMP.Height);
                    canvas.RotateDegrees(90, (float)skeletonPoint.Position.X + xOffset, (float)skeletonPoint.Position.Y + yOffset);

                    canvas.RotateDegrees(-180, (float)skeletonPoint.Position.X + xOffset, (float)skeletonPoint.Position.Y + yOffset);
                    canvas.Translate(-leafBMP.Width, -leafBMP.Height);
                    canvas.DrawBitmap(leafBMP, new SKRect((float)skeletonPoint.Position.X + xOffset, (float)skeletonPoint.Position.Y + yOffset, (float)skeletonPoint.Position.X + xOffset + leafBMP.Width, (float)skeletonPoint.Position.Y + yOffset + leafBMP.Height), bmpPaint);
                    canvas.Translate(leafBMP.Width, leafBMP.Height);
                    canvas.RotateDegrees(180, (float)skeletonPoint.Position.X + xOffset, (float)skeletonPoint.Position.Y + yOffset);

                    canvas.RotateDegrees((float)angle.Degrees, (float)skeletonPoint.Position.X + xOffset, (float)skeletonPoint.Position.Y + yOffset);
                }
            }
        }
    }
}