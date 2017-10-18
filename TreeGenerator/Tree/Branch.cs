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
                //var endPoints = this.GetEndPoints();
                //var startPoints = this.GetStartPoints();
                allPoints = allPoints.Concat(reversedRightPoints).ToList();
                allPoints.Add(allPoints[0]);
                allPoints.Add(allPoints[1]);
                allPoints = allPoints.Distinct().ToList();

                return allPoints;
            }
        }

        //private IEnumerable<Point2D> GetStartPoints()
        //{
        //    var leftPoint = this.LeftBorderPoints.First().ToVector();
        //    var rightPoint = this.RightBorderPoints.First().ToVector();

        //    var direction = rightPoint - leftPoint;
        //    var normDirection = direction.Normalize(1);

        //    var currentPosition = leftPoint + direction;
        //    for (double i = 0; i < direction.L2Norm(); i+=)
        //    {
                
        //    }
        //}

        private object GetEndPoints()
        {
            throw new System.NotImplementedException();
        }

        public Dictionary<Point2D, int> SDF { get; set; } = new Dictionary<Point2D, int>();
    }
}