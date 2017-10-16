namespace TreeGenerator
{
    using System;
    using System.Windows;
    using System.Windows.Media;

    using MathNet.Spatial.Euclidean;
    using MathNet.Spatial.Units;

    public class TreePoint
    {
        private Matrix rotation90RightMatrix = Matrix.Identity;

        public TreePoint(Point2D position, Vector2D growDirection)
        {
            this.rotation90RightMatrix.Rotate(90);
            this.Position = position;
            this.GrowDirection = growDirection;
        }

        public Point2D Position { get; set; }

        public Vector2D GrowDirection { get; set; }

        public int Width { get; set; }

        public Point2D GetLeftPosition()
        {
            var trunkElementRightDirection = this.GrowDirection.Rotate(new Angle(90, new Degrees()));
            var resizePointLeft = this.Position + (-this.Width * 0.5 * trunkElementRightDirection);
            resizePointLeft = new Point2D(Math.Round(resizePointLeft.X), Math.Round(resizePointLeft.Y));
            return resizePointLeft;
        }

        public Point2D GetRightPosition()
        {
            var trunkElementRightDirection = this.GrowDirection.Rotate(new Angle(90, new Degrees()));
            var resizePointRight = this.Position + (this.Width * 0.5 * trunkElementRightDirection);
            resizePointRight = new Point2D(Math.Round(resizePointRight.X), Math.Round(resizePointRight.Y));
            return resizePointRight;
        }
    }
}