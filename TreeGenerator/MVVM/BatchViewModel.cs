namespace TreeGeneratorWPF.ViewModels
{
    using System.Collections.ObjectModel;

    using GalaSoft.MvvmLight;

    public class BatchViewModel : ViewModelBase
    {
        private ObservableCollection<BatchTreeViewModel> batchTrees = new ObservableCollection<BatchTreeViewModel>();

        private int batchedImageWidth;

        private int batchedTreesCount;

        public ObservableCollection<BatchTreeViewModel> BatchTrees
        {
            get => this.batchTrees;
            set => this.Set(ref this.batchTrees, value);
        }

        public int BatchedImageWidth
        {
            get => this.batchedImageWidth;
            set => this.Set(ref this.batchedImageWidth, value);
        }

        public int BtchedTreesCount
        {
            get => this.batchedTreesCount;
            set => this.Set(ref this.batchedTreesCount, value);
        }
    }
}