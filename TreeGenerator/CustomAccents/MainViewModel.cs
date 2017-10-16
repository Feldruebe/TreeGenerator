namespace TreeGenerator.ViewModels
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
    using System.Windows.Media;

    using MathNet.Numerics.LinearAlgebra;
    using MathNet.Numerics.LinearAlgebra.Double;
    using MathNet.Spatial.Euclidean;
    using MathNet.Spatial.Units;

    using Microsoft.Win32;

    using SkiaSharp;

    using Color = System.Windows.Media.Color;
    using PixelFormat = System.Drawing.Imaging.PixelFormat;

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
        private Random rand = new Random();

        private int treeTrunkSize;
        private WriteableBitmap imageSkeletton;
        private WriteableBitmap imageTree;

        private BitmapSource skiaImage;

        private SKCanvas skCanvas;

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
        private Color trunkColor;
        private Color branchColor;
        private Color outlineColor;
        private bool isColorFlyoutOpen = false;

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {
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

            var tree = this.GenerateTree();
            this.DrawTree(tree);
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

        public WriteableBitmap ImageSkeletton
        {
            get { return this.imageSkeletton; }
            set { this.Set(ref this.imageSkeletton, value); }
        }

        public WriteableBitmap ImageTree
        {
            get { return this.imageTree; }
            set { this.Set(ref this.imageTree, value); }
        }

        public BitmapSource SkiaImage
        {
            get
            {
                return this.skiaImage;
            }

            set
            {
                this.Set(ref this.skiaImage, value);
            }
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
            get { return this.trunkSkewAngleStart; }
            set { this.Set(ref this.trunkSkewAngleStart, value); }
        }

        public int BranchCount
        {
            get { return this.branchCount; }
            set { this.Set(ref this.branchCount, value); }
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
                return this.trunkColor;
            }

            set
            {
                Set(ref this.trunkColor, value);
            }
        }

        public Color BranchColor
        {
            get
            {
                return this.branchColor;
            }

            set
            {
                this.Set(ref this.branchColor, value);
            }
        }

        public Color OutlineColor
        {
            get
            {
                return this.outlineColor;
            }

            set
            {
                this.Set(ref this.outlineColor, value);
                this.RaisePropertyChanged();
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

        public Color BranchOutlineColor => this.OutlineColor;

        public RelayCommand GenerateTreeCommand { get; set; }

        public RelayCommand ExportImageCommand { get; set; }

        private void GenerateTreeAndDraw()
        {
            //try
            //{
            var tree = this.GenerateTree();
            this.DrawTree(tree);
            //}
            //catch (Exception exception)
            //{
            //    MessageBox.Show(exception.Message);
            //}
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
            tree.Trunk.BranchPoints.Add(new TreePoint(currentPoint, growDirection) { Width = trunkStartWidth });

            int treeCrownSize = this.TreeTrunkSize - this.BranchStart;

            HashSet<int> leftBranches;
            HashSet<int> rightBranches;
            this.GenerateBranchPositions(this.BranchStart, treeCrownSize, this.BranchDistance, out leftBranches, out rightBranches);

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
                tree.Trunk.BranchPoints.Add(new TreePoint(currentPoint, growDirection) { Width = (int)currentWidth });

                // Generate branches.
                if (y > this.BranchStart)
                {
                    if (leftBranches.Contains(y))
                    {
                        this.GenerateBranch(tree, growDirection, currentPoint, BranchType.Left, currentWidth);
                    }

                    if (rightBranches.Contains(y))
                    {
                        this.GenerateBranch(tree, growDirection, currentPoint, BranchType.Right, currentWidth);
                    }
                }
            }

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

        private void GenerateBranch(TreeModel tree, Vector2D growDirection, Point2D currentPoint, BranchType branchType, double width, int level = 1)
        {
            int angle = this.BranchSkew + rand.Next(-this.BranchSkewDeviation, this.BranchSkewDeviation);

            if (branchType == BranchType.Left)
            {
                this.GrowBranch(tree, growDirection, currentPoint, -angle, BranchType.Left, width, level);
            }

            if (branchType == BranchType.Right)
            {
                this.GrowBranch(tree, growDirection, currentPoint, angle, BranchType.Right, width, level);
            }
        }

        private void GrowBranch(TreeModel tree, Vector2D growDirection, Point2D currentPoint, double angle, BranchType branchType, double width, int level)
        {
            if (level == 4)
            {
                return;
            }

            int branchLength = Math.Max(rand.Next(this.BranchLengthMin, this.BranchLengthMax + 1) / level, 1);
            int branchRotationAngleStart = this.BranchRotationAngleStart;

            float rotationStep = (float)this.BranchRotationAngle / (float)(branchLength - branchRotationAngleStart);
            rotationStep = branchType == BranchType.Right ? -rotationStep : rotationStep;

            HashSet<int> leftBranches;
            HashSet<int> rightBranches;
            this.GenerateBranchPositions(this.BranchStart, branchLength, this.BranchDistance / 2, out leftBranches, out rightBranches);

            var branch = new Branch();
            var branchStartWidth = width;
            var branchEndWidth = 2;
            for (int y = 0; y < branchLength; y++)
            {
                if (y == 0)
                {
                    growDirection = growDirection.Rotate(new Angle(angle, new Degrees()));
                }

                if (y > branchRotationAngleStart)
                {
                    growDirection = growDirection.Rotate(new Angle(rotationStep, new Degrees()));
                }

                currentPoint += growDirection;
                var currentWidth = branchEndWidth + (branchStartWidth - branchEndWidth) * ((branchLength - y) / (double)branchLength);
                branch.BranchPoints.Add(new TreePoint(currentPoint, growDirection) { Width = (int)currentWidth });

                if (y > BranchStart)
                {
                    if (leftBranches.Contains(y))
                    {
                        this.GenerateBranch(tree, growDirection, currentPoint, BranchType.Left, currentWidth, level + 1);
                    }

                    if (rightBranches.Contains(y))
                    {
                        this.GenerateBranch(tree, growDirection, currentPoint, BranchType.Right, currentWidth, level + 1);
                    }
                }
            }

            tree.Branches.Add(branch);
        }

        private void DrawTree(TreeModel tree)
        {
            var xOffset = -tree.AllBorderPoints.Min(point => (int)point.X);
            var yOffset = -tree.AllBorderPoints.Min(point => (int)point.Y);
            var borderWidth = tree.AllBorderPoints.Max(point => (int)Math.Ceiling(point.X)) - tree.AllBorderPoints.Min(point => (int)Math.Ceiling(point.X)) + 1;
            var imageWidth = Math.Max(borderWidth, (int)Math.Ceiling(this.TrunkWidthStart * 0.5));
            var imageHeight = tree.AllBorderPoints.Max(point => (int)Math.Abs(point.Y)) + 1;

            var bitmap = BitmapFactory.New(imageWidth, imageHeight);
            this.ImageSkeletton = bitmap;

            var bitmap2 = BitmapFactory.New(imageWidth, imageHeight);
            this.ImageTree = bitmap2;


            var width = imageWidth;
            var height = imageHeight;

            using (var skiaBitmap = new Bitmap(width, height, PixelFormat.Format32bppPArgb))
            {
                var data = skiaBitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, skiaBitmap.PixelFormat);
                using (var surface = SKSurface.Create(width, height, SKImageInfo.PlatformColorType, SKAlphaType.Premul, data.Scan0, width * 4))
                {
                    this.skCanvas = surface.Canvas;
                    // DoDraw (skcanvas);


                    using (this.ImageSkeletton.GetBitmapContext())
                    {
                        foreach (var point in tree.AllTreePoints)
                        {
                            this.ImageSkeletton.SetPixelSafeLeftBot((int)point.Position.X + xOffset, (int)point.Position.Y + yOffset, Colors.Black);
                        }
                    }

                    var treeImage = this.ImageTree;
                    using (treeImage.GetBitmapContext())
                    {
                        this.DrawBranch(this.ImageTree, tree.Trunk, xOffset, yOffset, this.TrunkColor, this.OutlineColor);

                        tree.Branches.Reverse();
                        // Draw branches.
                        foreach (var branch in tree.Branches)
                        {
                            this.DrawBranch(this.ImageTree, branch, xOffset, yOffset, this.BranchColor, this.BranchOutlineColor);
                        }
                    }

                    //List<Point2D> conture = new List<Point2D>();
                    //Polygon2D polygon = new Polygon2D(tree.Trunk.PolygonPoints.ToList());
                    //for (int i = -xOffset; i < imageWidth - xOffset; i++)
                    //{
                    //    for (int j = -yOffset; j < imageHeight - yOffset; j++)
                    //    {
                    //        if (this.IsConturePoint(new Point2D(i, j), polygon))
                    //        {
                    //            conture.Add(new Point2D(i, j));
                    //        }
                    //    }
                    //}

                    //this.CalculateSDF(tree.Trunk);

                }

                skiaBitmap.UnlockBits(data);
                this.SkiaImage = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(skiaBitmap.GetHbitmap(), IntPtr.Zero, System.Windows.Int32Rect.Empty, BitmapSizeOptions.FromWidthAndHeight(width, height));
            }
        }

        private void CalculateSDF(Branch branch)
        {
            int disctance = 0;
            var currentPoints = branch.PolygonPoints;
            var nextpoints = new HashSet<Point2D>();

            Polygon2D polygon = new Polygon2D(branch.PolygonPoints.ToList());

            while (currentPoints.Any())
            {
                foreach (var point in currentPoints)
                {
                    var target = point + new Vector2D(1d, 0d);
                    if (this.IsInPolygon(target, polygon) && this.IsNotInSDF(target, branch.SDF))
                    {
                        branch.SDF[target] = disctance + 1;
                        nextpoints.Add(target);
                    }

                    target = point + new Vector2D(-1, 0);
                    if (this.IsInPolygon(target, polygon) && this.IsNotInSDF(target, branch.SDF))
                    {
                        branch.SDF[target] = disctance + 1;
                        nextpoints.Add(target);
                    }

                    target = point + new Vector2D(0, 1);
                    if (this.IsInPolygon(target, polygon) && this.IsNotInSDF(target, branch.SDF))
                    {
                        branch.SDF[target] = disctance + 1;
                        nextpoints.Add(target);
                    }

                    target = point + new Vector2D(0, -1);
                    if (this.IsInPolygon(target, polygon) && this.IsNotInSDF(target, branch.SDF))
                    {
                        branch.SDF[target] = disctance + 1;
                        nextpoints.Add(target);
                    }
                }

                disctance++;
                currentPoints = nextpoints.ToList();
                nextpoints.Clear();
            }
        }

        private bool IsNotInSDF(Point2D target, Dictionary<Point2D, int> sdf)
        {
            return !sdf.ContainsKey(target);
        }

        private bool IsInPolygon(Point2D target, Polygon2D polygon)
        {
            if (polygon.EnclosesPoint(target) || polygon.Contains(target))
            {
                return true;
            }

            return false;
        }

        private bool IsConturePoint(Point2D point, Polygon2D polygon)
        {
            var isInside = this.IsInPolygon(point, polygon);
            var isLeftInside = this.IsInPolygon(point + new Vector2D(-1, 0), polygon);
            var isRightInside = this.IsInPolygon(point + new Vector2D(1, 0), polygon);
            var isTopInside = this.IsInPolygon(point + new Vector2D(0, 1), polygon);
            var isBotInside = this.IsInPolygon(point + new Vector2D(0, -1), polygon);

            if (isInside && (!isLeftInside || !isRightInside || !isTopInside || !isBotInside))
            {
                return true;
            }

            return false;
        }

        private void DrawBranch(WriteableBitmap bitmap, Branch branch, int xOffset, int yOffset, Color color, Color outline)
        {
            var points = branch.PolygonPoints.SelectMany(point => new[] { (int)point.X + xOffset, (int)point.Y + yOffset }).ToList();
            var polygon = points.ToArray().ToList();
            polygon.Add(polygon[0]);
            polygon.Add(polygon[1]);
            bitmap.FillPolygonOriginLeftBot(polygon.ToArray(), color);
            bitmap.DrawPolylineOriginLeftBot(points.Take(points.Count).ToArray(), outline);

            var copy = points.Select(element => element).ToArray();
            for (int i = 1; i < points.ToArray().Length; i += 2)
            {
                copy[i] = (int)bitmap.Height - 1 - points[i];
            }

            SKPaint p = new SKPaint();
            p.Color = SKColors.SaddleBrown;
            var path = new SKPath();
            for (int i = 0; i < copy.Length; i+=2)
            {
                path.LineTo(copy[i], copy[i+1]);
            }

            this.skCanvas.DrawPath(path, p);
        }
    }
}