namespace TreeGeneratorWPF.Wrapper
{
    using System.Drawing;

    using TreeGeneratorLib.Wrappers;
    public class WPFColorWrapper : IColor
    {
        public WPFColorWrapper(System.Windows.Media.Color color)
        {
            this.color = Color.FromArgb(color.A, color.R, color.G, color.B);
        }

        private Color color;

        public byte A
        {
            get
            {
                return this.color.A;
            }

            set
            {
                this.color = Color.FromArgb(value, this.color);
            }
        }

        public byte R
        {
            get
            {
                return this.color.R;
            }

            set
            {
                this.color = Color.FromArgb(this.color.A, value, this.color.G, this.color.B);
            }
        }

        public byte G
        {
            get
            {
                return this.color.G;
            }

            set
            {
                this.color = Color.FromArgb(this.color.A, this.color.R, value, this.color.B);
            }
        }

        public byte B
        {
            get
            {
                return this.color.B;
            }

            set
            {
                this.color = Color.FromArgb(this.color.A, this.color.R, this.color.G, value);
            }
        }

        public void ToHsv(out float h, out float s, out float v)
        {
            h = this.color.GetHue();
            s = this.color.GetSaturation();
            v = this.color.GetBrightness();
        }
    }
}