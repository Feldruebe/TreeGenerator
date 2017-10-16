namespace TreeGenerator
{
    using System.Collections.Generic;
    using System.Linq;

    using MathNet.Spatial.Euclidean;

    class Branch
    {
        public List<TreePoint> BranchPoints { get; set; } = new List<TreePoint>();

        public IList<Point2D> LeftBorderPoints
        {
            get
            {
                return this.BranchPoints.Select(point => point.GetLeftPosition()).ToList();
            }
        }

        public IList<Point2D> RightBorderPoints
        {
            get
            {
                return this.BranchPoints.Select(point => point.GetRightPosition()).ToList();
            }
        }

        public IList<Point2D> LeftAndRightBorderPoints => this.LeftBorderPoints.Concat(this.RightBorderPoints).ToList();

        public IList<Point2D> PolygonPoints
        {
            get
            {
                var allPoints = this.LeftBorderPoints;
                var reversedRightPoints = this.RightBorderPoints.Reverse();
                allPoints = allPoints.Concat(reversedRightPoints).ToList();
                allPoints.Add(allPoints[0]);
                allPoints.Add(allPoints[1]);
                allPoints = allPoints.Distinct().ToList();

                return allPoints;
            }
        }

        public Dictionary<Point2D, int> SDF { get; set; } = new Dictionary<Point2D, int>();
    }
}