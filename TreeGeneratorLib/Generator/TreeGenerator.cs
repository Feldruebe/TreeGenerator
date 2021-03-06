﻿using System;
using System.Collections.Generic;
using System.Linq;
using TreeGeneratorLib.Tree;

namespace TreeGeneratorLib.Generator
{
    using MathNet.Spatial.Euclidean;
    using MathNet.Spatial.Units;

    using TreeGeneratorLib.Wrappers;

    using TreeGeneratorWPF.ViewModels;

    public class TreeGenerator<T> where T : TreeVisual
    {
        private Random rand;

        private ICancelableProgress progress;

        public void Initialize(TreeParameters parameters, ICancelableProgress progress, int? randomSeed = null)
        {
            this.TreeParameters = parameters;
            this.rand = randomSeed.HasValue ? new Random(randomSeed.Value) : new Random();
            this.progress = progress;
        }

        public TreeParameters TreeParameters { get; private set; }

        public TreeModel<T> GenerateTree()
        {
            return this.GenerateTreeInternal();
        }

        private TreeModel<T> GenerateTreeInternal()
        {
            var tree = new TreeModel<T>(this.progress);
            tree.Leafparameters = this.TreeParameters.LeafParameters;

            Vector2D growDirection = new Vector2D(0d, 1d);

            float rotationStep = (float)this.TreeParameters.TrunkRotationAngle / (float)(this.TreeParameters.TreeTrunkSize - this.TreeParameters.TrunkRotationAngleStart);

            Point2D currentPoint = new Point2D(0d, 0d);
            var trunkStartWidth = this.TreeParameters.TrunkWidthStart;
            var trunkEndWidth = this.TreeParameters.TrunkWidthEnd;
            tree.Trunk.SkeletonPoints.Add(new TreePoint(currentPoint, growDirection) { Width = trunkStartWidth });

            int treeCrownSize = this.TreeParameters.TreeTrunkSize - this.TreeParameters.BranchStart;

            HashSet<int> leftBranches;
            HashSet<int> rightBranches;
            this.GenerateBranchPositions(this.TreeParameters.BranchStart, treeCrownSize, this.TreeParameters.BranchDistance, out leftBranches, out rightBranches);

            // Generate skelleton.
            this.progress.Report($"Generating trunk pixel.");
            for (int y = 0; y < this.TreeParameters.TreeTrunkSize - 1; y++)
            {
                if (tree.Progress.CancelRequested())
                {
                    return null;
                }

                if (y == this.TreeParameters.TrunkSkewAngleStart)
                {
                    growDirection = growDirection.Rotate(new Angle(this.TreeParameters.TrunkSkewAngle, new Degrees()));
                }

                if (y > this.TreeParameters.TrunkRotationAngleStart)
                {
                    growDirection = growDirection.Rotate(new Angle(rotationStep, new Degrees()));
                }

                // Generate trunk.
                var currentWidth = trunkEndWidth + (trunkStartWidth - trunkEndWidth) * ((this.TreeParameters.TreeTrunkSize - y) / (double)this.TreeParameters.TreeTrunkSize);
                currentPoint += growDirection;
                tree.Trunk.SkeletonPoints.Add(new TreePoint(new Point2D(Math.Round(currentPoint.X), Math.Round(currentPoint.Y)), growDirection) { Width = (int)currentWidth });
                tree.Trunk.ParentBranchConnectPoint = tree.Trunk.SkeletonPoints.First().Position;

                // Generate branches.
                if (y > this.TreeParameters.BranchStart)
                {
                    int connectIndex = Math.Max(0, y - 0);

                    if (leftBranches.Contains(y))
                    {
                        this.GenerateBranch(tree, tree.Trunk, growDirection, currentPoint, tree.Trunk.SkeletonPoints[connectIndex].Position, BranchType.Left, currentWidth);
                    }

                    if (rightBranches.Contains(y))
                    {
                        this.GenerateBranch(tree, tree.Trunk, growDirection, currentPoint, tree.Trunk.SkeletonPoints[connectIndex].Position, BranchType.Right, currentWidth);
                    }
                }
            }

            this.progress.Report($"Generating contour and fill.");
            tree.GenerateContourAndFillPoints();

            if (tree.Progress.CancelRequested())
            {
                return null;
            }

            this.progress.Report($"Generating shading.");
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
            for (int i = 0; i < this.TreeParameters.BranchCount; i++)
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

        private void GenerateBranch(TreeModel<T> tree, Branch parent, Vector2D growDirection, Point2D currentPoint, Point2D connectPoint, BranchType branchType, double width, int level = 1)
        {
            int angle = this.TreeParameters.BranchSkew + this.rand.Next(-this.TreeParameters.BranchSkewDeviation, this.TreeParameters.BranchSkewDeviation);

            if (branchType == BranchType.Left)
            {
                this.GrowBranch(tree, parent, growDirection, currentPoint, connectPoint, -angle, BranchType.Left, width, level);
            }

            if (branchType == BranchType.Right)
            {
                this.GrowBranch(tree, parent, growDirection, currentPoint, connectPoint, angle, BranchType.Right, width, level);
            }
        }

        private void GrowBranch(TreeModel<T> tree, Branch parent, Vector2D growDirection, Point2D currentPoint, Point2D connectPoint, double angle, BranchType branchType, double width, int level)
        {
            var branchChance = this.rand.Next(10);

            if (level > this.TreeParameters.BranchMaxLevel)
            {
                return;
            }

            if (level > this.TreeParameters.BranchMinLevel && branchChance <= 4)
            {
                return;
            }

            var t = this.TreeParameters.BranchMaxLevel == 1 ? 0 : ((level - 1) / (double)(this.TreeParameters.BranchMaxLevel - 1));
            var lengthFactor = (1 + (this.TreeParameters.BranchLevelLengthFactor - 1) * t);
            int branchLength = this.rand.Next(this.TreeParameters.BranchLengthMin, this.TreeParameters.BranchLengthMax + 1);
            branchLength = (int)(branchLength * lengthFactor);
            branchLength = Math.Max(branchLength, 1);
            int branchRotationAngleStart = this.TreeParameters.BranchRotationAngleStart;

            float rotationStep = (float)this.TreeParameters.BranchRotationAngle / (float)(branchLength - branchRotationAngleStart);
            rotationStep = branchType == BranchType.Right ? -rotationStep : rotationStep;

            HashSet<int> leftBranches = new HashSet<int>();
            HashSet<int> rightBranches = new HashSet<int>();
            var branchCrownSize = branchLength - this.TreeParameters.BranchStart;
            if (branchCrownSize > 0)
            {
                this.GenerateBranchPositions(this.TreeParameters.BranchStart, branchCrownSize, this.TreeParameters.BranchDistance / 2, out leftBranches, out rightBranches);
            }

            var branch = new Branch(parent);
            var branchStartWidth = Math.Max(width * 0.8, this.TreeParameters.TrunkWidthEnd);
            var branchEndWidth = this.TreeParameters.TrunkWidthEnd;
            growDirection = growDirection.Rotate(new Angle(angle, new Degrees()));
            branch.SkeletonPoints.Add(new TreePoint(new Point2D(Math.Round(currentPoint.X), Math.Round(currentPoint.Y)), growDirection) { Width = (int)branchStartWidth });
            branch.ParentBranchConnectPoint = connectPoint;

            for (int y = 0; y < branchLength; y++)
            {
                if (y > branchRotationAngleStart)
                {
                    growDirection = growDirection.Rotate(new Angle(rotationStep, new Degrees()));
                }

                currentPoint += growDirection;
                var currentWidth = branchEndWidth + (branchStartWidth - branchEndWidth) * ((branchLength - y) / (double)branchLength);
                branch.SkeletonPoints.Add(new TreePoint(new Point2D(Math.Round(currentPoint.X), Math.Round(currentPoint.Y)), growDirection) { Width = (int)currentWidth });

                if (y > this.TreeParameters.BranchStart)
                {
                    int connectIndex = Math.Max(0, y - 0);
                    if (leftBranches.Contains(y))
                    {
                        this.GenerateBranch(tree, branch, growDirection, currentPoint, branch.SkeletonPoints[connectIndex].Position, BranchType.Left, currentWidth, level + 1);
                    }

                    if (rightBranches.Contains(y))
                    {
                        this.GenerateBranch(tree, branch, growDirection, currentPoint, branch.SkeletonPoints[connectIndex].Position, BranchType.Right, currentWidth, level + 1);
                    }
                }
            }

            this.GenerateLeafPositions(branch, this.TreeParameters);

            tree.Branches.Add(branch);
        }

        private void GenerateLeafPositions(Branch branch, TreeParameters treeParameters)
        {
            List<LeafPosition> leafPositions = new List<LeafPosition>();
            if(treeParameters.LeafParameters.Count == 0)
            {
                return;
            }

            var leafDistance = Math.Max(1, treeParameters.LeafDistance + rand.Next(-treeParameters.LeafDistanceDeviation, treeParameters.LeafDistanceDeviation));
            for (int i = branch.SkeletonPoints.Count - 1; i >= 0; i -= leafDistance)
            {
                var leafPropability = rand.NextDouble() * treeParameters.LeafParameters.Sum(p => p.Probability);
                double probabilitySum = 0;
                LeafParameter selectedLeafType = treeParameters.LeafParameters.First();
                foreach(var leafParameter in treeParameters.LeafParameters)
                {
                    probabilitySum += leafParameter.Probability;
                    if(leafPropability < probabilitySum)
                    {
                        selectedLeafType = leafParameter;
                        break;
                    }
                }

                var scaleDeviation = rand.NextDouble() * selectedLeafType.SacleDeviation;

                var leafPosition = new LeafPosition { PositionInTree = branch.SkeletonPoints[i], Scale = selectedLeafType.Scale + scaleDeviation, ImageBuffer = selectedLeafType.ImageBuffer };
                leafPositions.Add(leafPosition);
                leafDistance = Math.Max(1, treeParameters.LeafDistance + rand.Next(-treeParameters.LeafDistanceDeviation, treeParameters.LeafDistanceDeviation));
            }

            branch.LeafPositions = leafPositions;
        }
    }
}
