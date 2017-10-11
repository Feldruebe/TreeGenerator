using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TreeGenerator.ViewModels;
using System.Collections.Generic;
using System.Linq;

namespace TreeGenerator
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        MainViewModel mainViewModel = new ViewModelLocator().Main;
        Random rand = new Random();

        Matrix leftSkewMatrix = Matrix.Identity;
        Matrix rightSkewMatrix = Matrix.Identity;

        public MainWindow()
        {
            InitializeComponent();

            var tree = this.GenerateTree();
            this.DrawTree(tree);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var tree = this.GenerateTree();
                this.DrawTree(tree);
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }

        private TreeModel GenerateTree()
        {
            var tree = new TreeModel();

            Vector growDirection = new Vector(0, 1);

            float rotationStep = (float)mainViewModel.TrunkRotationAngle / (float)(mainViewModel.TreeTrunkSize - mainViewModel.TrunkRotationAngleStart);

            Matrix skewMatrix = Matrix.Identity;
            skewMatrix.Rotate(mainViewModel.TrunkSkewAngle);

            Matrix rotationMatrix = Matrix.Identity;
            rotationMatrix.Rotate(rotationStep);

            Vector currentPoint = new Vector(0, 0);
            var trunkStartWidth = mainViewModel.TrunkWidthStart ;
            var trunkEndWidth = mainViewModel.TrunkWidthEnd;
            tree.Trunk.Add(new TreePoint(currentPoint, growDirection) { Width = trunkStartWidth });

            int treeCrownSize = mainViewModel.TreeTrunkSize - mainViewModel.BranchStart;

            HashSet<int> leftBranches;
            HashSet<int> rightBranches;
            this.GenerateBranchPositions(this.mainViewModel.BranchStart, treeCrownSize, mainViewModel.BranchDistance, out leftBranches, out rightBranches);

            for (int y = 0; y < mainViewModel.TreeTrunkSize - 1; y++)
            {
                if (y == mainViewModel.TrunkSkewAngleStart)
                {
                    growDirection = skewMatrix.Transform(growDirection);
                }

                if (y > mainViewModel.TrunkRotationAngleStart)
                {
                    growDirection = rotationMatrix.Transform(growDirection);
                }

                // Generate trunk.
                var currentWidth = trunkEndWidth + (trunkStartWidth - trunkEndWidth) * ((mainViewModel.TreeTrunkSize - y) / (double)mainViewModel.TreeTrunkSize);
                currentPoint += growDirection;
                tree.Trunk.Add(new TreePoint(currentPoint, growDirection) { Width = (int)currentWidth });

                // Generate branches.
                if (y > mainViewModel.BranchStart)
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
            for (int i = 0; i < this.mainViewModel.BranchCount; i++)
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

        private void GenerateBranch(TreeModel tree, Vector growDirection, Vector currentPoint, BranchType branchType, double width, int level = 1)
        {
            int angle = this.mainViewModel.BranchSkew + rand.Next(-this.mainViewModel.BranchSkewDeviation, this.mainViewModel.BranchSkewDeviation);
            this.leftSkewMatrix = Matrix.Identity;
            this.leftSkewMatrix.Rotate(-angle);
            this.rightSkewMatrix = Matrix.Identity;
            this.rightSkewMatrix.Rotate(angle);

            if (branchType == BranchType.Left)
            {
                this.GrowBranch(tree, growDirection, currentPoint, leftSkewMatrix, BranchType.Left, width, level);
            }

            if (branchType == BranchType.Right)
            {
                this.GrowBranch(tree, growDirection, currentPoint, rightSkewMatrix, BranchType.Right, width, level);
            }
        }

        private void GrowBranch(TreeModel tree, Vector growDirection, Vector currentPoint, Matrix skewMatrix, BranchType branchType, double width, int level)
        {
            if (level == 4)
            {
                return;
            }

            int branchLength = rand.Next(mainViewModel.BranchLengthMin, mainViewModel.BranchLengthMax + 1) / level;
            int branchRotationAngleStart = mainViewModel.BranchRotationAngleStart;

            float rotationStep = (float)mainViewModel.BranchRotationAngle / (float)(branchLength - branchRotationAngleStart);
            rotationStep = branchType == BranchType.Right ? -rotationStep : rotationStep;
            Matrix rotationMatrix = Matrix.Identity;
            rotationMatrix.Rotate(rotationStep);

            HashSet<int> leftBranches;
            HashSet<int> rightBranches;
            this.GenerateBranchPositions(this.mainViewModel.BranchStart, branchLength, this.mainViewModel.BranchDistance / 2, out leftBranches, out rightBranches);

            var branch = new Branch();
            var branchStartWidth = width;
            var branchEndWidth = 2;
            for (int y = 0; y < branchLength; y++)
            {
                if (y == 0)
                {
                    growDirection = skewMatrix.Transform(growDirection);
                }

                if (y > branchRotationAngleStart)
                {
                    growDirection = rotationMatrix.Transform(growDirection);
                }

                currentPoint += growDirection;
                var currentWidth = branchEndWidth + (branchStartWidth - branchEndWidth) * ((branchLength - y) / (double)branchLength);
                branch.BranchPoints.Add(new TreePoint(currentPoint, growDirection) { Width = (int)currentWidth });

                if (y > mainViewModel.BranchStart)
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
            var treePoints = tree.Trunk.Concat(tree.Branches.SelectMany(branch => branch.BranchPoints));
            var xOffset = -treePoints.Min(point => (int)point.Position.X) + (int)(this.mainViewModel.TrunkWidthStart * 0.5);
            var yOffset = 0;
            var imageWidth = treePoints.Max(point => (int)Math.Abs(point.Position.X)) * 2 + mainViewModel.TrunkWidthStart;
            var imageHeight = treePoints.Max(point => (int)Math.Abs(point.Position.Y)) + 2;

            var bitmap = BitmapFactory.New(imageWidth, imageHeight);
            mainViewModel.ImageSkeletton = bitmap;

            var bitmap2 = BitmapFactory.New(imageWidth, imageHeight);
            mainViewModel.ImageTree = bitmap2;

            using (this.mainViewModel.ImageSkeletton.GetBitmapContext())
            {
                foreach (var point in treePoints)
                {
                    this.mainViewModel.ImageSkeletton.SetPixelSafeLeftBot((int)point.Position.X + xOffset, (int)point.Position.Y, Colors.Black);
                }
            }

            var treeImage = this.mainViewModel.ImageTree;
            using (treeImage.GetBitmapContext())
            {
                Matrix rotation90RightMatrix = Matrix.Identity;
                rotation90RightMatrix.Rotate(90);

                // Draw trunk.
                int trunkArrayLength = (tree.Trunk.Count) * 4 + 2;
                var trunkArray = new int[trunkArrayLength];
                for (int i = 0; i < tree.Trunk.Count; i++)
                {
                    var trunkElement = tree.Trunk[i];
                    var trunkElementRightDirection = rotation90RightMatrix.Transform(trunkElement.GrowDirection);
                    var resizePointLeft = -trunkElement.Width * 0.5 * trunkElementRightDirection + trunkElement.Position;
                    var resizePointRight = trunkElement.Width * 0.5 * trunkElementRightDirection + trunkElement.Position;

                    trunkArray[i * 2] = (int)resizePointLeft.X + xOffset;
                    trunkArray[i * 2 + 1] = (int)resizePointLeft.Y - yOffset;

                    int reverseIndex = (tree.Trunk.Count) * 4 - 2 - i * 2;
                    trunkArray[reverseIndex] = (int)resizePointRight.X + xOffset;
                    trunkArray[reverseIndex + 1] = (int)resizePointRight.Y - yOffset;
                }

                trunkArray[trunkArrayLength - 2] = trunkArray[0];
                trunkArray[trunkArrayLength - 1] = trunkArray[1];
                treeImage.FillPolygonOriginLeftBot(trunkArray, this.mainViewModel.TrunkColor);
                treeImage.DrawPolylineOriginLeftBot(trunkArray.Take(trunkArray.Length - 2).ToArray(), this.mainViewModel.OutlineColor);

                // Draw branches.
                foreach (var branch in tree.Branches)
                {
                    int branchArrayLength = (branch.BranchPoints.Count) * 4 + 2;
                    var branchArray = new int[branchArrayLength];

                    for (int i = 0; i < branch.BranchPoints.Count; i++)
                    {
                        var branchElement = branch.BranchPoints[i];
                        var branchElementRightDirection = rotation90RightMatrix.Transform(branchElement.GrowDirection);
                        var resizePointLeft = -branchElement.Width * 0.5 * branchElementRightDirection + branchElement.Position;
                        var resizePointRight = branchElement.Width * 0.5 * branchElementRightDirection + branchElement.Position;

                        branchArray[i * 2] = (int)resizePointLeft.X + xOffset;
                        branchArray[i * 2 + 1] = (int)resizePointLeft.Y - yOffset;

                        int reverseIndex = (branch.BranchPoints.Count) * 4 - 2 - i * 2;
                        branchArray[reverseIndex] = (int)resizePointRight.X + xOffset;
                        branchArray[reverseIndex + 1] = (int)resizePointRight.Y - yOffset;
                    }

                    Color branchOutlineColor = this.mainViewModel.OutlineColor;
                    Color branchColor = this.mainViewModel.BranchColor;

                    branchArray[branchArrayLength - 2] = branchArray[0];
                    branchArray[branchArrayLength - 1] = branchArray[1];
                    treeImage.FillPolygonOriginLeftBot(branchArray, branchColor);
                    treeImage.DrawPolylineOriginLeftBot(branchArray.Take(branchArray.Length - 2).ToArray(), branchOutlineColor);
                }
            }
        }
    }

    class TreeModel
    {
        public List<TreePoint> Trunk { get; } = new List<TreePoint>();

        public List<Branch> Branches { get; } = new List<Branch>();
    }

    class Branch
    {
        public List<TreePoint> BranchPoints { get; set; } = new List<TreePoint>();
    }

    public class TreePoint
    {
        public TreePoint(Vector position, Vector growDirection)
        {
            this.Position = position;
            this.GrowDirection = growDirection;
        }

        public Vector Position { get; set; }

        public Vector GrowDirection { get; set; }

        public int Width { get; set; }
    }

    public static class BitmapExtensions
    {
        public static void SetPixelSafeLeftBot(this WriteableBitmap bitmap, int x, int y, Color color)
        {
            y = (int)bitmap.Height - 1 - y;

            if (x < bitmap.Width && x >= 0 && y <= bitmap.Height && y >= 0)
            {
                bitmap.SetPixel(x, y, color);
            }
        }

        public static void FillPolygonOriginLeftBot(this WriteableBitmap bitmap, int[] polygon, Color color)
        {
            var copy = polygon.Select(element => element).ToArray();
            for (int i = 1; i < polygon.Length; i += 2)
            {
                copy[i] = (int)bitmap.Height - 1 - polygon[i];
            }

            bitmap.FillPolygon(copy, WriteableBitmapExtensions.ConvertColor(color), true);
        }

        public static void DrawPolylineOriginLeftBot(this WriteableBitmap bitmap, int[] polyline, Color color)
        {
            var copy = polyline.Select(element => element).ToArray();
            for (int i = 1; i < polyline.Length; i += 2)
            {
                copy[i] = (int)bitmap.Height - 1 - polyline[i];

            }

            bitmap.DrawPolyline(copy, color);
        }
    }
}
