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

        private TreeModel tree;


        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //try
            //{
            this.tree = new TreeModel();
            this.GenerateTree();
            this.DrawTree(this.tree);
            //}
            //catch (Exception exception)
            //{
            //    MessageBox.Show(exception.Message);
            //}
        }

        private void GenerateTree()
        {
            var bitmap = BitmapFactory.New(mainViewModel.ImageWidth, mainViewModel.ImageHeight);
            mainViewModel.ImageSkeletton = bitmap;

            var bitmap2 = BitmapFactory.New(mainViewModel.ImageWidth, mainViewModel.ImageHeight);
            mainViewModel.ImageTree = bitmap2;

            using (bitmap.GetBitmapContext())
            {
                Vector growDirection = new Vector(0, 1);

                float rotationStep = (float)mainViewModel.RotationAngle / (float)(mainViewModel.TreeTrunkSize - mainViewModel.RotationAngleStart);

                Matrix skewMatrix = Matrix.Identity;
                skewMatrix.Rotate(mainViewModel.SkewAngle);

                Matrix rotationMatrix = Matrix.Identity;
                rotationMatrix.Rotate(rotationStep);

                var trunkStartWidth = 5;
                var trunkEndWidth = 2;

                Vector currentPoint = new Vector(mainViewModel.ImageWidth / 2, (mainViewModel.ImageHeight - 1));
                bitmap.SetPixel((int)currentPoint.X, (int)currentPoint.Y, Colors.Black);
                this.tree.Trunk.Add(new TreePoint(currentPoint, growDirection) { Width = trunkStartWidth});

                int treeCrownSize = mainViewModel.TreeTrunkSize - mainViewModel.BranchStart;

                HashSet<int> leftBranches;
                HashSet<int> rightBranches;
                this.GenerateBranchPositions(this.mainViewModel.BranchStart, treeCrownSize, mainViewModel.BranchDistance, out leftBranches, out rightBranches);

                for (int y = 0; y < mainViewModel.TreeTrunkSize - 1; y++)
                {
                    if (y == mainViewModel.SkewAngleStart)
                    {
                        growDirection = skewMatrix.Transform(growDirection);
                    }

                    if (y > mainViewModel.RotationAngleStart)
                    {
                        growDirection = rotationMatrix.Transform(growDirection);
                    }

                    // Generate trunk.
                    var currentWidth = trunkEndWidth + (trunkStartWidth - trunkEndWidth) * ((mainViewModel.TreeTrunkSize - y) / (double)mainViewModel.TreeTrunkSize);
                    currentPoint -= growDirection;
                    bitmap.SetPixelSafe((int)currentPoint.X, (int)currentPoint.Y, Colors.Black);
                    this.tree.Trunk.Add(new TreePoint(currentPoint, growDirection) { Width = (int)currentWidth });

                    // Generate branches.
                    if (y > mainViewModel.BranchStart)
                    {
                        if (leftBranches.Contains(y))
                        {
                            this.GenerateBranch(bitmap, growDirection, currentPoint, BranchType.Left, currentWidth);
                        }

                        if (rightBranches.Contains(y))
                        {
                            this.GenerateBranch(bitmap, growDirection, currentPoint, BranchType.Right, currentWidth);
                        }
                    }
                }
            }
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

        private void GenerateBranch(WriteableBitmap bitmap, Vector growDirection, Vector currentPoint, BranchType branchType, double width, int level = 1)
        {
            int angle = this.mainViewModel.BranchSkew + rand.Next(-this.mainViewModel.BranchSkewDeviation, this.mainViewModel.BranchSkewDeviation);
            this.leftSkewMatrix = Matrix.Identity;
            this.leftSkewMatrix.Rotate(-angle);
            this.rightSkewMatrix = Matrix.Identity;
            this.rightSkewMatrix.Rotate(angle);

            if (branchType == BranchType.Left)
            {
                this.GrowBranch(bitmap, growDirection, currentPoint, leftSkewMatrix, BranchType.Left, width, level);
            }

            if (branchType == BranchType.Right)
            {
                this.GrowBranch(bitmap, growDirection, currentPoint, rightSkewMatrix, BranchType.Right, width, level);
            }
        }

        private void GrowBranch(WriteableBitmap bitmap, Vector growDirection, Vector currentPoint, Matrix skewMatrix, BranchType branchType, double width, int level)
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
            var branchEndWidth = 1;
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

                currentPoint -= growDirection;
                var currentWidth = branchEndWidth + (branchStartWidth - branchEndWidth) * ((branchLength - y) / (double)branchLength);
                bitmap.SetPixelSafe((int)currentPoint.X, (int)currentPoint.Y, Colors.Black);
                branch.BranchPoints.Add(new TreePoint(currentPoint, growDirection) {Width = (int)currentWidth});

                if (y > mainViewModel.BranchStart)
                {
                    if (leftBranches.Contains(y))
                    {
                        this.GenerateBranch(bitmap, growDirection, currentPoint, BranchType.Left, currentWidth, level + 1);
                    }

                    if (rightBranches.Contains(y))
                    {
                        this.GenerateBranch(bitmap, growDirection, currentPoint, BranchType.Right, currentWidth, level + 1);
                    }
                }
            }

            this.tree.Branches.Add(branch);
        }

        private void DrawTree(TreeModel tree)
        {
            var treeImage = this.mainViewModel.ImageTree;
            Matrix rotation90RightMatrix = Matrix.Identity;
            rotation90RightMatrix.Rotate(90);

            // Draw trunk.
            int trunkArrayLength = (tree.Trunk.Count) * 4 + 2;
            var trunkArray = new int[trunkArrayLength];
            for (int i = 0; i < tree.Trunk.Count; i++)
            {
                var trunkElement = tree.Trunk[i];
                var trunkElementRightDirection = rotation90RightMatrix.Transform(trunkElement.GrowDirection);
                var resizePointLeft = -trunkElement.Width * trunkElementRightDirection + trunkElement.Position;
                var resizePointRight = trunkElement.Width * trunkElementRightDirection + trunkElement.Position;

                trunkArray[i * 2] = (int)resizePointLeft.X;
                trunkArray[i * 2 + 1] = (int)resizePointLeft.Y;

                int reverseIndex = (tree.Trunk.Count) * 4 - 2 - i * 2;
                trunkArray[reverseIndex] = (int)resizePointRight.X;
                trunkArray[reverseIndex + 1] = (int)resizePointRight.Y;
            }

            trunkArray[trunkArrayLength - 2] = trunkArray[0];
            trunkArray[trunkArrayLength - 1] = trunkArray[1];
            treeImage.FillPolygon(trunkArray, Colors.SaddleBrown);
            treeImage.DrawPolyline(trunkArray.Take(trunkArray.Length - 2).ToArray(), Colors.Black);

            // Draw branches.
            foreach (var branch in tree.Branches)
            {
                int branchArrayLength = (branch.BranchPoints.Count) * 4 + 2;
                var branchArray = new int[branchArrayLength];

                for (int i = 0; i < branch.BranchPoints.Count; i++)
                {
                    var branchElement = branch.BranchPoints[i];
                    var branchElementRightDirection = rotation90RightMatrix.Transform(branchElement.GrowDirection);
                    var resizePointLeft = -branchElement.Width * branchElementRightDirection + branchElement.Position;
                    var resizePointRight = branchElement.Width * branchElementRightDirection + branchElement.Position;

                    branchArray[i * 2] = (int)resizePointLeft.X;
                    branchArray[i * 2 + 1] = (int)resizePointLeft.Y;

                    int reverseIndex = (branch.BranchPoints.Count) * 4 - 2 - i * 2;
                    branchArray[reverseIndex] = (int)resizePointRight.X;
                    branchArray[reverseIndex + 1] = (int)resizePointRight.Y;
                }

                branchArray[branchArrayLength - 2] = branchArray[0];
                branchArray[branchArrayLength - 1] = branchArray[1];
                treeImage.FillPolygon(branchArray, Colors.SaddleBrown);
                treeImage.DrawPolyline(branchArray.Take(branchArray.Length - 2).ToArray(), Colors.Black);

                //treeImage.SetPixelSafe((int)branchElement.Position.X, (int)branchElement.Position.Y, Colors.Black);

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
        public static void SetPixelSafe(this WriteableBitmap bitmap, int x, int y, Color color)
        {
            if (x < bitmap.Width && x > 0 && y < bitmap.Height && y > 0)
            {
                bitmap.SetPixel(x, y, color);
            }
        }
    }
}
