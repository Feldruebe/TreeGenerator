namespace TreeGeneratorWPF.Wrapper
{
    using System.Windows.Media;
    using TreeGeneratorLib.Wrappers;
    public class WPFColorWrapper : IColor
    {
        public WPFColorWrapper(Color color)
        {
            this.color = color;
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
                this.color = Color.FromArgb(value, value, this.color.G, this.color.B);
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
    }
}