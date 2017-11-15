﻿namespace TreeGenerator.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Windows.Media.Imaging;

    using GalaSoft.MvvmLight;

    using GalaSoft.MvvmLight.CommandWpf;
    using System.Diagnostics;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Threading.Tasks;
    using System.Windows.Media;

    using MahApps.Metro.Controls.Dialogs;

    using MathNet.Numerics.LinearAlgebra;
    using MathNet.Numerics.LinearAlgebra.Double;
    using MathNet.Numerics.Random;
    using MathNet.Spatial.Euclidean;
    using MathNet.Spatial.Units;

    using Microsoft.Win32;

    using SkiaSharp;

    using Color = System.Windows.Media.Color;
    using PixelFormat = System.Drawing.Imaging.PixelFormat;
    using MathNet.Numerics.Interpolation;
    using TreeGenerator.Properties;

    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    /// </para>
    /// <para>
    /// You can also use Blend to data bind with the tool's support.
    /// </para>
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        private Random rand;
        private Random randomRand = new Random();

        private int treeTrunkSize;
        private BitmapSource imageSkeletton;
        private BitmapSource imageTree;

        private int trunkRotationAngle;
        private float trunkRotationAngleStart;

        private int trunkSkewAngle;
        private int trunkSkewAngleStart;
        private int branchCount;
        private int branchStart;
        private int branchLengthMin;
        private int branchLengthMax;
        private int branchDistance;
        private int branchSkew;
        private int branchSkewDeviation;
        private int branchRotationAngleStart;
        private float branchRotationAngle;
        private int trunkWidthStart;
        private int trunkWidthEnd;
        private SKColor trunkColor;
        private SKColor branchColor;
        private SKColor outlineColor;
        private SKColor branchOutlineColor;
        private bool isColorFlyoutOpen = false;

        private int randomSeed;

        private bool regenerateRandomSeed;

        private bool debugModeEnabled;

        private TreeModel tree;

        private ProgressDialogController controller;
        private double branchLevelLengthFactor;
        private int branchMaxLevel;

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {
#if DEBUG
            this.DebugModeEnabled = true;
#endif

            this.DebugViewModel = new DebugViewModel(this);

            this.GenerateTreeCommand = new RelayCommand(this.GenerateTreeAndDraw);
            this.ExportImageCommand = new RelayCommand(this.ExportImage);
            this.TreeTrunkSize = 60;
            this.TrunkSkewAngle = 0;
            this.TrunkSkewAngleStart = 0;
            this.TrunkRotationAngle = 0;
            this.TrunkRotationAngleStart = 0;
            this.TrunkWidthStart = 8;
            this.TrunkWidthEnd = 2;
            this.BranchCount = 4;
            this.BranchStart = this.TreeTrunkSize / 3;
            this.BranchLengthMin = this.TreeTrunkSize;
            this.BranchLengthMax = this.TreeTrunkSize;
            this.BranchDistance = 20;
            this.BranchSkew = 35;
            this.BranchSkewDeviation = 10;
            this.BranchRotationAngleStart = 0;
            this.BranchRotationAngle = 30;
            this.TrunkColor = Colors.SaddleBrown;
            this.BranchColor = Colors.SaddleBrown;
            this.OutlineColor = Colors.Black;
            this.BranchOutlineColor = Colors.Black;
            this.BranchMaxLevel = 3;
            this.BranchLevelLengthFactor = 1;
        }

        public int RandomSeed
        {
            get
            {
                return this.randomSeed;
            }

            set
            {
                this.Set(ref this.randomSeed, value);
            }
        }

        public bool RegenerateRandomSeed
        {
            get
            {
                return this.regenerateRandomSeed;
            }

            set
            {
                this.Set(ref this.regenerateRandomSeed, value);
            }
        }

        public int TreeTrunkSize
        {
            get { return this.treeTrunkSize; }
            set
            {
                this.Set(ref this.treeTrunkSize, value);
                if (value < this.BranchStart)
                {
                    this.BranchStart = value;
                }
            }
        }

        public BitmapSource ImageSkeletton
        {
            get { return this.imageSkeletton; }
            set { this.Set(ref this.imageSkeletton, value); }
        }

        public BitmapSource ImageTree
        {
            get { return this.imageTree; }
            set { this.Set(ref this.imageTree, value); }
        }

        public int TrunkRotationAngle
        {
            get { return this.trunkRotationAngle; }
            set { this.Set(ref this.trunkRotationAngle, value); }
        }

        public float TrunkRotationAngleStart
        {
            get { return this.trunkRotationAngleStart; }

            set { this.Set(ref this.trunkRotationAngleStart, value); }
        }

        public int TrunkSkewAngle
        {
            get { return this.trunkSkewAngle; }
            set { this.Set(ref this.trunkSkewAngle, value); }
        }

        public int TrunkSkewAngleStart
        {
            get
            {
                return this.trunkSkewAngleStart;
            }

            set
            {
                var max = Math.Max(this.TrunkWidthStart, value);
                this.Set(ref this.trunkSkewAngleStart, max);
            }
        }

        public int BranchCount
        {
            get { return this.branchCount; }
            set { this.Set(ref this.branchCount, value); }
        }

        public int AllBranchesCount
        {
            get
            {
                return this.Tree?.Branches.Count - 1 ?? 0;
            }
        }

        public int BranchStart
        {
            get { return this.branchStart; }
            set { this.Set(ref this.branchStart, value); }
        }

        public int BranchLengthMin
        {
            get { return this.branchLengthMin; }
            set { this.Set(ref this.branchLengthMin, value); }
        }

        public int BranchLengthMax
        {
            get { return this.branchLengthMax; }
            set { this.Set(ref this.branchLengthMax, value); }
        }

        public int BranchDistance
        {
            get { return this.branchDistance; }
            set { this.Set(ref this.branchDistance, value); }
        }

        public double BranchLevelLengthFactor
        {
            get { return this.branchLevelLengthFactor; }
            set { this.Set(ref this.branchLevelLengthFactor, value); }
        }

        public int BranchSkew
        {
            get { return this.branchSkew; }

            set { this.Set(ref this.branchSkew, value); }
        }

        public int BranchSkewDeviation
        {
            get { return this.branchSkewDeviation; }
            set { this.Set(ref this.branchSkewDeviation, value); }
        }

        public int BranchRotationAngleStart
        {
            get { return this.branchRotationAngleStart; }
            set { this.Set(ref this.branchRotationAngleStart, value); }
        }

        public float BranchRotationAngle
        {
            get { return this.branchRotationAngle; }
            set { this.Set(ref this.branchRotationAngle, value); }
        }

        public int TrunkWidthStart
        {
            get
            {
                return this.trunkWidthStart;
            }

            set
            {
                if (value < this.TrunkWidthEnd)
                {
                    this.TrunkWidthEnd = value;
                }

                if (value > this.TrunkSkewAngleStart)
                {
                    this.TrunkSkewAngleStart = value;
                }

                this.Set(ref this.trunkWidthStart, value);
            }
        }

        public int TrunkWidthEnd
        {
            get { return this.trunkWidthEnd; }
            set { this.Set(ref this.trunkWidthEnd, value); }
        }

        public Color TrunkColor
        {
            get
            {
                return Color.FromArgb(this.trunkColor.Alpha, this.trunkColor.Red, this.trunkColor.Green, this.trunkColor.Blue);
            }

            set
            {
                SKColor c = new SKColor(value.R, value.G, value.B, value.A);
                this.Set(ref this.trunkColor, c);
                this.ReDrawTree();
            }
        }

        public Color BranchColor
        {
            get
            {
                return Color.FromArgb(this.branchColor.Alpha, this.branchColor.Red, this.branchColor.Green, this.branchColor.Blue);
            }

            set
            {
                SKColor c = new SKColor(value.R, value.G, value.B, value.A);
                this.Set(ref this.branchColor, c);
            }
        }

        public Color OutlineColor
        {
            get
            {
                return Color.FromArgb(this.outlineColor.Alpha, this.outlineColor.Red, this.outlineColor.Green, this.outlineColor.Blue);
            }

            set
            {
                SKColor c = new SKColor(value.R, value.G, value.B, value.A);
                this.Set(ref this.outlineColor, c);
                this.ReDrawTree();
            }
        }

        public bool IsColorFlyoutOpen
        {
            get
            {
                return this.isColorFlyoutOpen;
            }

            set
            {
                this.Set(ref this.isColorFlyoutOpen, value);
            }
        }

        public Color BranchOutlineColor
        {
            get
            {
                return Color.FromArgb(this.branchOutlineColor.Alpha, this.branchOutlineColor.Red, this.branchOutlineColor.Green, this.branchOutlineColor.Blue);
            }

            set
            {
                SKColor c = new SKColor(value.R, value.G, value.B, value.A);
                this.Set(ref this.branchOutlineColor, c);
                this.RaisePropertyChanged();
            }
        }

        public RelayCommand GenerateTreeCommand { get; set; }

        public RelayCommand ExportImageCommand { get; set; }

        public bool DebugModeEnabled
        {
            get
            {
                return this.debugModeEnabled;
            }

            set
            {
                this.Set(ref this.debugModeEnabled, value);
                this.ReDrawTree();
            }
        }

        public bool IsDebugMode
        {
            get
            {
#if DEBUG
                return true;
#endif

                return false;
            }
        }

        public DebugViewModel DebugViewModel { get; }

        public TreeModel Tree
        {
            get
            {
                return this.tree;
            }

            private set
            {
                this.tree = value;
                this.RaisePropertyChanged(nameof(this.AllBranchesCount));
            }
        }

        public int BranchMaxLevel
        {
            get { return this.branchMaxLevel; }
            set { this.Set(ref this.branchMaxLevel, value); }
        }

        private async void GenerateTreeAndDraw()
        {
            //try
            //{
            this.controller = await DialogCoordinator.Instance.ShowProgressAsync(this, "Waiting...", "Wait", true);
            this.ManageRandom();
            this.Tree = await this.GenerateTreeAsync();
            this.DrawTree(this.Tree);
            await this.controller.CloseAsync();
            //}
            //catch (Exception exception)
            //{
            //    MessageBox.Show(exception.Message);
            //}
        }

        private Task<TreeModel> GenerateTreeAsync()
        {
            return Task.Run(() => this.GenerateTree());
        }

        private void ManageRandom()
        {
            if (this.RegenerateRandomSeed || this.rand == null)
            {
                var seed = this.randomRand.Next();
                this.RandomSeed = seed;
            }

            this.rand = new Random(this.RandomSeed);
        }

        private void ExportImage()
        {
            SaveFileDialog dialog = new SaveFileDialog();
            if (dialog.ShowDialog() == true)
            {
                this.CreatePng(dialog.FileName, this.ImageTree);
            }
        }

        private void CreatePng(string filename, BitmapSource image5)
        {
            if (filename != string.Empty)
            {
                using (FileStream stream5 = new FileStream(filename, FileMode.Create))
                {
                    PngBitmapEncoder encoder5 = new PngBitmapEncoder();
                    encoder5.Frames.Add(BitmapFrame.Create(image5));
                    encoder5.Save(stream5);
                }
            }
        }

        private TreeModel GenerateTree()
        {
            var tree = new TreeModel();

            Vector2D growDirection = new Vector2D(0d, 1d);

            float rotationStep = (float)this.TrunkRotationAngle / (float)(this.TreeTrunkSize - this.TrunkRotationAngleStart);

            Point2D currentPoint = new Point2D(0d, 0d);
            var trunkStartWidth = this.TrunkWidthStart;
            var trunkEndWidth = this.TrunkWidthEnd;
            tree.Trunk.SkelletonPoints.Add(new TreePoint(currentPoint, growDirection) { Width = trunkStartWidth });

            int treeCrownSize = this.TreeTrunkSize - this.BranchStart;

            HashSet<int> leftBranches;
            HashSet<int> rightBranches;
            this.GenerateBranchPositions(this.BranchStart, treeCrownSize, this.BranchDistance, out leftBranches, out rightBranches);

            // Generate skelleton.
            this.controller.SetMessage($"Generating trunk pixel.");
            for (int y = 0; y < this.TreeTrunkSize - 1; y++)
            {
                if (y == this.TrunkSkewAngleStart)
                {
                    growDirection = growDirection.Rotate(new Angle(this.TrunkSkewAngle, new Degrees()));
                }

                if (y > this.TrunkRotationAngleStart)
                {
                    growDirection = growDirection.Rotate(new Angle(rotationStep, new Degrees()));
                }

                // Generate trunk.
                var currentWidth = trunkEndWidth + (trunkStartWidth - trunkEndWidth) * ((this.TreeTrunkSize - y) / (double)this.TreeTrunkSize);
                currentPoint += growDirection;
                tree.Trunk.SkelletonPoints.Add(new TreePoint(new Point2D(Math.Round(currentPoint.X), Math.Round(currentPoint.Y)), growDirection) { Width = (int)currentWidth });
                tree.Trunk.ParentBranchConnectPoint = tree.Trunk.SkelletonPoints.First().Position;

                // Generate branches.
                if (y > this.BranchStart)
                {
                    int connectIndex = Math.Max(0, y - 20);

                    if (leftBranches.Contains(y))
                    {
                        this.GenerateBranch(tree, growDirection, currentPoint, tree.Trunk.SkelletonPoints[connectIndex].Position, BranchType.Left, currentWidth);
                    }

                    if (rightBranches.Contains(y))
                    {
                        this.GenerateBranch(tree, growDirection, currentPoint, tree.Trunk.SkelletonPoints[connectIndex].Position, BranchType.Right, currentWidth);
                    }
                }
            }

            this.controller.SetMessage($"Generating contour and fill.");
            tree.GenerateContourAndFillPoints();

            this.controller.SetMessage($"Generating shading.");
            tree.GenerateSDF();

            return tree;
        }

        private void GenerateBranchPositions(int branchStart, int treeCrownSize, int branchDistance, out HashSet<int> leftBranches, out HashSet<int> rightBranches)
        {
            HashSet<int> leftCrown = new HashSet<int>(Enumerable.Range(branchStart, treeCrownSize));
            HashSet<int> rightCrown = new HashSet<int>(Enumerable.Range(branchStart, treeCrownSize));
            leftBranches = new HashSet<int>();
            rightBranches = new HashSet<int>();

            // Generate branch starts
            for (int i = 0; i < this.BranchCount; i++)
            {
                if (!leftCrown.Any() && !rightCrown.Any())
                {
                    break;
                }

                int sign = this.rand.Next(-1, 1);
                var selectedCrown = sign < 0 ? leftCrown.Count > 0 ? leftCrown : rightCrown : rightCrown.Count > 0 ? rightCrown : leftCrown;

                var selectedBranches = sign < 0 ? leftCrown.Count > 0 ? leftBranches : rightBranches : rightCrown.Count > 0 ? rightBranches : leftBranches;

                var selectedCrownList = selectedCrown.ToList();
                int newBranch = selectedCrownList[this.rand.Next(0, selectedCrownList.Count)];

                if (selectedCrown.Contains(newBranch))
                {
                    selectedBranches.Add(newBranch);
                    for (int j = newBranch - branchDistance; j < newBranch + branchDistance; j++)
                    {
                        selectedCrown.Remove(j);
                    }
                }
            }
        }

        private void GenerateBranch(TreeModel tree, Vector2D growDirection, Point2D currentPoint, Point2D connectPoint, BranchType branchType, double width, int level = 1)
        {
            int angle = this.BranchSkew + rand.Next(-this.BranchSkewDeviation, this.BranchSkewDeviation);

            if (branchType == BranchType.Left)
            {
                this.GrowBranch(tree, growDirection, currentPoint, connectPoint, -angle, BranchType.Left, width, level);
            }

            if (branchType == BranchType.Right)
            {
                this.GrowBranch(tree, growDirection, currentPoint, connectPoint, angle, BranchType.Right, width, level);
            }
        }

        private void GrowBranch(TreeModel tree, Vector2D growDirection, Point2D currentPoint, Point2D connectPoint, double angle, BranchType branchType, double width, int level)
        {
            if (level > this.BranchMaxLevel)
            {
                return;
            }

            var t = ((level - 1) / (double)(this.BranchMaxLevel - 1));
            var lengthFactor = (1 + (this.BranchLevelLengthFactor - 1) * t);
            int branchLength = rand.Next(this.BranchLengthMin, this.BranchLengthMax + 1);
            branchLength = (int)(branchLength * lengthFactor);
            branchLength = Math.Max(branchLength, 1);
            int branchRotationAngleStart = this.BranchRotationAngleStart;

            float rotationStep = (float)this.BranchRotationAngle / (float)(branchLength - branchRotationAngleStart);
            rotationStep = branchType == BranchType.Right ? -rotationStep : rotationStep;

            HashSet<int> leftBranches;
            HashSet<int> rightBranches;
            this.GenerateBranchPositions(this.BranchStart, branchLength, this.BranchDistance / 2, out leftBranches, out rightBranches);

            var branch = new Branch();
            var branchStartWidth = Math.Max(width * 0.8, this.TrunkWidthEnd);
            var branchEndWidth = this.TrunkWidthEnd;
            growDirection = growDirection.Rotate(new Angle(angle, new Degrees()));
            branch.SkelletonPoints.Add(new TreePoint(new Point2D(Math.Round(currentPoint.X), Math.Round(currentPoint.Y)), growDirection) { Width = (int)branchStartWidth });
            branch.ParentBranchConnectPoint = connectPoint;                   

            for (int y = 0; y < branchLength; y++)
            {
                if (y > branchRotationAngleStart)
                {
                    growDirection = growDirection.Rotate(new Angle(rotationStep, new Degrees()));
                }
                
                currentPoint += growDirection;
                var currentWidth = branchEndWidth + (branchStartWidth - branchEndWidth) * ((branchLength - y) / (double)branchLength);
                branch.SkelletonPoints.Add(new TreePoint(new Point2D(Math.Round(currentPoint.X), Math.Round(currentPoint.Y)), growDirection) { Width = (int)currentWidth });
                
                if (y > BranchStart)
                {
                    int connectIndex = Math.Max(0, y - 20);
                    if (leftBranches.Contains(y))
                    {
                        this.GenerateBranch(tree, growDirection, currentPoint, branch.SkelletonPoints[connectIndex].Position, BranchType.Left, currentWidth, level + 1);
                    }

                    if (rightBranches.Contains(y))
                    {
                        this.GenerateBranch(tree, growDirection, currentPoint, branch.SkelletonPoints[connectIndex].Position, BranchType.Right, currentWidth, level + 1);
                    }
                }
            }

            tree.Branches.Add(branch);
        }

        public void ReDrawTree()
        {
            if (this.Tree != null)
            {
                this.DrawTree(this.Tree);
            }
        }

        public void DrawTree(TreeModel tree)
        {
            var xOffset = -tree.ContourPoints.Min(point => (int)point.X);
            var yOffset = -tree.ContourPoints.Min(point => (int)point.Y);
            var imageWidth = tree.ContourPoints.Max(point => (int)point.X) - tree.ContourPoints.Min(point => (int)point.X) + 1;
            var imageHeight = tree.ContourPoints.Max(point => (int)point.Y) - tree.ContourPoints.Min(point => (int)point.Y) + 1;

            var width = imageWidth;
            var height = imageHeight;

            using (var treeBitmap = new Bitmap(width, height, PixelFormat.Format32bppPArgb))
            using (var skelettonBitmap = new Bitmap(width, height, PixelFormat.Format32bppPArgb))
            {
                var dataTree = treeBitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, treeBitmap.PixelFormat);
                var dataSkeletton = skelettonBitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, skelettonBitmap.PixelFormat);
                using (var surfaceTree = SKSurface.Create(width, height, SKImageInfo.PlatformColorType, SKAlphaType.Premul, dataTree.Scan0, width * 4))
                using (var surfaceSkeletton = SKSurface.Create(width, height, SKImageInfo.PlatformColorType, SKAlphaType.Premul, dataSkeletton.Scan0, width * 4))
                {
                    // Move origin to left bottom.
                    surfaceTree.Canvas.Translate(0, height);
                    surfaceTree.Canvas.Scale(1, -1);
                    surfaceSkeletton.Canvas.Translate(0, height);
                    surfaceSkeletton.Canvas.Scale(1, -1);

                    SKPaint paint = new SKPaint() { Color = SKColors.Black, StrokeWidth = 1, Style = SKPaintStyle.Stroke };
                    SKPaint paintBrown = new SKPaint() { Color = SKColors.SaddleBrown, StrokeWidth = 1, Style = SKPaintStyle.Stroke };
                    var skelettonPath = new SKPath();
                    var treeBranches = tree.Branches.Concat(new[] { tree.Trunk }).ToList();
                    foreach (var branch in treeBranches)
                    {
                        foreach (var point in branch.SkelletonPoints)
                        {
                            surfaceSkeletton.Canvas.DrawPoint((float)point.Position.X + xOffset, (float)point.Position.Y + yOffset, paint);
                        }
                    }

                    if (!this.DebugModeEnabled || this.DebugViewModel.DrawTrunk)
                    {
                        this.DrawBranch(surfaceTree.Canvas, tree.Trunk, xOffset, yOffset, this.trunkColor, this.outlineColor);
                    }

                    var reversedBranches = tree.Branches.ToList();
                    reversedBranches.Reverse();
                    for (int i = 0; i < reversedBranches.Count; i++)
                    {
                        if (this.DebugModeEnabled && i != this.DebugViewModel.BranchToDraw)
                        {
                            continue;
                        }

                        this.DrawBranch(surfaceTree.Canvas, reversedBranches[i], xOffset, yOffset, this.trunkColor, this.outlineColor);
                    }
                }

                treeBitmap.UnlockBits(dataTree);
                this.ImageTree = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(treeBitmap.GetHbitmap(), IntPtr.Zero, System.Windows.Int32Rect.Empty, BitmapSizeOptions.FromWidthAndHeight(width, height));
                skelettonBitmap.UnlockBits(dataTree);
                this.ImageSkeletton = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(skelettonBitmap.GetHbitmap(), IntPtr.Zero, System.Windows.Int32Rect.Empty, BitmapSizeOptions.FromWidthAndHeight(width, height));
            }
        }

        private void DrawBranch(SKCanvas canvas, Branch branch, int xOffset, int yOffset, SKColor color, SKColor outlineColor)
        {
            var outlinePoints = branch.PolygonPoints.Select(point => new SKPoint((int)point.X + xOffset, (int)point.Y + yOffset)).ToList();
            var polygonPoints = outlinePoints.ToList();
            polygonPoints.Add(polygonPoints[0]);

            var maxDistance = branch.SDF.Values.Max();
            float vReduce = 0.8f;
            for (int i = 1; i <= maxDistance; i++)
            {
                color.ToHsv(out float h, out float s, out float v);
                var reduceFactor = 1 - (vReduce * ((maxDistance - i) / (float)maxDistance));

                SKColor colorForDistance = SKColor.FromHsv(h, s, v * reduceFactor);
                SKPaint fillForDistance = new SKPaint { Color = colorForDistance, Style = SKPaintStyle.Fill };
                var pointsWithDistance = branch.SDF.Where(sdfPoint => sdfPoint.Value == i).Select(sdfPoint => new SKPoint((float)sdfPoint.Key.X + xOffset, (float)sdfPoint.Key.Y + yOffset)).ToArray();
                canvas.DrawPoints(SKPointMode.Points, pointsWithDistance, fillForDistance);
            }


            SKPaint outline = new SKPaint { Color = outlineColor, Style = SKPaintStyle.Stroke };
            canvas.DrawPoints(SKPointMode.Points, branch.ContourPointsWithoutBot.Select(point => new SKPoint((float)point.X + xOffset, (float)point.Y + yOffset)).ToArray(), outline);
            //canvas.DrawPoints(SKPointMode.Points, branch.BotContourPoints.Select(point => new SKPoint((float)point.X + xOffset, (float)point.Y + yOffset)).ToArray(), outline);
        }
    }
}