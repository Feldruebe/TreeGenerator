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

        public LeafImageViewModel(string path, bool deletable = true)
        {
            var uri = new Uri(path);
            this.LoadedImage = new BitmapImage(uri);

            this.CanBeDeleted = deletable;
        }

        public BitmapImage LoadedImage { get; }

        public bool CanBeDeleted
        {
            get => this.canBeDeleted;
            set => this.Set(ref  canBeDeleted, value);
        }

        public bool IsIncluded
        {
            get => isIncluded;
            set => this.Set(ref isIncluded, value);
        }

        public float Probability
        {
            get => this.probability;
            set => this.Set(ref this.probability, value);
        }
    }
}