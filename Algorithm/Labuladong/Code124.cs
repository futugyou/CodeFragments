using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Labuladong
{
    public class Code124
    {
        private static int result = int.MinValue;
        public static void Exection()
        {
            var tree = BaseTree.DefaultTree;
            Maxpathsum(tree);
            Console.WriteLine(result);
        }

        private static int Maxpathsum(BaseTree tree)
        {
            if (tree == null)
            {
                return 0;
            }
            var left = Math.Max(0, Maxpathsum(tree.Left));
            var right = Math.Max(0, Maxpathsum(tree.Right));
            result = Math.Max(result, tree.Value + left + right);
            return Math.Max(left, right) + tree.Value;
        }
    }
}
