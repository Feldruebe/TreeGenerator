namespace TreeGenerator
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;

    class Branch
    {
        public List<TreePoint> BranchPoints { get; set; } = new List<TreePoint>();

        public IList<Vector> LeftBorderPoints
        {
            get
            {
                return this.BranchPoints.Select(point => point.GetLeftPosition()).ToList();
            }
        }

        public IList<Vector> RightBorderPoints
        {
            get
            {
                return this.BranchPoints.Select(point => point.GetRightPosition()).ToList();
            }
        }

        public IList<Vector> LeftAndRightBorderPoints => this.LeftBorderPoints.Concat(this.RightBorderPoints).ToList();
    }
}