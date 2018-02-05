namespace TreeGeneratorLib.Tree
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using MathNet.Spatial.Euclidean;

    using TreeGeneratorLib.Wrappers;

    public class TreeModel<T> where T : TreeVisual
    {
        public TreeModel(ICancelableProgress progress)
        {
            var type = typeof(T);
            this.TreeVisual = (T)Activator.CreateInstance(type);
            this.Progress = progress;
        }

        public ICancelableProgress Progress { get; }

        public T TreeVisual { get; private set; }

        public Branch Trunk { get; } = new Branch(null);

        public List<Branch> Branches { get; } = new List<Branch>();

        public IList<Point2D> AllBorderPoints
        {
            get
            {
                return this.Branches.SelectMany(branch => branch.LeftAndRightBorderPoints).Concat(this.Trunk.LeftAndRightBorderPoints).ToList();
            }
        }

        public IList<TreePoint> AllTreeSkelletonPoints
        {
            get
            {
                return this.Branches.SelectMany(branch => branch.SkeletonPoints).Concat(this.Trunk.SkeletonPoints).ToList();
            }
        }

        public IList<Point2D> ContourPoints
        {
            get
            {
                return this.Trunk.ContourPoints.Concat(this.Branches.SelectMany(branch => branch.ContourPoints)).ToList();
            }
        }

        public IList<Point2D> ContourPointsWithoutBot
        {
            get
            {
                return this.Trunk.ContourPoints.Concat(this.Branches.SelectMany(branch => branch.ContourPointsWithoutBot)).ToList();
            }
        }

        public IList<Point2D> FillPoints
        {
            get
            {
                return this.Trunk.FillPoints.Concat(this.Branches.SelectMany(branch => branch.FillPoints)).ToList();
            }
        }

        public void GenerateContourAndFillPoints()
        {
            this.Trunk.GenerateContour();
            this.Branches.ForEach(branch => branch.GenerateContour());

            this.Trunk.GenerateFillPoints();
            this.Branches.ForEach(branch => branch.GenerateFillPoints());
        }

        public void GenerateSDF()
        {
            this.Trunk.GenerateSDF();
            this.Branches.ForEach(branch => branch.GenerateSDF());
        }
    }
}