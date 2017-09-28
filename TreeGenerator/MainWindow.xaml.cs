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


        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var bitmap = BitmapFactory.New(mainViewModel.ImageWidth, mainViewModel.ImageHeight);
            mainViewModel.Image = bitmap;

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

                int treeCrownSize = mainViewModel.TreeTrunkSize - mainViewModel.BranchStart;
                int halfBranchDistance = mainViewModel.BranchDistance / 2;

                HashSet<int> leftBranches;
                HashSet<int> rightBranches;
                this.GenerateBranchPositions(this.mainViewModel.BranchStart, treeCrownSize, halfBranchDistance, out leftBranches, out rightBranches);

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

                    currentPoint -= growDirection;
                    bitmap.SetPixel((int)currentPoint.X, (int)currentPoint.Y, Colors.Black);

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

        private void GenerateBranchPositions(int branchStart, int treeCrownSize, int halfBranchDistance, out HashSet<int> leftBranches, out HashSet<int> rightBranches)
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
                    for (int j = newBranch - halfBranchDistance; j < newBranch + halfBranchDistance; j++)
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

            List<int> subBranchPositions = new List<int>();
            HashSet<int> leftBranches;
            HashSet<int> rightBranches;
            this.GenerateBranchPositions(this.mainViewModel.BranchStart, branchLength, this.mainViewModel.BranchDistance / 2, out leftBranches, out rightBranches);

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
                bitmap.SetPixel((int)currentPoint.X, (int)currentPoint.Y, Colors.Black);

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

        }
    }
}
