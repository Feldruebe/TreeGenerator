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

        public void GenerateContour()
        {
            HashSet<Point2D> contour = new HashSet<Point2D>();
            var leftBorder = this.LeftBorderPoints.ToList();
            var rightBorder = this.RightBorderPoints.Reverse().ToList();

            // Generate left contour.
            contour.UnionWith(leftBorder);
            for (int i = 0; i < leftBorder.Count - 1; i++)
            {
                var point1 = leftBorder[i];
                var point2 = leftBorder[i + 1];

                this.AddContourPointsBetweenPoints(point1, point2, contour);
            }

            // Generate top contour.
            this.AddContourPointsBetweenPoints(leftBorder.Last(), rightBorder.First(), contour);

            // Generate right contour.
            contour.UnionWith(rightBorder);
            for (int i = 0; i < leftBorder.Count - 1; i++)
            {
                var point1 = rightBorder[i];
                var point2 = rightBorder[i + 1];

                this.AddContourPointsBetweenPoints(point1, point2, contour);
            }

            // Generate bot contour.
            //this.AddContourPointsBetweenPoints(rightBorder.Last(), leftBorder.First(), contour);

            this.ContourPoints = contour.ToList();
        }

        private void AddContourPointsBetweenPoints(Point2D point1, Point2D point2, HashSet<Point2D> contour)
        {
            int p1X = (int)Math.Round(point1.X);
            int p1Y = (int)Math.Round(point1.Y);
            int p2X = (int)Math.Round(point2.X);
            int p2Y = (int)Math.Round(point2.Y);

            int deltaX = Math.Abs(p2X - p1X);
            int signX = p1X < p2X ? 1 : -1;
            int deltaY = Math.Abs(p2Y - p1Y);
            int signY = p1Y < p2Y ? 1 : -1;

            int error = (deltaX > deltaY ? deltaX : -deltaY) / 2;

            while (true)
            {
                var error2 = error;
                if (error2 > -deltaX)
                {
                    error -= deltaY;
                    p1X += signX;
                }

                if (error2 < deltaY)
                {
                    error += deltaX;
                    p1Y += signY;
                }

                if (p1X == p2X && p1Y == p2Y)
                {
                    break;
                }

                contour.Add(new Point2D(p1X, p1Y));
            }
        }
    }
}