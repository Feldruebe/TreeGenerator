namespace TreeGeneratorWPF.MVVM
{
    using System;
    using System.Windows.Media.Imaging;
    using GalaSoft.MvvmLight;

    public class LeafImageViewModel : ViewModelBase
    {
        private bool canBeDeleted;
        private bool isIncluded;

        private float probability;
        private float scale;
        private float scaleDeviation;

        public LeafImageViewModel(string path, bool deletable = true)
        {
            var uri = new Uri(path);
            this.LoadedImage = new BitmapImage(uri);

            this.CanBeDeleted = deletable;
        }

        public LeafImageViewModel(BitmapImage image, bool deletable = true)
        {
            this.LoadedImage = image;

            this.CanBeDeleted = deletable;
        }

        public BitmapImage LoadedImage { get; }

        public bool CanBeDeleted
        {
            get => this.canBeDeleted;
            set => this.Set(ref this.canBeDeleted, value);
        }

        public bool IsIncluded
        {
            get => this.isIncluded;
            set => this.Set(ref this.isIncluded, value);
        }

        public float Probability
        {
            get => this.probability;
            set => this.Set(ref this.probability, value);
        }

        public float Scale
        {
            get => this.scale;
            set => this.Set(ref this.scale, value);
        }

        public float ScaleDeviation
        {
            get => this.scaleDeviation;
            set => this.Set(ref this.scaleDeviation, (float)Math.Round(value, 2));
        }
    }
}