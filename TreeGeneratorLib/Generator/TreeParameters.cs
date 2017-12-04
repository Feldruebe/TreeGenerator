using System.Drawing;
using TreeGeneratorLib.Wrappers;

namespace TreeGeneratorLib.Generator
{
    using System;

    public class TreeParameters
    {
        public int TreeTrunkSize { get; set; }

        public int TrunkRotationAngle { get; set; }

        public float TrunkRotationAngleStart { get; set; }

        public int TrunkSkewAngle { get; set; }

        public int TrunkSkewAngleStart { get; set; }

        public int BranchCount { get; set; }

        public int BranchStart { get; set; }

        public int BranchLengthMin { get; set; }

        public int BranchLengthMax { get; set; }

        public int BranchDistance { get; set; }

        public int BranchSkew { get; set; }

        public int BranchSkewDeviation { get; set; }

        public int BranchRotationAngleStart { get; set; }

        public float BranchRotationAngle { get; set; }

        public int TrunkWidthStart { get; set; }

        public int TrunkWidthEnd { get; set; }

        public IColor TrunkColor { get; set; }

        public IColor OutlineColor { get; set; }

        public double BranchLevelLengthFactor { get; set; }

        public int BranchMaxLevel { get; set; }

        public int BranchMinLevel { get; set; }

        public int RandomSeed { get; set; }
    }
}