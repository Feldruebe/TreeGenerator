namespace TreeGenerator
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using MathNet.Spatial.Euclidean;

    class Branch
    {
        public List<Point2D> ContourPoints { get; set; } = new List<Point2D>();

        public List<Point2D> AreaPoints { get; set; } = new List<Point2D>();

        public List<TreePoint> SkelletonPoints { get; set; } = new List<TreePoint>();

        public IList<Point2D> LeftBorderPoints
        {
            get
            {
                return this.SkelletonPoints.Select(point => point.GetLeftPosition()).ToList();
            }
        }

        public IList<Point2D> RightBorderPoints
        {
            get
            {
                return this.SkelletonPoints.Select(point => point.GetRightPosition()).ToList();
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

        public void GenerateContoure()
        {
            List<Point2D> contour = new List<Point2D>();

            // Generate left contour.
            //var leftBorder = this.LeftBorderPoints.ToList();
            //for (int i = 0; i < leftBorder.Count - 1; i++)
            //{
            //    var point1 = leftBorder[i];
            //    var point2 = leftBorder[i + 1];

            //    this.AddContourPointsBetweenPoints(point1, point2, contour);
            //}

            this.AddContourPointsBetweenPoints(new Point2D(0,0), new Point2D(5,10), contour);

            // Generate right contour.

            // Generate top contour.

            // Generate bot contour.
        }

        private void AddContourPointsBetweenPoints(Point2D point1, Point2D point2, List<Point2D> contour)
        {
            List<Vector2D> octagonOffsets = new List<Vector2D> {
                new Vector2D(-1, -1),
                new Vector2D(0, -1),
                new Vector2D(1, -1),
                new Vector2D(1, 0),
                new Vector2D(1, 1),
                new Vector2D(0, 1),
                new Vector2D(-1, 1),
                new Vector2D(-1, 0)
            };

            var currentPosition = point1;
            // Add points till we reach the otehr point.
            while (currentPosition != point2)
            {
                var minDistance = double.MaxValue;
                Point2D? minPoint = null;
                foreach (var offset in octagonOffsets)
                {
                    var nextPosition = currentPosition + offset;
                    var distance = point2.DistanceTo(nextPosition);

                    // On reaching the target stop.
                    if(nextPosition == point2)
                    {
                        return;
                    }

                    if(distance < minDistance)
                    {
                        minDistance = distance;
                        minPoint = nextPosition;
                    }
                }

                if(minPoint != null)
                {
                    contour.Add(minPoint.Value);
                    currentPosition = minPoint.Value;
                }
            }
        }
    }
}