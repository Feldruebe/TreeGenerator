namespace TreeGeneratorWPF.ViewModels
{
    using System.Collections.ObjectModel;
    using TreeGeneratorLib.Generator;

    using GalaSoft.MvvmLight;
    using GalaSoft.MvvmLight.Command;
    using TreeGeneratorWPF.Wrapper;
    using System.Drawing;
    using SkiaSharp;
    using System.Drawing.Imaging;

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

            var maxTreeHeight = 0.0;
            var maxTreeWidth = 0.0;
            foreach (var tree in treesBatch)
            {
                tree.Tree.TreeVisual.DrawTree(tree.Tree, tree.TreeParameter);
                if(tree.Tree.TreeVisual.TreeIamge.Height > maxTreeHeight)
                {
                    maxTreeHeight = tree.Tree.TreeVisual.TreeIamge.Height;
                }

                if (tree.Tree.TreeVisual.TreeIamge.Width > maxTreeWidth)
                {
                    maxTreeWidth = tree.Tree.TreeVisual.TreeIamge.Width;
                }
            }

            int imageHeight = (int)maxTreeHeight;
            int imageWidth = batchParameter.BatchWidth + (int)maxTreeWidth;
            using (var treeBitmap = new Bitmap(imageWidth, imageHeight, PixelFormat.Format32bppPArgb))
            {
                var dataTree = treeBitmap.LockBits(new Rectangle(0, 0, imageWidth, imageHeight), ImageLockMode.WriteOnly, treeBitmap.PixelFormat);
                using (var surfaceTree = SKSurface.Create(imageWidth, imageHeight, SKImageInfo.PlatformColorType, SKAlphaType.Premul, dataTree.Scan0, imageWidth * 4))
                {
                    WpfTreeVisualWrapper.MoveOriginToLeftBottom(surfaceTree, imageHeight);
                    SKPaint bmpPaint = new SKPaint()
                    {
                        IsAntialias = false,
                        FilterQuality = SKFilterQuality.None,
                        IsDither = true
                    };

                    foreach (var tree in treesBatch)
                    {
                        var bytes = MainViewModel.GetPngBytesFromImageControl(tree.Tree.TreeVisual.TreeIamge);
                        SKBitmap bitmap = SKBitmap.Decode(bytes);
                        surfaceTree.Canvas.DrawBitmap(bitmap, tree.XPosition, 0, bmpPaint);
                    }
                }

                treeBitmap.UnlockBits(dataTree);
                treeBitmap.Save(@"C:\Users\Michael\Desktop\Neuer Ordner\Test.png", ImageFormat.Png);
            }
        }
    }
}