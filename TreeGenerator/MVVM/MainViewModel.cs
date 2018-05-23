using System.Collections.Generic;
using System.Linq;

namespace TreeGeneratorWPF.ViewModels
{
    using System;
    using System.Windows.Media.Imaging;

    using GalaSoft.MvvmLight;

    using GalaSoft.MvvmLight.CommandWpf;
    using System.IO;
    using System.Threading.Tasks;
    using System.Collections.ObjectModel;
    using System.Windows.Media;

    using MahApps.Metro.Controls.Dialogs;

    using SkiaSharp;

    using Microsoft.Win32;

    using Color = System.Windows.Media.Color;

    using TreeGeneratorLib.Generator;
    using TreeGeneratorLib.Tree;
    using TreeGeneratorLib.Wrappers;
    using TreeGeneratorWPF.Wrapper;
    using TreeGeneratorWPF.MVVM;
    using System.Windows;
    using System.Text;
    using Newtonsoft.Json;

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
        private int branchMinLevel;
        private int branchMaxLevel;
        private int randomSeed;

        private bool regenerateRandomSeed;
        private bool debugModeEnabled;

        private TreeModel<WpfTreeVisualWrapper> tree;
        private WPFProgressController controller = new WPFProgressController();
        private ObservableCollection<LeafImageViewModel> leafImageViewModels = new ObservableCollection<LeafImageViewModel>();

        private int leafDistance;

        private float leafPropability;

        private int leafDistanceDeviation;
        private bool showSkeleton;
        private bool leafAntialising;

        

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {
            this.DebugViewModel = new DebugViewModel(this);
#if DEBUG
            this.DebugViewModel.DebugModeEnabled = true;
#endif

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
            this.BranchMinLevel = 1;
            this.BranchLevelLengthFactor = 1;
            this.LeafDistance = 15;

            var defaultLeafImage = new LeafImageViewModel(@"pack://application:,,,/TreeGeneratorWPF;component/Images/Blatt.png", false)
            {
                IsIncluded = true,
                Probability = 1,
                Scale = 1,
                ScaleDeviation = 0
            };
            this.LeafImageViewModels.Add(defaultLeafImage);

            this.BatchViewModel = new BatchViewModel(this.controller);
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
                this.RedrawTree();
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
                this.RedrawTree();
            }
        }

        public RelayCommand GenerateTreeCommand => new RelayCommand(this.GenerateTreeAndDraw);          

        public RelayCommand ExportImageCommand => new RelayCommand(this.ExportImage);

        public RelayCommand SaveParametersCommand => new RelayCommand(this.SaveParameters);

        public RelayCommand LoadParametersCommand => new RelayCommand(this.LoadParameters);

        public RelayCommand LoadLeafImageCommand => new RelayCommand(this.LoadLeafImage);

        public RelayCommand AddToBatchCommand => new RelayCommand(this.AddToBatch);

        public RelayCommand<BatchTreeViewModel> BatchTreeRestoreCommand => new RelayCommand<BatchTreeViewModel>(this.RestoreBatchTree);
        
        public RelayCommand<LeafImageViewModel> DeleteLeafCommand => new RelayCommand<LeafImageViewModel>(this.Delete);

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

        public BatchViewModel BatchViewModel { get; }

        public TreeModel<WpfTreeVisualWrapper> Tree
        {
            get
            {
                return this.tree;
            }

            private set
            {
                this.Set(ref this.tree, value);
                this.RaisePropertyChanged(nameof(this.AllBranchesCount));
            }
        }

        public TreeParameters Parameters { get; set; }

        public int BranchMaxLevel
        {
            get { return this.branchMaxLevel; }
            set { this.Set(ref this.branchMaxLevel, value); }
        }

        public int BranchMinLevel
        {
            get { return this.branchMinLevel; }
            set { this.Set(ref this.branchMinLevel, value); }
        }

        public ObservableCollection<LeafImageViewModel> LeafImageViewModels
        {
            get => leafImageViewModels;
            set => this.Set(ref leafImageViewModels, value);
        }

        public int LeafDistance
        {
            get => this.leafDistance;
            set => this.Set(ref this.leafDistance, value);
        }

        public int LeafDistanceDeviation
        {
            get => this.leafDistanceDeviation;
            set => this.Set(ref this.leafDistanceDeviation, value);
        }

        public bool ShowSkeleton
        {
            get => this.showSkeleton;
            set => this.Set(ref this.showSkeleton, value);
        }

        public bool LeafAntialising
        {
            get => leafAntialising;
            set => this.Set(ref leafAntialising, value);
        }

        public void RedrawTree()
        {
            this.Parameters = this.CreateTreeParameters();
            this.Tree?.TreeVisual.DrawTree(this.Tree, this.Parameters);
            this.RaisePropertyChanged(nameof(this.Tree));
        }

        private async void GenerateTreeAndDraw()
        {
            //try
            //{
            this.controller.ProgressDialogController = await DialogCoordinator.Instance.ShowProgressAsync(this, "Waiting...", "Wait", true);
            this.ManageRandom();
            this.Parameters = this.CreateTreeParameters();
            var tree = await this.GenerateTreeAsync();
            tree?.TreeVisual.DrawTree(tree, this.Parameters);
            this.Tree = tree;

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
                TrunkRotationAngleStart = this.TrunkRotationAngleStart,
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
                BranchMinLevel = this.BranchMinLevel,
                RandomSeed = this.RandomSeed,
                LeafParameters = this.GetLeafParameters(),
                LeafDistance = this.LeafDistance,
                LeafDistanceDeviation = this.LeafDistanceDeviation,
                LeafAntialising = this.LeafAntialising,
            };
        }

        private void RestorePrameters(TreeParameters treeParameters)
        {
            this.TreeTrunkSize = treeParameters.TreeTrunkSize;
            this.TrunkRotationAngle = treeParameters.TrunkRotationAngle;
            this.TrunkRotationAngleStart = treeParameters.TrunkRotationAngleStart;
            this.TrunkSkewAngle = treeParameters.TrunkSkewAngle;
            this.TrunkSkewAngleStart = treeParameters.TrunkSkewAngleStart;
            this.BranchCount = treeParameters.BranchCount;
            this.BranchStart = treeParameters.BranchStart;
            this.BranchLengthMin = treeParameters.BranchLengthMin;
            this.BranchLengthMax = treeParameters.BranchLengthMax;
            this.BranchDistance = treeParameters.BranchDistance;
            this.BranchSkew = treeParameters.BranchSkew;
            this.BranchSkewDeviation = treeParameters.BranchSkewDeviation;
            this.BranchRotationAngleStart = treeParameters.BranchRotationAngleStart;
            this.BranchRotationAngle = treeParameters.BranchRotationAngle;
            this.TrunkWidthStart = treeParameters.TrunkWidthStart;
            this.TrunkWidthEnd = treeParameters.TrunkWidthEnd;
            this.TrunkColor = Color.FromArgb(treeParameters.TrunkColor.A, treeParameters.TrunkColor.R, treeParameters.TrunkColor.G, treeParameters.TrunkColor.B);
            this.OutlineColor = Color.FromArgb(treeParameters.OutlineColor.A, treeParameters.OutlineColor.R, treeParameters.OutlineColor.G, treeParameters.OutlineColor.B);
            this.BranchLevelLengthFactor = treeParameters.BranchLevelLengthFactor;
            this.BranchMaxLevel = treeParameters.BranchMaxLevel;
            this.BranchMinLevel = treeParameters.BranchMinLevel;
            this.RandomSeed = treeParameters.RandomSeed ?? -1;
            this.LeafDistance = treeParameters.LeafDistance;
            this.LeafDistanceDeviation = treeParameters.LeafDistanceDeviation;
            this.LeafAntialising = treeParameters.LeafAntialising;

            this.LeafImageViewModels = new ObservableCollection<LeafImageViewModel>(this.LeafImageViewModels.Where(vm => !vm.CanBeDeleted));
            foreach (var vm in this.LeafImageViewModels)
            {
                vm.IsIncluded = false;
            }

            var loadedLeafParameters = treeParameters.LeafParameters.Select(p =>
            {
                var bitmap = new BitmapImage();
                using (MemoryStream stream = new MemoryStream(p.ImageBuffer))
                {
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.StreamSource = stream;
                    bitmap.EndInit();
                }

                return new LeafImageViewModel(bitmap)
                {
                    CanBeDeleted = true,
                    IsIncluded = true,
                    Probability = p.Probability,
                    Scale = p.Scale,
                    ScaleDeviation = p.SacleDeviation
                };
            });

            foreach (var vm in loadedLeafParameters)
            {
                this.LeafImageViewModels.Add(vm);
            }
        }

        private Task<TreeModel<WpfTreeVisualWrapper>> GenerateTreeAsync()
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

        private void SaveParameters()
        {
            SaveFileDialog save = new SaveFileDialog();
            save.AddExtension = true;
            save.DefaultExt = "*.tree";
            save.Filter = "*.tree|*.tree";
            var result = save.ShowDialog();
            if (result == true)
            {
                using (FileStream stream = new FileStream(save.FileName, FileMode.Create))
                {
                    var parametersForSave = this.CreateTreeParameters();
                    var settings = new JsonSerializerSettings()
                    {
                        TypeNameHandling = TypeNameHandling.All
                    };
                    var parametersString = JsonConvert.SerializeObject(parametersForSave, Formatting.Indented, settings);
                    var parametersBytes = new ASCIIEncoding().GetBytes(parametersString);
                    stream.Write(parametersBytes, 0, parametersBytes.Length);
                }
            }
        }

        private void LoadParameters()
        {
            OpenFileDialog open = new OpenFileDialog();
            open.AddExtension = true;
            open.DefaultExt = "*.tree";
            open.Filter = "*.tree|*.tree";
            var result = open.ShowDialog();
            if (result == true)
            {
                string fileContens = File.ReadAllText(open.FileName);
                var settings = new JsonSerializerSettings()
                {
                    TypeNameHandling = TypeNameHandling.All
                };
                var treeParameters = JsonConvert.DeserializeObject<TreeParameters>(fileContens, settings);
                this.RestorePrameters(treeParameters);
            }
        }

        private void ExportImage()
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.AddExtension = true;
            dialog.DefaultExt = "*.png";
            dialog.Filter = "*.png|*.png";
            if (dialog.ShowDialog() == true)
            {
                this.CreatePng(dialog.FileName, this.Tree.TreeVisual.TreeIamge);
            }
        }

        private void CreatePng(string filename, BitmapSource image)
        {
            if (filename != string.Empty)
            {
                using (FileStream stream = new FileStream(filename, FileMode.Create))
                {
                    PngBitmapEncoder encoderPNG = new PngBitmapEncoder();
                    encoderPNG.Frames.Add(BitmapFrame.Create(image));
                    encoderPNG.Save(stream);
                }
            }
        }

        private void LoadLeafImage()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            var result = openFileDialog.ShowDialog();

            if (result == true)
            {
                var leafImageviewModel = new LeafImageViewModel(openFileDialog.FileName);
                this.LeafImageViewModels.Add(leafImageviewModel);
            }
        }

        private void Delete(LeafImageViewModel image)
        {
            this.LeafImageViewModels.Remove(image);
        }

        private List<LeafParameter> GetLeafParameters()
        {
            var result = new List<LeafParameter>();
            foreach (var leafViewModel in this.LeafImageViewModels.Where(p => p.IsIncluded))
            {
                var leafParameter = new LeafParameter
                {
                    ImageBuffer = GetPngBytesFromImageControl(leafViewModel.LoadedImage),
                    Probability = leafViewModel.Probability,
                    Scale = leafViewModel.Scale,
                    SacleDeviation = leafViewModel.ScaleDeviation,
                };

                result.Add(leafParameter);
            }

            return result;
        }

        public static byte[] GetPngBytesFromImageControl(BitmapSource imageC)
        {
            MemoryStream memStream = new MemoryStream();
            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(imageC));
            encoder.Save(memStream);
            return memStream.ToArray();
        }

        private void AddToBatch()
        {
            var batchTreeViewModel = new BatchTreeViewModel { Name = this.Parameters.RandomSeed.ToString(), Thumbnail = this.Tree.TreeVisual.TreeIamge.Clone(), Parameters = this.Parameters };
            this.BatchViewModel.BatchTrees.Add(batchTreeViewModel);
        }

        private void RestoreBatchTree(BatchTreeViewModel batchTreeViewModel)
        {
            this.RestorePrameters(batchTreeViewModel.Parameters);
        }
    }

    public class WPFProgressController : ICancelableProgress
    {
        public ProgressDialogController ProgressDialogController { get; set; }

        public void Report(string value)
        {
            this.ProgressDialogController?.SetMessage(value);
        }

        public bool CancelRequested()
        {
            return ProgressDialogController.IsCanceled;
        }
    }
}