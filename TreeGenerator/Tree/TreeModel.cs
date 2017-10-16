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

        public IList<TreePoint> AllTreePoints
        {
            get
            {
                return this.Branches.SelectMany(branch => branch.BranchPoints).Concat(this.Trunk.BranchPoints).ToList();
            }
        }
    }
}