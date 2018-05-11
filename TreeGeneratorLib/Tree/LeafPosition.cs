using TreeGeneratorLib.Generator;

namespace TreeGeneratorLib.Tree
{
    public class LeafPosition
    {
        public TreePoint PositionInTree { get; set; }

        public double Scale { get; set; }
        public byte[] ImageBuffer { get; set; }
    }
}