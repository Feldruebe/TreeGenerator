namespace TreeGeneratorLib.Tree
{
    using System.Collections.Generic;
    using System.Linq;

    using MathNet.Spatial.Euclidean;

    public class TreeModel
    {
        public Branch Trunk { get; } = new Branch();

        public List<Branch> Branches { get; } = new List<Branch>();

        public IList<Point2D> AllBorderPoints
        {
            get
            {
                return this.Branches.SelectMany(branch => branch.LeftAndRightBorderPoints).Concat(this.Trunk.LeftAndRightBorderPoints).ToList();
            }
        }

        public IList<TreePoint> AllTreeSkelletonPoints
        {
            get
            {
                return this.Branches.SelectMany(branch => branch.SkelletonPoints).Concat(this.Trunk.SkelletonPoints).ToList();
            }
        }

        public IList<Point2D> ContourPoints
        {
            get
            {
                return this.Trunk.ContourPoints.Concat(this.Branches.SelectMany(branch => branch.ContourPoints)).ToList();
            }
        }

        public IList<Point2D> ContourPointsWithoutBot
        {
            get
            {
                return this.Trunk.ContourPoints.Concat(this.Branches.SelectMany(branch => branch.ContourPointsWithoutBot)).ToList();
            }
        }

        public IList<Point2D> FillPoints
        {
            get
            {
                return this.Trunk.FillPoints.Concat(this.Branches.SelectMany(branch => branch.FillPoints)).ToList();
            }
        }

        public void GenerateContourAndFillPoints()
        {
            this.Trunk.GenerateContour();
            this.Branches.ForEach(branch => branch.GenerateContour());

            this.Trunk.GenerateFillPoints();
            this.Branches.ForEach(branch => branch.GenerateFillPoints());
        }

        public void GenerateSDF()
        {
            this.Trunk.GenerateSDF();
            this.Branches.ForEach(branch => branch.GenerateSDF());
        }
    }
}