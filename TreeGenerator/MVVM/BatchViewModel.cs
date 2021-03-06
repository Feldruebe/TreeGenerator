﻿namespace TreeGeneratorWPF.ViewModels
{
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;

    using TreeGeneratorLib.Generator;

    using GalaSoft.MvvmLight;
    using GalaSoft.MvvmLight.Command;

    using TreeGeneratorLib.Wrappers;

    using TreeGeneratorWPF.Wrapper;
    using System.Drawing;
    using SkiaSharp;
    using System.Drawing.Imaging;
    using System.Windows.Media.Imaging;
    using Microsoft.Win32;
    using MahApps.Metro.Controls.Dialogs;

    public class BatchViewModel : ViewModelBase
    {
        private ObservableCollection<BatchTreeViewModel> batchTrees = new ObservableCollection<BatchTreeViewModel>();

        private int batchedImageWidth;

        private int batchedTreesCount;

        private WPFProgressController progress;

        private BitmapSource treeBatchImage;
        private int batchTreesDistance;

        public RelayCommand ExecuteBatchCommand => new RelayCommand(this.ExecuteBatch);

        public RelayCommand SaveBatchCommand => new RelayCommand(this.SaveBatchImage);

        public RelayCommand<BatchTreeViewModel> DeleteCommand => new RelayCommand<BatchTreeViewModel>(viewModel => this.BatchTrees.Remove(viewModel));

        public BatchViewModel(WPFProgressController controller)
        {
            this.progress = controller;
            this.BatchedTreesCount = 10;
            this.BatchedImageWidth = 50;
            this.BatchTreesDistance = 5;
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

        public int BatchTreesDistance
        {
            get => this.batchTreesDistance;
            set => this.Set(ref this.batchTreesDistance, value);
        }

        public BitmapSource TreeBatchImage
        {
            get => this.treeBatchImage;
            set => this.Set(ref this.treeBatchImage, value);
        }

        private void SaveBatchImage()
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.AddExtension = true;
            dialog.DefaultExt = "*.png";
            dialog.Filter = "*.png|*.png";
            if (dialog.ShowDialog() == true)
            {
                MainViewModel.CreatePng(dialog.FileName, this.TreeBatchImage);
            }
        }

        private async void ExecuteBatch()
        {
            if(!this.BatchTrees.Any())
            {
                return;
            }

            this.progress.ProgressDialogController = await DialogCoordinator.Instance.ShowProgressAsync(this, "Waiting...", "Wait", true);

            var batchTreeParameters = this.BatchTrees.Select(
                batchTreeViewModel => new BatchTreeParameter()
                {
                    Probability = batchTreeViewModel.Probability,
                    TreeParameters = batchTreeViewModel.Parameters,
                    UseNewSeed = batchTreeViewModel.UseNewSeed,
                }).ToList();
            var batchParameters = new BatchParameters() { BatchTreeParameters = batchTreeParameters, BatchWidth = this.BatchedImageWidth, TreeCount = this.BatchedTreesCount, BatchTreeDistance = this.batchTreesDistance };
            BatchGenerator<WpfTreeVisualWrapper> generator = new BatchGenerator<WpfTreeVisualWrapper>();
            generator.Initialize(this.progress);
            var treesBatch = generator.GenerateBatch(batchParameters);

            var maxTreeHeight = 0.0;
            var maxTreeWidth = 0.0;
            foreach (var tree in treesBatch)
            {
                tree.Tree.TreeVisual.DrawTree(tree.Tree, tree.TreeParameter);
                if (tree.Tree.TreeVisual.TreeIamge.Height > maxTreeHeight)
                {
                    maxTreeHeight = tree.Tree.TreeVisual.TreeIamge.Height;
                }

                if (tree.Tree.TreeVisual.TreeIamge.Width > maxTreeWidth)
                {
                    maxTreeWidth = tree.Tree.TreeVisual.TreeIamge.Width;
                }
            }

            int imageHeight = (int)maxTreeHeight;
            int imageWidth = batchParameters.BatchWidth + (int)maxTreeWidth;
            using (var treeBitmap = new Bitmap(imageWidth, imageHeight, PixelFormat.Format32bppPArgb))
            {
                var dataTree = treeBitmap.LockBits(new Rectangle(0, 0, imageWidth, imageHeight), ImageLockMode.WriteOnly, treeBitmap.PixelFormat);
                using (var surfaceTree = SKSurface.Create(imageWidth, imageHeight, SKImageInfo.PlatformColorType, SKAlphaType.Premul, dataTree.Scan0, imageWidth * 4))
                {
                    //WpfTreeVisualWrapper.MoveOriginToLeftBottom(surfaceTree, imageHeight);
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
                        surfaceTree.Canvas.DrawBitmap(bitmap, tree.XPosition, imageHeight - bitmap.Height, bmpPaint);
                    }
                }

                treeBitmap.UnlockBits(dataTree);
                this.TreeBatchImage = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(treeBitmap.GetHbitmap(), IntPtr.Zero, System.Windows.Int32Rect.Empty, BitmapSizeOptions.FromWidthAndHeight(imageWidth, imageHeight));
            }

            await this.progress.ProgressDialogController.CloseAsync();
        }
    }
}