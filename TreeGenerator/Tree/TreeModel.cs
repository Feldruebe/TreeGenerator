namespace TreeGenerator
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;

    class TreeModel
    {
        public Branch Trunk { get; } = new Branch();

        public List<Branch> Branches { get; } = new List<Branch>();

        public IList<Vector> AllBorderPoints
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