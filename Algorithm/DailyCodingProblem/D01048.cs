namespace DailyCodingProblem;

/// <summary>
/// A Cartesian tree with sequence S is a binary tree defined by the following two properties:
/// It is heap-ordered, so that each parent value is strictly less than that of its children.
/// An in-order traversal of the tree produces nodes with values that correspond exactly to S.
/// For example, given the sequence[3, 2, 6, 1, 9], the resulting Cartesian tree would be:
///      1
///    /   \   
///   2     9
///  / \
/// 3   6
/// Given a sequence S, construct the corresponding Cartesian tree.
/// </summary>
public class D01048
{
    // 1. 用单调栈维护最右链
    // 2. 每次插入当前的i，在单调栈中不停弹出栈顶，直到栈顶fa满足val[fa]<val[i]，则最后一次弹出的就是j
    // 3. 将i作为fa的右儿子，j作为i的左儿子
    // 4. 如果没有找到右链上的节点，那么i就是新根，把原来的树当成i的左儿子
    public static void Exection()
    {
        var nums = new int[] { 3, 2, 6, 1, 9 };
        var stack = new Stack<Tree>();
        Tree? node = null;
        Tree newNode;
        Tree? last = null;
        var n = nums.Length;
        for (int i = 0; i < n; i++)
        {
            newNode = new Tree(i, nums[i]);
            while (stack.Any())
            {
                node = stack.Peek();
                //直到栈顶的元素的Value大于当前结点的Value
                if (node.Value > newNode.Value)
                {
                    //将原来的右子链挂载到newNode的左子树
                    if (node.Right != null)
                    {
                        node.Right.Parent = newNode;
                        newNode.Left = node.Right;
                    }
                    //将新插入的节点插入，作为右链的最后
                    node.Right = newNode;
                    newNode.Parent = node;
                    break;
                }
                last = stack.Pop();
            }
            // 如果当前节点把栈全部弹空了，就要把原先的根节点作为当前节点的左子节点
            if (!stack.Any() && last != null)
            {
                newNode.Left = last;
                last.Parent = newNode;
            }
            stack.Push(newNode);
        }
        while (stack.Any())
        {
            node = stack.Pop();
        }
        Display(node);
    }

    private static void Display(Tree? node)
    {
        if (node == null)
        {
            return;
        }
        Display(node.Left);
        Console.Write(node.Value + " ,");
        Display(node.Right);
    }

    public class Tree
    {
        public Tree(int index, int value)
        {
            Index = index;
            Value = value;
        }
        public int Value { get; set; }
        public int Index { get; set; }
        public Tree Left { get; set; }
        public Tree Right { get; set; }
        public Tree Parent { get; set; }
    }
}


// for(int i=1;i<=n;i++)
//     {
//         int j = 0;
//         while(v.size() && a[v.back()]>a[i])
//         {
//             j=v.back();
//             v.pop_back();
//         }

//         if (!v.size())
//     root = i;
// else
//     rs[v.back()] = i;

// ls[i] = j;
// v.push_back(i);
//     }