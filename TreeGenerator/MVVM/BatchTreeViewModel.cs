namespace TreeGeneratorWPF.ViewModels
{
    using System.Windows.Media.Imaging;

    using GalaSoft.MvvmLight;

    using TreeGeneratorLib.Generator;

    public class BatchTreeViewModel : ViewModelBase
    {
        private string name;

        private BitmapSource thumbnail;

        private double probability;
        private bool useNewSeed;

        public string Name
        {
            get => this.name;
            set => this.Set(ref this.name, value);
        }

        public BitmapSource Thumbnail
        {
            get => this.thumbnail;
            set => this.Set(ref this.thumbnail, value);
        }

        public TreeParameters Parameters { get; set; }

        public double Probability
        {
            get => this.probability;
            set => this.Set(ref this.probability, value);
        }

        public bool UseNewSeed
        {
            get => this.useNewSeed;
            set => this.Set(ref this.useNewSeed, value);
        }
    }
}