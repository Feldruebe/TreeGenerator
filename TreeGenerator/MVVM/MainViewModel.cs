namespace TreeGeneratorWPF.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Windows.Media.Imaging;

    using GalaSoft.MvvmLight;

    using GalaSoft.MvvmLight.CommandWpf;
    using System.Diagnostics;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Threading.Tasks;
    using System.Windows.Media;

    using MahApps.Metro.Controls.Dialogs;

    using MathNet.Numerics.LinearAlgebra;
    using MathNet.Numerics.LinearAlgebra.Double;
    using MathNet.Numerics.Random;
    using MathNet.Spatial.Euclidean;
    using MathNet.Spatial.Units;

    using Microsoft.Win32;

    using SkiaSharp;

    using Color = System.Windows.Media.Color;
    using PixelFormat = System.Drawing.Imaging.PixelFormat;
    using MathNet.Numerics.Interpolation;

    using TreeGeneratorLib.Generator;
    using TreeGeneratorLib.Tree;

    using TreeGeneratorWPF.Properties;
    using TreeGeneratorWPF.Wrapper;

    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    /// </para>
    /// <para>
    /// You can also use Blend to data bind with the tool's support.
    /// </para>
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        private Random rand;
        private Random randomRand = new Random();

        private BitmapSource imageSkeletton;
        private BitmapSource imageTree;

        private int treeTrunkSize;
        private int trunkRotationAngle;
        private float trunkRotationAngleStart;
        private int trunkSkewAngle;
        private int trunkSkewAngleStart;
        private int branchCount;
        private int branchStart;
        private int branchLengthMin;
        private int branchLengthMax;
        private int branchDistance;
        private int branchSkew;
        private int branchSkewDeviation;
        private int branchRotationAngleStart;
        private float branchRotationAngle;
        private int trunkWidthStart;
        private int trunkWidthEnd;
        private SKColor trunkColor;
        private SKColor branchColor;
        private SKColor outlineColor;
        private SKColor branchOutlineColor;
        private bool isColorFlyoutOpen = false;
        private double branchLevelLengthFactor;
        private int branchMaxLevel;
        private int randomSeed;

        private bool regenerateRandomSeed;
        private bool debugModeEnabled;

        private TreeModel<WpfTreeVisualWrapper> tree;
        private WPFProgressController controller = new WPFProgressController();

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {
            this.DebugViewModel = new DebugViewModel(this);
#if DEBUG
            this.DebugViewModel.DebugModeEnabled = true;
#endif

            this.GenerateTreeCommand = new RelayCommand(this.GenerateTreeAndDraw);
            this.ExportImageCommand = new RelayCommand(this.ExportImage);
            this.TreeTrunkSize = 60;
            this.TrunkSkewAngle = 0;
            this.TrunkSkewAngleStart = 0;
            this.TrunkRotationAngle = 0;
            this.TrunkRotationAngleStart = 0;
            this.TrunkWidthStart = 8;
            this.TrunkWidthEnd = 2;
            this.BranchCount = 4;
            this.BranchStart = this.TreeTrunkSize / 3;
            this.BranchLengthMin = this.TreeTrunkSize;
            this.BranchLengthMax = this.TreeTrunkSize;
            this.BranchDistance = 20;
            this.BranchSkew = 35;
            this.BranchSkewDeviation = 10;
            this.BranchRotationAngleStart = 0;
            this.BranchRotationAngle = 30;
            this.TrunkColor = Colors.SaddleBrown;
            this.OutlineColor = Colors.Black;
            this.BranchMaxLevel = 3;
            this.BranchLevelLengthFactor = 1;
        }

        public int RandomSeed
        {
            get
            {
                return this.randomSeed;
            }

            set
            {
                this.Set(ref this.randomSeed, value);
            }
        }

        public bool RegenerateRandomSeed
        {
            get
            {
                return this.regenerateRandomSeed;
            }

            set
            {
                this.Set(ref this.regenerateRandomSeed, value);
            }
        }

        public int TreeTrunkSize
        {
            get { return this.treeTrunkSize; }
            set
            {
                this.Set(ref this.treeTrunkSize, value);
                if (value < this.BranchStart)
                {
                    this.BranchStart = value;
                }
            }
        }

        public BitmapSource ImageSkeletton
        {
            get { return this.imageSkeletton; }
            set { this.Set(ref this.imageSkeletton, value); }
        }

        public BitmapSource ImageTree
        {
            get { return this.imageTree; }
            set { this.Set(ref this.imageTree, value); }
        }

        public int TrunkRotationAngle
        {
            get { return this.trunkRotationAngle; }
            set { this.Set(ref this.trunkRotationAngle, value); }
        }

        public float TrunkRotationAngleStart
        {
            get { return this.trunkRotationAngleStart; }

            set { this.Set(ref this.trunkRotationAngleStart, value); }
        }

        public int TrunkSkewAngle
        {
            get { return this.trunkSkewAngle; }
            set { this.Set(ref this.trunkSkewAngle, value); }
        }

        public int TrunkSkewAngleStart
        {
            get
            {
                return this.trunkSkewAngleStart;
            }

            set
            {
                var max = Math.Max(this.TrunkWidthStart, value);
                this.Set(ref this.trunkSkewAngleStart, max);
            }
        }

        public int BranchCount
        {
            get { return this.branchCount; }
            set { this.Set(ref this.branchCount, value); }
        }

        public int AllBranchesCount
        {
            get
            {
                return this.Tree?.Branches.Count - 1 ?? 0;
            }
        }

        public int BranchStart
        {
            get { return this.branchStart; }
            set { this.Set(ref this.branchStart, value); }
        }

        public int BranchLengthMin
        {
            get { return this.branchLengthMin; }
            set { this.Set(ref this.branchLengthMin, value); }
        }

        public int BranchLengthMax
        {
            get { return this.branchLengthMax; }
            set { this.Set(ref this.branchLengthMax, value); }
        }

        public int BranchDistance
        {
            get { return this.branchDistance; }
            set { this.Set(ref this.branchDistance, value); }
        }

        public double BranchLevelLengthFactor
        {
            get { return this.branchLevelLengthFactor; }
            set { this.Set(ref this.branchLevelLengthFactor, value); }
        }

        public int BranchSkew
        {
            get { return this.branchSkew; }

            set { this.Set(ref this.branchSkew, value); }
        }

        public int BranchSkewDeviation
        {
            get { return this.branchSkewDeviation; }
            set { this.Set(ref this.branchSkewDeviation, value); }
        }

        public int BranchRotationAngleStart
        {
            get { return this.branchRotationAngleStart; }
            set { this.Set(ref this.branchRotationAngleStart, value); }
        }

        public float BranchRotationAngle
        {
            get { return this.branchRotationAngle; }
            set { this.Set(ref this.branchRotationAngle, value); }
        }

        public int TrunkWidthStart
        {
            get
            {
                return this.trunkWidthStart;
            }

            set
            {
                if (value < this.TrunkWidthEnd)
                {
                    this.TrunkWidthEnd = value;
                }

                if (value > this.TrunkSkewAngleStart)
                {
                    this.TrunkSkewAngleStart = value;
                }

                this.Set(ref this.trunkWidthStart, value);
            }
        }

        public int TrunkWidthEnd
        {
            get { return this.trunkWidthEnd; }
            set { this.Set(ref this.trunkWidthEnd, value); }
        }

        public Color TrunkColor
        {
            get
            {
                return Color.FromArgb(this.trunkColor.Alpha, this.trunkColor.Red, this.trunkColor.Green, this.trunkColor.Blue);
            }

            set
            {
                SKColor c = new SKColor(value.R, value.G, value.B, value.A);
                this.Set(ref this.trunkColor, c);
                this.Tree?.TreeVisual.DrawTree(this.Tree, this.Parameters);
            }
        }

        public Color OutlineColor
        {
            get
            {
                return Color.FromArgb(this.outlineColor.Alpha, this.outlineColor.Red, this.outlineColor.Green, this.outlineColor.Blue);
            }

            set
            {
                SKColor c = new SKColor(value.R, value.G, value.B, value.A);
                this.Set(ref this.outlineColor, c);
                this.Tree?.TreeVisual.DrawTree(this.Tree, this.Parameters);
            }
        }

        public RelayCommand GenerateTreeCommand { get; set; }

        public RelayCommand ExportImageCommand { get; set; }

        public bool IsDebugMode
        {
            get
            {
#if DEBUG
                return true;
#endif

                return false;
            }
        }

        public DebugViewModel DebugViewModel { get; }

        public TreeModel<WpfTreeVisualWrapper> Tree
        {
            get
            {
                return this.tree;
            }

            private set
            {
                this.tree = value;
                this.RaisePropertyChanged(nameof(this.AllBranchesCount));
            }
        }

        public TreeParameters Parameters { get; set; }

        public int BranchMaxLevel
        {
            get { return this.branchMaxLevel; }
            set { this.Set(ref this.branchMaxLevel, value); }
        }

        public void RedrawTree()
        {
            this.Tree?.TreeVisual.DrawTree(this.Tree, this.Parameters);
        }

        private async void GenerateTreeAndDraw()
        {
            //try
            //{
            this.controller.ProgressDialogController = await DialogCoordinator.Instance.ShowProgressAsync(this, "Waiting...", "Wait", true);
            this.ManageRandom();
            this.Parameters = this.CreateTreeParameters();
            this.Tree = await this.GenerateAndDrawTreeAsync();
            this.Tree.TreeVisual.DrawTree(this.Tree, this.Parameters);
            //this.DrawTree(this.Tree);
            await this.controller.ProgressDialogController.CloseAsync();
            //}
            //catch (Exception exception)
            //{
            //    MessageBox.Show(exception.Message);
            //}
        }

        private TreeParameters CreateTreeParameters()
        {
            return new TreeParameters
            {
                TreeTrunkSize = this.TreeTrunkSize,
                TrunkRotationAngle = this.TrunkRotationAngle,
                TrunkRotationAngleStart = this.TrunkRotationAngle,
                TrunkSkewAngle = this.TrunkSkewAngle,
                TrunkSkewAngleStart = this.TrunkSkewAngleStart,
                BranchCount = this.BranchCount,
                BranchStart = this.BranchStart,
                BranchLengthMin = this.BranchLengthMin,
                BranchLengthMax = this.BranchLengthMax,
                BranchDistance = this.BranchDistance,
                BranchSkew = this.BranchSkew,
                BranchSkewDeviation = this.BranchSkewDeviation,
                BranchRotationAngleStart = this.BranchRotationAngleStart,
                BranchRotationAngle = this.BranchRotationAngle,
                TrunkWidthStart = this.TrunkWidthStart,
                TrunkWidthEnd = this.TrunkWidthEnd,
                TrunkColor = new WPFColorWrapper(this.TrunkColor),
                OutlineColor = new WPFColorWrapper(this.OutlineColor),
                BranchLevelLengthFactor = this.BranchLevelLengthFactor,
                BranchMaxLevel = this.BranchMaxLevel,
                RandomSeed = this.RandomSeed,
            };
        }

        private Task<TreeModel<WpfTreeVisualWrapper>> GenerateAndDrawTreeAsync()
        {
            return Task.Run(
                () =>
                    {
                        TreeGenerator<WpfTreeVisualWrapper> generator = new TreeGenerator<WpfTreeVisualWrapper>();
                        generator.Initialize(this.Parameters, this.controller, this.RandomSeed);
                        var tree = generator.GenerateTree();
                        return tree;
                    });
        }

        private void ManageRandom()
        {
            if (this.RegenerateRandomSeed || this.rand == null)
            {
                var seed = this.randomRand.Next();
                this.RandomSeed = seed;
            }

            this.rand = new Random(this.RandomSeed);
        }

        private void ExportImage()
        {
            SaveFileDialog dialog = new SaveFileDialog();
            if (dialog.ShowDialog() == true)
            {
                this.CreatePng(dialog.FileName, this.ImageTree);
            }
        }

        private void CreatePng(string filename, BitmapSource image)
        {
            if (filename != string.Empty)
            {
                using (FileStream stream = new FileStream(filename, FileMode.Create))
                {
                    PngBitmapEncoder encoder5 = new PngBitmapEncoder();
                    encoder5.Frames.Add(BitmapFrame.Create(image));
                    encoder5.Save(stream);
                }
            }
        }
    }

    public class WPFProgressController : IProgress<string>
    {
        public ProgressDialogController ProgressDialogController { get; set; }

        public void Report(string value)
        {
            this.ProgressDialogController?.SetMessage(value);
        }
    }
}