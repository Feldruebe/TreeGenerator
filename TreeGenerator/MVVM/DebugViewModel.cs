namespace TreeGeneratorWPF.ViewModels
{
    using GalaSoft.MvvmLight;

    public class DebugViewModel : ViewModelBase
    {
        private bool drawTrunk;

        private int branchToDraw;

        private MainViewModel mainViewModel;

        private bool debugModeEnabled;

        public DebugViewModel(MainViewModel mainViewModel)
        {
            this.mainViewModel = mainViewModel;
        }


        public bool DrawTrunk
        {
            get
            {
                return this.drawTrunk;
            }

            set
            {
                this.Set(ref this.drawTrunk, value);
                this.mainViewModel.RedrawTree();
            }
        }

        public int BranchToDraw
        {
            get
            {
                return this.branchToDraw;
            }

            set
            {
                this.Set(ref this.branchToDraw, value);
                this.mainViewModel.RedrawTree();
            }
        }

        public bool DebugModeEnabled
        {
            get
            {
                return this.debugModeEnabled;
            }

            set
            {
                this.Set(ref this.debugModeEnabled, value);
                this.mainViewModel.RedrawTree();
            }
        }
    }
}