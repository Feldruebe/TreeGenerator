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

        public List<TreeBatchPosition<T>> GenerateBatch(BatchParameters batchParameters)
        {
            if(!batchParameters.BatchTreeParameters.Any())
            {
                return new List<TreeBatchPosition<T>>();
            }

            List<TreeBatchPosition<T>> trees = new List<TreeBatchPosition<T>>();
            Queue<(int min, int max)> ranges = new Queue<(int min, int max)>(new[] { (0, batchParameters.BatchWidth) });
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
                        break;
                    }
                }
                
                var treeBatchParameter = batchParameters.BatchTreeParameters[randomIndex];
                var seed = !treeBatchParameter.UseNewSeed ? treeBatchParameter.TreeParameters.RandomSeed : new Random().Next();
                TreeGenerator<T> generator = new TreeGenerator<T>();
                generator.Initialize(treeBatchParameter.TreeParameters, this.progress, seed);
                var tree = generator.GenerateTree();

                var range = ranges.Dequeue();
                int min = range.min;
                int max = range.max;

                if(max - min > batchParameters.BatchTreeDistance * 2)
                {
                    max -= batchParameters.BatchTreeDistance;
                    min += batchParameters.BatchTreeDistance;
                }
                else
                {
                    int k = 0;
                    k++;
                }

                var xPosition = new Random().Next(min, max);
                ranges.Enqueue((range.min, xPosition));
                ranges.Enqueue((xPosition, range.max));
                var rangeList = ranges.ToList();
                rangeList.Sort((range1, range2) => (range1.max - range1.min) - (range2.max - range2.min));
                rangeList.Reverse();
                ranges = new Queue<(int min, int max)>(rangeList);

                var newBatchPosition = new TreeBatchPosition<T> { XPosition = xPosition, Tree = tree, TreeParameter = treeBatchParameter.TreeParameters };
                trees.Add(newBatchPosition);
            }

            return trees;
        }
    }
}
