using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DailyCodingProblem
{
    public class D00922
    {
        public static void Exection()
        {

        }

        /// <summary>
        /// Implement locking in a binary tree. A binary tree node can be locked or unlocked only if all of its descendants or ancestors are not locked.
        ///   Design a binary tree node class with the following methods:
        ///       is_locked, which returns whether the node is locked
        ///       lock, which attempts to lock the node.If it cannot be locked, then it should return false. Otherwise, it should lock it and return true.
        ///       unlock, which unlocks the node. If it cannot be unlocked, then it should return false. Otherwise, it should unlock it and return true.
        ///   You may augment the node to add parent pointers or any other property you would like. 
        ///   You may assume the class is used in a single-threaded program, 
        ///   so there is no need for actual locks or mutexes.Each method should run in ""O(h)"", where h is the height of the tree.
        /// </summary>
        private class BinaryTree
        {
            public BinaryTree Parent { get; set; }
            public BinaryTree Left { get; set; }
            public BinaryTree Right { get; set; }
            public int Value { get; set; }

            private bool _islock;
            public bool IsLock()
            {
                return _islock;
            }

            public bool Lock()
            {
                var parent = Parent;
                while (parent != null)
                {
                    if (parent.IsLock())
                    {
                        return false;
                    }
                    parent = parent.Parent;
                }
                var sub = GetSubLockNode(this);
                if (sub != null)
                {
                    return false;
                }
                _islock = true;
                return true;
            }

            private BinaryTree GetSubLockNode(BinaryTree binaryTree)
            {
                if (binaryTree == null)
                {
                    return null;
                }
                if (binaryTree.IsLock())
                {
                    return binaryTree;
                }
                return GetSubLockNode(binaryTree.Left) ?? GetSubLockNode(binaryTree.Right);
            }

            public bool Unlock()
            {
                var parent = Parent;
                while (parent != null)
                {
                    if (parent.IsLock())
                    {
                        return false;
                    }
                    parent = parent.Parent;
                }
                var sub = GetSubLockNode(this);
                if (sub != null)
                {
                    return false;
                }
                _islock = false;
                return true;
            }
        }
    }
}
