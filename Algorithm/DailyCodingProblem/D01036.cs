namespace DailyCodingProblem;

/// <summary>
/// Given the sequence of keys visited by a postorder traversal of a binary search tree, reconstruct the tree.
/// For example, given the sequence 2, 4, 3, 8, 7, 5, you should construct the following tree:
///     5
///    / \
///   3   7
///  / \   \
/// 2   4   8
/// </summary>
public class D01036
{
    public static void Exection()
    {
        var nums = new int[] { 2, 4, 3, 8, 7, 5 };
        var n = nums[nums.Length - 1];
        var root = new Tree(n);
        var r = BuildTree(root, 0, nums.Length - 1, nums);
        Display(r);
    }

    private static void Display(Tree r)
    {
        if (r == null)
        {
            return;
        }
        Display(r.Left);
        Display(r.Right);
        Console.WriteLine(r.Value);
    }

    private static Tree BuildTree(Tree root, int start, int end, int[] nums)
    {
        if (start < 0 || start > end || end >= nums.Length)
        {
            return root;
        }
        var rootvalue = root.Value;
        var leftindex = -1;
        var rightindex = nums.Length + 1;
        for (int i = start; i <= end; i++)
        {
            if (nums[i] < rootvalue)
            {
                leftindex = i;
            }
            if (nums[i] > rootvalue)
            {
                rightindex = i;
            }
        }
        if (leftindex != -1)
        {
            var left = new Tree(nums[leftindex]);
            root.Left = BuildTree(left, start, leftindex - 1, nums);
        }
        if (rightindex != nums.Length + 1)
        {
            var right = new Tree(nums[rightindex]);
            root.Right = BuildTree(right, Math.Max(leftindex, start) + 1, end - 1, nums);
        }
        return root;
    }

    public class Tree
    {
        public Tree(int n)
        {
            Value = n;
        }
        public int Value { get; set; }
        public Tree Left { get; set; }
        public Tree Right { get; set; }
    }
}