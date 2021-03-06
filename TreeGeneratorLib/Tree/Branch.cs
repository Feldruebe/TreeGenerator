namespace TreeGeneratorLib.Tree
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using MathNet.Spatial.Euclidean;

    public class Branch
    {
        public Dictionary<Point2D, int> SDF { get; private set; }

        public HashSet<Point2D> ContourPoints { get; set; } = new HashSet<Point2D>();

        public HashSet<Point2D> ContourPointsWithoutBot { get; set; } = new HashSet<Point2D>();

        public HashSet<Point2D> FillPoints { get; set; } = new HashSet<Point2D>();

        public HashSet<Point2D> LeftContourPoints { get; set; } = new HashSet<Point2D>();

        public HashSet<Point2D> TopContourPoints { get; set; } = new HashSet<Point2D>();

        public HashSet<Point2D> RightContourPoints { get; set; } = new HashSet<Point2D>();

        public HashSet<Point2D> BotContourPoints { get; set; } = new HashSet<Point2D>();

        public List<Point2D> AreaPoints { get; set; } = new List<Point2D>();

        public List<TreePoint> SkeletonPoints { get; set; } = new List<TreePoint>();

        public Point2D ParentBranchConnectPoint { get; set; }

        public Branch Parent { get; }

        public List<LeafPosition> LeafPositions { get; set; }

        public IList<Point2D> LeftBorderPoints
        {
            get
            {
                return this.SkeletonPoints.Select(point => point.GetLeftPosition()).ToList();
            }
        }

        public IList<Point2D> RightBorderPoints
        {
            get
            {
                return this.SkeletonPoints.Select(point => point.GetRightPosition()).ToList();
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

        public Branch(Branch parent)
        {
            this.Parent = parent;
        }

        public void GenerateSDF()
        {
            HashSet<Point2D> pointsToVisit;
            HashSet<Point2D> nextPointsToVisit = new HashSet<Point2D>(this.LeftContourPoints.Concat(this.RightContourPoints));//.Concat(this.BotContourPoints));

            this.SDF = new Dictionary<Point2D, int>();

            int distance = 1;
            while (nextPointsToVisit.Any())
            {
                pointsToVisit = nextPointsToVisit;
                nextPointsToVisit = new HashSet<Point2D>();
                foreach (var point in pointsToVisit)
                {
                    var leftPoint = point + new Vector2D(-1, 0);
                    var botPoint = point + new Vector2D(0, -1);
                    var rightPoint = point + new Vector2D(1, 0);
                    var topPoint = point + new Vector2D(0, 1);

                    var neighbours = new[] { leftPoint, topPoint, rightPoint, botPoint };

                    foreach (var neighbour in neighbours)
                    {
                        if (this.FillPoints.Contains(neighbour) && !this.SDF.ContainsKey(neighbour))
                        {
                            nextPointsToVisit.Add(neighbour);
                            this.SDF[neighbour] = distance;
                        }
                    }
                }

                distance++;
            }

        }

        public void GenerateContour()
        {
            var leftBorder = this.LeftBorderPoints.ToList();
            var rightBorder = this.RightBorderPoints.Reverse().ToList();

            // Generate left contour.
            this.LeftContourPoints.Clear();
            //this.LeftContourPoints.UnionWith(leftBorder);
            for (int i = 0; i < leftBorder.Count - 1; i++)
            {
                var point1 = leftBorder[i];
                var point2 = leftBorder[i + 1];

                this.LeftContourPoints.Add(point1);
                this.AddContourPointsBetweenPoints(point1, point2, this.LeftContourPoints);
                this.LeftContourPoints.Add(point2);
            }

            // Generate top contour.
            this.TopContourPoints.Clear();
            this.AddContourPointsBetweenPoints(leftBorder.Last(), rightBorder.First(), this.TopContourPoints);

            // Generate right contour.
            this.RightContourPoints.Clear();
            //this.RightContourPoints.UnionWith(rightBorder);
            for (int i = 0; i < rightBorder.Count - 1; i++)
            {
                var point1 = rightBorder[i];
                var point2 = rightBorder[i + 1];

                this.RightContourPoints.Add(point1);
                this.AddContourPointsBetweenPoints(point1, point2, this.RightContourPoints);
                this.RightContourPoints.Add(point2);
            }

            // Generate bot contour.
            this.BotContourPoints.Clear();
//            var botContourSamplePoints = new List<Point2D>();
//            for (int i = 0; i < 6; i++)
//            {
//                botContourSamplePoints.Add(InterpolateBezier(rightBorder.Last(), this.ParentBranchConnectPoint, leftBorder.First(), i / 5d));
//            }
//
//            for (int i = 0; i < botContourSamplePoints.Count - 1; i++)
//            {
//                var point1 = botContourSamplePoints[i];
//                var point2 = botContourSamplePoints[i + 1];
//
//                this.BotContourPoints.Add(point1);
//                this.AddContourPointsBetweenPoints(point1, point2, this.BotContourPoints);
//                this.BotContourPoints.Add(point2);
//            }

           // this.AddContourPointsBetweenPoints(rightBorder.Last(), leftBorder.First(), this.BotContourPoints);

            HashSet<Point2D> contourPoints = new HashSet<Point2D>(this.LeftContourPoints);
            contourPoints.UnionWith(this.TopContourPoints);
            contourPoints.UnionWith(this.RightContourPoints);
            this.ContourPointsWithoutBot = new HashSet<Point2D>(contourPoints);
            contourPoints.UnionWith(this.BotContourPoints);
            this.ContourPoints = contourPoints;

            var newContour = new HashSet<Point2D>(this.ContourPoints);
            this.ContourPoints = new HashSet<Point2D>(this.ContourPoints.Where(point => this.CheckIfNeededForConnectionAndRemove(point, newContour)));
            newContour = new HashSet<Point2D>(this.ContourPointsWithoutBot);
            this.ContourPointsWithoutBot = new HashSet<Point2D>(this.ContourPointsWithoutBot.Where(point => this.CheckIfNeededForConnectionAndRemove(point, newContour)));
        }

        public void GenerateFillPoints()
        {
            int minX = (int)this.ContourPoints.Min(point => point.X);
            int maxX = (int)this.ContourPoints.Max(point => point.X);
            int minY = (int)this.ContourPoints.Min(point => point.Y);
            int maxY = (int)this.ContourPoints.Max(point => point.Y);

            Polygon2D contourPolygon = new Polygon2D(this.ContourPoints);
            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    var newPoint = new Point2D(x, y);
                    if (contourPolygon.EnclosesPoint(newPoint) && !this.ContourPoints.Contains(newPoint))
                    {
                        this.FillPoints.Add(newPoint);
                    }
                }
            }
        }

        private bool CheckIfNeededForConnectionAndRemove(Point2D point, HashSet<Point2D> neighbours)
        {
            var leftPoint = point + new Vector2D(-1, 0);
            var leftBotPoint = point + new Vector2D(-1, -1);
            var botPoint = point + new Vector2D(0, -1);
            var botRightPoint = point + new Vector2D(1, -1);
            var rightPoint = point + new Vector2D(1, 0);
            var rightTopPoint = point + new Vector2D(1, 1);
            var topPoint = point + new Vector2D(0, 1);
            var topLeftPoint = point + new Vector2D(-1, 1);

            if ((neighbours.Contains(leftPoint) && neighbours.Contains(topPoint)) &&
                !neighbours.Contains(botRightPoint))
            {
                neighbours.Remove(point);
                return false;
            }

            if ((neighbours.Contains(topPoint) && neighbours.Contains(rightPoint)) &&
                !neighbours.Contains(leftBotPoint))
            {
                neighbours.Remove(point);
                return false;
            }

            if ((neighbours.Contains(rightPoint) && neighbours.Contains(botPoint)) &&
                !neighbours.Contains(topLeftPoint))
            {
                neighbours.Remove(point);
                return false;
            }

            if ((neighbours.Contains(botPoint) && neighbours.Contains(leftPoint)) &&
                !neighbours.Contains(rightTopPoint))
            {
                neighbours.Remove(point);
                return false;
            }

            return true;
        }

        private bool CheckPointAndAddDecandants(Point2D pointToCheck, HashSet<Point2D> checkedPoints, HashSet<Point2D> currentFillPoints, Queue<Point2D> pointsToCheck, int minX, int maxX, int minY, int maxY)
        {
            var hitBounds = false;
            if (pointToCheck.X >= minX && pointToCheck.X <= maxX && pointToCheck.Y >= minY && pointToCheck.Y <= maxY)
            {
                if (!checkedPoints.Contains(pointToCheck))
                {
                    if (!this.ContourPoints.Contains(pointToCheck))
                    {
                        currentFillPoints.Add(pointToCheck);
                    }

                    var leftPoint = pointToCheck + new Vector2D(-1, 0);
                    if (!this.ContourPoints.Contains(leftPoint))
                    {
                        pointsToCheck.Enqueue(leftPoint);
                    }

                    var topPoint = pointToCheck + new Vector2D(0, 1);
                    if (!this.ContourPoints.Contains(topPoint))
                    {
                        pointsToCheck.Enqueue(topPoint);
                    }

                    var rightPoint = pointToCheck + new Vector2D(1, 0);
                    if (!this.ContourPoints.Contains(rightPoint))
                    {
                        pointsToCheck.Enqueue(rightPoint);
                    }

                    var botPoint = pointToCheck + new Vector2D(0, -1);
                    if (!this.ContourPoints.Contains(botPoint))
                    {
                        pointsToCheck.Enqueue(botPoint);
                    }
                }
            }
            else
            {
                hitBounds = true;
            }

            checkedPoints.Add(pointToCheck);

            return hitBounds;
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

        public static Point2D InterpolateBezier(Point2D p0, Point2D p1, Point2D p2, double factor)
        {
            var factor2 = Math.Pow(factor, 2);
            int xInterpolation = (int)Math.Round((p0.X - 2 * p1.X + p2.X) * factor2 + (-2 * p0.X + 2 * p1.X) * factor + p0.X);
            int yInterpolation = (int)Math.Round((p0.Y - 2 * p1.Y + p2.Y) * factor2 + (-2 * p0.Y + 2 * p1.Y) * factor + p0.Y);

            return new Point2D(xInterpolation, yInterpolation);
        }
    }
}