using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Other
{
    public class HuffmanNode
    {
        public int key { get; set; }              // 权值
        public HuffmanNode left;     // 左孩子
        public HuffmanNode right;    // 右孩子
        public HuffmanNode parent;   // 父结点

        public HuffmanNode(int key, HuffmanNode left, HuffmanNode right, HuffmanNode parent)
        {
            this.key = key;
            this.left = left;
            this.right = right;
            this.parent = parent;
        }
    }

    public class MinHeap
    {
        private List<HuffmanNode> _mHeap;
        public MinHeap(int[] a)
        {
            _mHeap = new List<HuffmanNode>();
            for (int i = 0; i < a.Length; i++)
            {
                var node = new HuffmanNode(a[i], null, null, null);
                _mHeap.Add(node);
            }
            for (int i = a.Length / 2 - 1; i >= 0; i--)
            {
                FilterDown(i, a.Length - 1);
            }
        }

        private void FilterDown(int start, int end)
        {
            var c = start;
            var left = 2 * c + 1;
            var tmp = _mHeap[c];
            while (1 <= end)
            {
                if (1 < end && _mHeap[left].key > _mHeap[left + 1].key)
                {
                    left++;
                }

                var cmp = tmp.key - _mHeap[left].key;
                if (cmp <= 0)
                {
                    break;
                }
                else
                {
                    _mHeap[c] = _mHeap[left];
                    c = left;
                    left = 2 * left + 1;
                }
            }
            _mHeap[c] = tmp;
        }

        private void FilterUp(int start)
        {
            var c = start;
            var p = (c - 1) / 2;
            var tmp = _mHeap[c];
            while (c > 0)
            {
                var cmp = _mHeap[p].key - tmp.key;
                if (cmp <= 0)
                {
                    break;
                }
                else
                {
                    _mHeap[c] = _mHeap[p];
                    c = p;
                    p = (p - 1) / 2;
                }
            }
            _mHeap[c] = tmp;
        }

        public void Insert(HuffmanNode node)
        {
            var size = _mHeap.Count;
            _mHeap.Add(node);
            FilterUp(size);
        }

        private void SwapNode(int l, int j)
        {
            var tmp = _mHeap[l];
            _mHeap[l] = _mHeap[j];
            _mHeap[j] = tmp;
        }

        public HuffmanNode DumpFromMinimum()
        {
            int size = _mHeap.Count;
            if (size == 0)
                return null;
            HuffmanNode node = _mHeap[0];

            // 交换"最小节点"和"最后一个节点"
            _mHeap[0] = _mHeap[size - 1];
            // 删除最后的元素
            _mHeap.RemoveAt(size - 1);

            if (_mHeap.Count > 1)
                FilterDown(0, _mHeap.Count - 1);

            return node;
        }
        public void Destroy()
        {
            _mHeap.Clear();
        }
    }

    public class Huffman
    {
        private HuffmanNode Root;

        public Huffman(int[] a)
        {
            HuffmanNode parent = null;
            MinHeap heap = new MinHeap(a);
            for (int i = 0; i < a.Length - 1; i++)
            {
                var left = heap.DumpFromMinimum();
                var right = heap.DumpFromMinimum();
                parent = new HuffmanNode(left.key + right.key, left, right, null);
                left.parent = parent;
                right.parent = parent;
                heap.Insert(parent);
            }
            Root = parent;
            heap.Destroy();
        }
        private void PreOrder(HuffmanNode tree)
        {
            if (tree != null)
            {
                Console.Write(tree.key + " ");
                PreOrder(tree.left);
                PreOrder(tree.right);
            }
        }

        public void PreOrder()
        {
            PreOrder(Root);
        }
        private void InOrder(HuffmanNode tree)
        {
            if (tree != null)
            {
                InOrder(tree.left);
                Console.Write(tree.key + " ");
                InOrder(tree.right);
            }
        }

        public void InOrder()
        {
            InOrder(Root);
        }
        private void PostOrder(HuffmanNode tree)
        {
            if (tree != null)
            {
                PostOrder(tree.left);
                PostOrder(tree.right);
                Console.Write(tree.key + " ");
            }
        }

        public void PostOrder()
        {
            PostOrder(Root);
        }
        private void Destroy(HuffmanNode tree)
        {
            if (tree == null)
                return;

            if (tree.left != null)
                Destroy(tree.left);
            if (tree.right != null)
                Destroy(tree.right);

            tree = null;
        }

        public void Destroy()
        {
            Destroy(Root);
            Root = null;
        }
        public void Print()
        {
            if (Root != null)
                Print(Root, Root.key, 0);
        }
        private void Print(HuffmanNode tree, int key, int direction)
        {

            if (tree != null)
            {

                if (direction == 0)
                {
                    Console.WriteLine($"{tree.key} is root ");
                }
                else
                {
                    var str = direction == 1 ? "right" : "left";
                    Console.WriteLine($"{tree.key} is {key}'s {str} child");
                }
                Print(tree.left, tree.key, -1);
                Print(tree.right, tree.key, 1);
            }
        }
    }

    public class HuffmanTest
    {
        private static int[] a = { 5, 6, 8, 7, 15 };
        public static void Exection()
        {
            int i;
            Huffman tree = new Huffman(a);
            for (i = 0; i < a.Length; i++)
            {
                Console.Write(a[i] + " ");
            }
            tree.PreOrder();
        }
    }
}