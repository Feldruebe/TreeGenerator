﻿namespace TreeGenerator.ViewModels
{
    using System;
    using System.Linq.Expressions;
    using System.Windows.Media.Imaging;
    using GalaSoft.MvvmLight;
    using System.Windows.Media;

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
        private int treeTrunkSize;
        private int imageHeight;
        private int imageWidth;
        private WriteableBitmap imageSkeletton;
        private WriteableBitmap imageTree;

        private int rotationAngle;
        private float rotationAngleStart;

        private int skewAngle;
        private int skewAngleStart;
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
        private Color trunkColor;
        private Color branchColor;
        private Color outlineColor = false;


        public int TreeTrunkSize
        {
            get { return this.treeTrunkSize; }
            set { this.Set(ref this.treeTrunkSize, value); }
        }

        public int ImageHeight
        {
            get { return this.imageHeight; }
            set { this.Set(ref this.imageHeight, value); }
        }

        public int ImageWidth
        {
            get { return this.imageWidth; }

            set { this.Set(ref this.imageWidth, value); }
        }

        public WriteableBitmap ImageSkeletton
        {
            get { return this.imageSkeletton; }
            set { this.Set(ref this.imageSkeletton, value); }
        }

        public WriteableBitmap ImageTree
        {
            get { return this.imageTree; }
            set { this.Set(ref this.imageTree, value); }
        }

        public int RotationAngle
        {
            get { return this.rotationAngle; }
            set { this.Set(ref this.rotationAngle, value); }
        }

        public float RotationAngleStart
        {
            get { return this.rotationAngleStart; }

            set { this.Set(ref this.rotationAngleStart, value); }
        }

        public int SkewAngle
        {
            get { return this.skewAngle; }
            set { this.Set(ref this.skewAngle, value); }
        }

        public int SkewAngleStart
        {
            get { return this.skewAngleStart; }
            set { this.Set(ref this.skewAngleStart, value); }
        }

        public int BranchCount
        {
            get { return this.branchCount; }
            set { this.Set(ref this.branchCount, value); }
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

        public int BranchSkew
        {
            get { return this.branchSkew; }

            set { this.Set(ref this.branchSkew, value); }
        }

        public int BranchSkewDeviation
        {
            get { return branchSkewDeviation; }
            set { this.Set(ref branchSkewDeviation, value); }
        }

        public int BranchRotationAngleStart
        {
            get { return branchRotationAngleStart; }
            set { this.Set(ref branchRotationAngleStart, value); }
        }

        public float BranchRotationAngle
        {
            get { return branchRotationAngle; }
            set { this.Set(ref branchRotationAngle, value); }
        }

        public int TrunkWidthStart
        {
            get { return trunkWidthStart; }
            set { this.Set(ref trunkWidthStart, value); }
        }

        public int TrunkWidthEnd
        {
            get { return trunkWidthEnd; }
            set { this.Set(ref trunkWidthEnd, value); }
        }
                
        public Color TrunkColor
        {
            get
            {
                return trunkColor;
            }
            set
            {
                Set(ref trunkColor, value);
            }
        }    

        public Color BranchColor
        {
            get
            {
                return branchColor;
            }

            set
            {
                Set(ref branchColor, value);
            }
        }

        public Color OutlineColor
        {
            get
            {
                return this.outlineColor;
            }
            set
            {
                Set(ref outlineColor, value);
            }
        }

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {
            this.ImageWidth = 60;
            this.ImageHeight = 100;
            this.TreeTrunkSize = 2 * this.ImageHeight / 3;
            this.SkewAngle = 0;
            this.SkewAngleStart = 0;
            this.RotationAngle = 0;
            this.RotationAngleStart = 0;
            this.BranchCount = 4;
            this.BranchStart = 2 * this.TreeTrunkSize / 3;
            this.BranchLengthMin = this.TreeTrunkSize / 6;
            this.BranchLengthMax = this.TreeTrunkSize / 3;
            this.BranchDistance = 1;
            this.BranchSkew = 35;
            this.BranchSkewDeviation = 10;
            this.BranchRotationAngleStart = 0;
            this.BranchRotationAngle = 30;
            this.TrunkWidthStart = 8;
            this.TrunkWidthEnd = 2;
            this.TrunkColor = Colors.SaddleBrown;
            this.BranchColor = Colors.SaddleBrown;
            this.OutlineColor = Colors.Black;

        }
    }
}