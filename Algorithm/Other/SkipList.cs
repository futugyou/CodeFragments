using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Other
{
    public class SkipList
    {
        public static void Exection()
        {
            SkipList list = new SkipList();
            list.Insert(50);
            list.Insert(15);
            list.Insert(13);
            list.Insert(20);
            list.Insert(100);
            list.Insert(75);
            list.Insert(99);
            list.Insert(76);
            list.Insert(83);
            list.Insert(65);
            list.PrintLevel0List();
            list.Search(50);
            list.Remove(50);
            list.Search(50);
            list.PrintLevel0List();
        }

        public class Node
        {
            public int Data { get; set; }
            public Node Up, Down, Left, Right;
            public Node(int data)
            {
                Data = data;
            }
        }

        private const double RATE = 0.5;
        private Node head, tail;
        private int maxLevel;

        public SkipList()
        {
            head = new Node(int.MinValue);
            tail = new Node(int.MaxValue);
            head.Right = tail;
            tail.Left = head;
        }

        public Node Search(int data)
        {
            Node p = FindNode(data);
            if (p.Data == data)
            {
                Console.WriteLine("found node:" + data);
                return p;
            }
            Console.WriteLine("can not found:" + data);
            return null;
        }

        private Node FindNode(int data)
        {
            Node node = head;
            while (true)
            {
                while (node.Right.Data != int.MaxValue && node.Right.Data <= data)
                {
                    node = node.Right;
                }
                if (node.Down == null)
                {
                    break;
                }
                node = node.Down;
            }
            return node;
        }

        public void Insert(int data)
        {
            Node preNode = FindNode(data);
            if (preNode.Data == data)
            {
                return;
            }
            Node node = new Node(data);
            AppendNode(preNode, node);
            int currentLevel = 0;
            Random random = new Random();
            while (random.NextDouble() < RATE)
            {
                if (currentLevel == maxLevel)
                {
                    AddLevel();
                }
                while (preNode.Up == null)
                {
                    preNode = preNode.Left;
                }
                preNode = preNode.Up;
                Node upperNode = new Node(data);
                AppendNode(preNode, upperNode);
                upperNode.Down = node;
                node.Up = upperNode;
                node = upperNode;
                currentLevel++;
            }
        }

        private void AddLevel()
        {
            maxLevel++;
            Node p1 = new Node(int.MinValue);
            Node p2 = new Node(int.MaxValue);
            p1.Right = p2;
            p2.Left = p1;
            p1.Down = head;
            head.Up = p1;
            p2.Down = tail;
            tail.Down = p2;
            head = p1;
            tail = p2;
        }

        private void AppendNode(Node preNode, Node node)
        {
            node.Left = preNode;
            node.Right = preNode.Right;
            preNode.Right.Left = node;
            preNode.Right = node;
        }

        public bool Remove(int data)
        {
            Node removeNode = FindNode(data);
            if (removeNode == null)
            {
                return false;
            }
            int currentLevel = 0;
            while (removeNode != null)
            {
                removeNode.Left.Right = removeNode.Right;
                removeNode.Right.Left = removeNode.Left;
                if (currentLevel != 0 && removeNode.Left.Data == int.MinValue && removeNode.Right.Data == int.MaxValue)
                {
                    RemoveLevel(removeNode.Left);
                }
                else
                {
                    currentLevel++;
                }
                removeNode = removeNode.Up;
            }
            return true;
        }

        private void RemoveLevel(Node left)
        {
            Node right = left.Right;
            if (left.Up == null)
            {
                left.Down.Up = null;
                right.Down.Up = null;
            }
            else
            {
                left.Up.Down = left.Down;
                left.Down.Up = left.Up;
                right.Up.Down = right.Down;
                right.Down.Up = right.Up;
            }
            maxLevel--;
        }

        public void PrintLevel0List()
        {
            Node node = head;
            while (node.Down != null)
            {
                node = node.Down;
            }
            while (node.Right.Data != int.MaxValue)
            {
                Console.Write(node.Right.Data + " ");
                node = node.Right;
            }
            Console.WriteLine();
        }
    }
}
