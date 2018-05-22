namespace TreeGeneratorWPF.ViewModels
{
    using System.Collections.ObjectModel;
    using System.Linq;

    using TreeGeneratorLib.Generator;

    using GalaSoft.MvvmLight;
    using GalaSoft.MvvmLight.Command;

    using TreeGeneratorLib.Wrappers;

    using TreeGeneratorWPF.Wrapper;

    public class BatchViewModel : ViewModelBase
    {
        private ObservableCollection<BatchTreeViewModel> batchTrees = new ObservableCollection<BatchTreeViewModel>();

        private int batchedImageWidth;

        private int batchedTreesCount;

        private ICancelableProgress progress;

        public RelayCommand ExecuteBatchCommand => new RelayCommand(this.ExecuteBatch);

        public RelayCommand<BatchTreeViewModel> DeleteCommand => new RelayCommand<BatchTreeViewModel>(viewModel => this.BatchTrees.Remove(viewModel));

        public BatchViewModel(WPFProgressController controller)
        {
            this.progress = controller;
            this.BatchedTreesCount = 10;
            this.BatchedImageWidth = 50;
        }

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

        public int BatchedTreesCount
        {
            get => this.batchedTreesCount;
            set => this.Set(ref this.batchedTreesCount, value);
        }

        private void ExecuteBatch()
        {
            var batchTreeParameters = this.BatchTrees.Select(
                batchTreeViewModel => new BatchTreeParameter()
                {
                    Probability = batchTreeViewModel.Probability,
                    TreeParameters = batchTreeViewModel.Parameters
                }).ToList();
            var batchParameters = new BatchParameters() { BatchTreeParameters = batchTreeParameters, BatchWidth = this.BatchedImageWidth, TreeCount = this.BatchedTreesCount };
            BatchGenerator<WpfTreeVisualWrapper> generator = new BatchGenerator<WpfTreeVisualWrapper>();
            generator.Initialize(this.progress);
            var treesBatch = generator.GenerateBatch(batchParameters);

        }
    }
}