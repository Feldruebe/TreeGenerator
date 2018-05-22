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
        public List<TreeBatchPosition> GenerateBatch(BatchParameters batchParameters)
        {
            List<TreeBatchPosition> trees = new List<TreeBatchPosition>();
            for (int i = 0; i < batchParameters.TreeCount; i++)
            {

                var randomPick = new Random().Next(batchParameters.TreeParameters.Count);
                var treeParameter = batchParameters.TreeParameters[randomPick];
                var seed = treeParameter.RandomSeed ?? new Random().Next();
                TreeGenerator<T> generator = new TreeGenerator<T>();
                generator.Initialize(treeParameter, null, seed);
                var tree = generator.GenerateTree();


                var newBatchPosition = new TreeBatchPosition { XPosition = new Random().Next(batchParameters.BatchWidth), TreeVisual = tree.TreeVisual, };
                trees.Add(newBatchPosition);
            }

            return trees;
        }
    }
}
