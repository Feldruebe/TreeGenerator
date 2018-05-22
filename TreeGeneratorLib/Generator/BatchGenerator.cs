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
        private ICancelableProgress progress;

        public void Initialize(ICancelableProgress progress)
        {
            this.progress = progress;
        }

        public List<TreeBatchPosition> GenerateBatch(BatchParameters batchParameters)
        {
            List<TreeBatchPosition> trees = new List<TreeBatchPosition>();
            for (int i = 0; i < batchParameters.TreeCount; i++)
            {
                var probabilitySum = batchParameters.BatchTreeParameters.Sum(parameter => parameter.Probability);
                int randomIndex = batchParameters.BatchTreeParameters.Count - 1;
                var randomPick = new Random().NextDouble() * probabilitySum;
                for (int j = 0; j < batchParameters.BatchTreeParameters.Count; j++)
                {
                    randomPick -= batchParameters.BatchTreeParameters[j].Probability;
                    if (randomPick < 0)
                    {
                        randomIndex = j;
                    }
                }
                
                var treeBatchParameter = batchParameters.BatchTreeParameters[randomIndex];
                var seed = treeBatchParameter.TreeParameters.RandomSeed ?? new Random().Next();
                TreeGenerator<T> generator = new TreeGenerator<T>();
                generator.Initialize(treeBatchParameter.TreeParameters, this.progress, seed);
                var tree = generator.GenerateTree();

                var newBatchPosition = new TreeBatchPosition { XPosition = new Random().Next(batchParameters.BatchWidth), TreeVisual = tree.TreeVisual, };
                trees.Add(newBatchPosition);
            }

            return trees;
        }
    }
}
