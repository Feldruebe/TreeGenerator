namespace TreeGenerator
{
    using System.Collections.Generic;
    using System.Linq;

    using MathNet.Spatial.Euclidean;

    class TreeModel
    {
        public Branch Trunk { get; } = new Branch();

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
                return this.Branches.SelectMany(branch => branch.SkelletonPoints).Concat(this.Trunk.SkelletonPoints).ToList();
            }
        }

        public IList<Point2D> ContourPoints
        {
            get
            {
                return this.Trunk.ContourPoints.Concat(this.Branches.SelectMany(branch => branch.ContourPoints)).ToList();
            }
        }

        public void GenerateContour()
        {
            this.Trunk.GenerateContour();
            this.Branches.ForEach(branch => branch.GenerateContour());
        }
    }
}