namespace TreeGenerator.ViewModels
{
    using GalaSoft.MvvmLight;

    public class DebugViewModel : ViewModelBase
    {
        private bool drawTrunk;

        private int branchToDraw;

        private MainViewModel mainViewModel;

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
                this.mainViewModel.ReDrawTree();
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
                this.mainViewModel.ReDrawTree();
            }
        }
    }
}