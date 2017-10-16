namespace TreeGenerator
{
    using System.Linq;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;

    public static class BitmapExtensions
    {
        public static void SetPixelSafeLeftBot(this WriteableBitmap bitmap, int x, int y, Color color)
        {
            y = (int)bitmap.Height - 1 - y;

            if (x < bitmap.Width && x >= 0 && y < bitmap.Height && y >= 0)
            {
                bitmap.SetPixel(x, y, color);
            }
        }

        public static void FillPolygonOriginLeftBot(this WriteableBitmap bitmap, int[] polygon, Color color)
        {
            var copy = polygon.Select(element => element).ToArray();
            for (int i = 1; i < polygon.Length; i += 2)
            {
                copy[i] = (int)bitmap.Height - 1 - polygon[i];
            }

            bitmap.FillPolygon(copy, WriteableBitmapExtensions.ConvertColor(color), true);
        }

        public static void DrawPolylineOriginLeftBot(this WriteableBitmap bitmap, int[] polyline, Color color)
        {
            var copy = polyline.Select(element => element).ToArray();
            for (int i = 1; i < polyline.Length; i += 2)
            {
                copy[i] = (int)bitmap.Height - 1 - polyline[i];
            }

            bitmap.DrawPolyline(copy, color);
        }
    }
}