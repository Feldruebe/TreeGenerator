using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TreeGeneratorLib.Wrappers;

namespace TreeGeneratorLib.Generator
{
    public class BatchGenerator<T> where T : TreeVisual
    {
        public List<TreeBatchPosition<T>> GenerateBatch(BatchParameters batchParameters)
        {
            List<TreeBatchPosition<T>> trees = new List<TreeBatchPosition<T>>();
            Queue<(int min, int max)> ranges = new Queue<(int min, int max)>(new[] { (0, batchParameters.BatchWidth) });
            for (int i = 0; i < batchParameters.TreeCount; i++)
            {
                var randomPick = new Random().Next(batchParameters.TreeParameters.Count);
                var treeParameter = batchParameters.TreeParameters[randomPick];
                var seed = treeParameter.RandomSeed ?? new Random().Next();
                TreeGenerator<T> generator = new TreeGenerator<T>();
                generator.Initialize(treeParameter, null, seed);
                var tree = generator.GenerateTree();

                var range = ranges.Dequeue();
                var xPosition = new Random().Next(range.min, range.max);
                ranges.Enqueue((range.min, xPosition));
                ranges.Enqueue((xPosition, range.max));
                var rangeList = ranges.ToList();
                rangeList.Sort((range1, range2) => (range1.max - range1.min) - (range2.max - range2.min));
                ranges = new Queue<(int min, int max)>(rangeList);

                var newBatchPosition = new TreeBatchPosition<T> { XPosition = xPosition, Tree = tree, TreeParameter = treeParameter };
                trees.Add(newBatchPosition);
            }

            return trees;
        }
    }
}
