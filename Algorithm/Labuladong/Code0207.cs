using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Labuladong
{
    public class Code0207
    {
        public static void Exection()
        {
            int numCourses = 4;
            int[][] prerequisites = new int[numCourses][];
            prerequisites[0] = new int[2] { 1, 0 };
            prerequisites[1] = new int[2] { 2, 1 };
            prerequisites[2] = new int[2] { 3, 2 };
            prerequisites[3] = new int[2] { 3, 1 };
            var result = FindOrder(numCourses, prerequisites);
            if (result)
            {
                postorder.Reverse();
                Console.WriteLine(string.Join(" ", postorder));
            }
        }

        private static bool FindOrder(int numCourses, int[][] prerequisites)
        {
            visited = new bool[numCourses];
            onPath = new bool[numCourses];
            var graph = BuildGraph(prerequisites);
            for (int i = 0; i < numCourses; i++)
            {
                Traverse(graph, i);
                if (hasCycle)
                {
                    return !hasCycle;
                }
            }
            return !hasCycle;
        }

        private static bool[] visited;
        //onpath record the 'node' foreach 'path'
        private static bool[] onPath;
        private static bool hasCycle;

        //拓扑排序 Topological sort 
        private static List<int> postorder = new List<int>();
        private static void Traverse(Dictionary<int, List<int>> dic, int key)
        {
            // when it is true here, that means it has a cycle. e.g  1-2-3-4-1, that will true.
            if (onPath[key])
            {
                hasCycle = true;
            }
            if (visited[key] || hasCycle)
            {
                return;
            }
            visited[key] = true;
            onPath[key] = true;
            if (dic.ContainsKey(key))
            {
                foreach (var item in dic[key])
                {
                    Traverse(dic, item);
                }
            }
            postorder.Add(key);
            onPath[key] = false;
        }

        private static Dictionary<int, List<int>> BuildGraph(int[][] prerequisites)
        {
            var dic = new Dictionary<int, List<int>>();
            foreach (var item in prerequisites)
            {
                var from = item[1];
                var to = item[0];
                if (dic.ContainsKey(from))
                {
                    dic[from].Add(to);
                }
                else
                {
                    var t = new List<int> { to };
                    dic.Add(from, t);
                }
            }
            return dic;
        }
    }
}
