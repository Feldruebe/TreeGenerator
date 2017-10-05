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
    public partial class MainWindow : Window
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
            try
            {
                this.tree = new TreeModel();
                this.GenerateTree();
                this.DrawTree(this.tree);
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
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

                Vector currentPoint = new Vector(mainViewModel.ImageWidth / 2, (mainViewModel.ImageHeight - 1));
                bitmap.SetPixel((int)currentPoint.X, (int)currentPoint.Y, Colors.Black);
                this.tree.Trunk.Add(new TreePoint(currentPoint, growDirection));

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
                    currentPoint -= growDirection;
                    bitmap.SetPixelSafe((int)currentPoint.X, (int)currentPoint.Y, Colors.Black);
                    this.tree.Trunk.Add(new TreePoint(currentPoint, growDirection));

                    // Generate branches.
                    if (y > mainViewModel.BranchStart)
                    {
                        if (leftBranches.Contains(y))
                        {
                            this.GenerateBranch(bitmap, growDirection, currentPoint, BranchType.Left);
                        }

                        if (rightBranches.Contains(y))
                        {
                            this.GenerateBranch(bitmap, growDirection, currentPoint, BranchType.Right);
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

        private void GenerateBranch(WriteableBitmap bitmap, Vector growDirection, Vector currentPoint, BranchType branchType, int level = 1)
        {
            int angle = this.mainViewModel.BranchSkew + rand.Next(-this.mainViewModel.BranchSkewDeviation, this.mainViewModel.BranchSkewDeviation);
            this.leftSkewMatrix = Matrix.Identity;
            this.leftSkewMatrix.Rotate(-angle);
            this.rightSkewMatrix = Matrix.Identity;
            this.rightSkewMatrix.Rotate(angle);

            if (branchType == BranchType.Left)
            {
                this.GrowBranch(bitmap, growDirection, currentPoint, leftSkewMatrix, BranchType.Left, level);
            }

            if (branchType == BranchType.Right)
            {
                this.GrowBranch(bitmap, growDirection, currentPoint, rightSkewMatrix, BranchType.Right, level);
            }
        }

        private void GrowBranch(WriteableBitmap bitmap, Vector growDirection, Vector currentPoint, Matrix skewMatrix, BranchType branchType, int level)
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
                bitmap.SetPixelSafe((int)currentPoint.X, (int)currentPoint.Y, Colors.Black);
                branch.BranchPoints.Add(new TreePoint(currentPoint, growDirection));

                if (y > mainViewModel.BranchStart)
                {
                    if (leftBranches.Contains(y))
                    {
                        this.GenerateBranch(bitmap, growDirection, currentPoint, BranchType.Left, level + 1);
                    }

                    if (rightBranches.Contains(y))
                    {
                        this.GenerateBranch(bitmap, growDirection, currentPoint, BranchType.Right, level + 1);
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
            List<int> trunkPoints = new List<int>();// { 10, 10, 30, 30, 30, 80, 10, 10 };
            var trunkWidth = 5;
            foreach (var trunkElement in tree.Trunk)
            {
                var trunkElementRightDirection = rotation90RightMatrix.Transform(trunkElement.GrowDirection);
                var resizePoint = -trunkWidth * trunkElementRightDirection + trunkElement.Position;
                trunkPoints.Add((int)resizePoint.X);
                trunkPoints.Add((int)resizePoint.Y);
            }

            foreach (var trunkElement in tree.Trunk.ToArray().Reverse())
            {
                var trunkElementRightDirection = rotation90RightMatrix.Transform(trunkElement.GrowDirection);
                var resizePoint = trunkWidth * trunkElementRightDirection + trunkElement.Position;
                trunkPoints.Add((int)resizePoint.X);
                trunkPoints.Add((int)resizePoint.Y);
            }

            trunkPoints.Add(trunkPoints[0]);
            trunkPoints.Add(trunkPoints[1]);
            treeImage.FillPolygon(trunkPoints.ToArray(), Colors.SaddleBrown);

            // Draw branches.
            var branchWidth = 2;
            foreach (var branch in tree.Branches)
            {
                List<int> branchPoints = new List<int>();// { 10, 10, 30, 30, 30, 80, 10, 10 };

                foreach (var trunkElement in branch.BranchPoints)
                {
                    var branchElementRightDirection = rotation90RightMatrix.Transform(trunkElement.GrowDirection);
                    var resizePoint = -branchWidth * branchElementRightDirection + trunkElement.Position;
                    branchPoints.Add((int)resizePoint.X);
                    branchPoints.Add((int)resizePoint.Y);
                }

                foreach (var trunkElement in branch.BranchPoints.ToArray().Reverse())
                {
                    var branchElementRightDirection = rotation90RightMatrix.Transform(trunkElement.GrowDirection);
                    var resizePoint = branchWidth * branchElementRightDirection + trunkElement.Position;
                    branchPoints.Add((int)resizePoint.X);
                    branchPoints.Add((int)resizePoint.Y);
                }

                branchPoints.Add(branchPoints[0]);
                branchPoints.Add(branchPoints[1]);
                treeImage.FillPolygon(branchPoints.ToArray(), Colors.SaddleBrown);

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
