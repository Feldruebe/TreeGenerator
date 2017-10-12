namespace TreeGenerator
{
    using System;
    using System.Windows;
    using System.Windows.Media;

    public class TreePoint
    {
        private Matrix rotation90RightMatrix = Matrix.Identity;

        public TreePoint(Vector position, Vector growDirection)
        {
            this.rotation90RightMatrix.Rotate(90);
            this.Position = position;
            this.GrowDirection = growDirection;
        }

        public Vector Position { get; set; }

        public Vector GrowDirection { get; set; }

        public int Width { get; set; }

        public Vector GetLeftPosition()
        {
            var trunkElementRightDirection = this.rotation90RightMatrix.Transform(this.GrowDirection);
            var resizePointLeft = this.Position + (-this.Width * 0.5 * trunkElementRightDirection);
            resizePointLeft = new Vector(Math.Round(resizePointLeft.X), Math.Round(resizePointLeft.Y));
            return resizePointLeft;
        }

        public Vector GetRightPosition()
        {
            var trunkElementRightDirection = this.rotation90RightMatrix.Transform(this.GrowDirection);
            var resizePointRight = this.Position + (this.Width * 0.5 * trunkElementRightDirection);
            resizePointRight = new Vector(Math.Round(resizePointRight.X), Math.Round(resizePointRight.Y));
            return resizePointRight;
        }
    }
}