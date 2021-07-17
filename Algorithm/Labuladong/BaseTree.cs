using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Labuladong
{
    public class BaseTree
    {
        public static BaseTree DefaultTree = new BaseTree(1)
        {
            Left = new BaseTree(2)
            {
                Left = new BaseTree(4),
                Right = new BaseTree(5)
            },
            Right = new BaseTree(3)
            {
                Left = new BaseTree(6),
                Right = new BaseTree(7)
            }
        };
        public int Value { get; set; }
        public BaseTree(int n)
        {
            Value = n;
        }
        public BaseTree Left { get; set; }
        public BaseTree Right { get; set; }
    }
}
