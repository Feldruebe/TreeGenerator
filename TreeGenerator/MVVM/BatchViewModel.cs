namespace TreeGeneratorWPF.ViewModels
{
    using System.Collections.ObjectModel;
    using TreeGeneratorLib.Generator;

    using GalaSoft.MvvmLight;
    using GalaSoft.MvvmLight.Command;
    using TreeGeneratorWPF.Wrapper;

    public class BatchViewModel : ViewModelBase
    {
        private ObservableCollection<BatchTreeViewModel> batchTrees = new ObservableCollection<BatchTreeViewModel>();

        private int batchedImageWidth;

        private int batchedTreesCount;

        public RelayCommand ExecuteBatchCommand => new RelayCommand(this.ExecuteBatch);

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

        private void ExecuteBatch()
        {
            var batchParameter = new BatchParameters();
            BatchGenerator<WpfTreeVisualWrapper> generator = new BatchGenerator<WpfTreeVisualWrapper>();
            var treesBatch = generator.GenerateBatch(batchParameter);

        }
    }
}