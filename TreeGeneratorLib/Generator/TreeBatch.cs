using MathNet.Spatial.Euclidean;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TreeGeneratorLib.Wrappers;

namespace TreeGeneratorLib.Generator
{
    public class TreeBatch<T> : IList<TreeBatchPosition<T>> where T : TreeVisual
    {
        public TreeBatchPosition<T> this[int index] { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public List<TreeBatchPosition<T>> TreePositions { get; set; } = new List<TreeBatchPosition<T>>();

        public int Count => throw new NotImplementedException();

        public bool IsReadOnly => throw new NotImplementedException();

        public void Add(TreeBatchPosition<T> item)
        {
            this.TreePositions.Add(item);
        }

        public void Clear()
        {
            this.TreePositions.Clear();
        }

        public bool Contains(TreeBatchPosition<T> item)
        {
            return this.TreePositions.Contains(item);
        }

        public void CopyTo(TreeBatchPosition<T>[] array, int arrayIndex)
        {
            this.TreePositions.CopyTo(array, arrayIndex);
        }

        public IEnumerator<TreeBatchPosition<T>> GetEnumerator()
        {
            return this.TreePositions.GetEnumerator();
        }

        public int IndexOf(TreeBatchPosition<T> item)
        {
            return this.TreePositions.IndexOf(item);
        }

        public void Insert(int index, TreeBatchPosition<T> item)
        {
            this.TreePositions.Insert(index, item);
        }

        public bool Remove(TreeBatchPosition<T> item)
        {
            return this.TreePositions.Remove(item);
        }

        public void RemoveAt(int index)
        {
            this.TreePositions.RemoveAt(index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)this.TreePositions).GetEnumerator();
        }
    }
}
