using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Labuladong
{
    public class Dijkstra
    {
        private static List<List<int>> graph = new List<List<int>>();

        public static List<int> Adj(int s)
        {
            return graph[s];
        }

        public static int Weight(int from, int to)
        {
            // retrurn weight
            return 0;
        }

        public static int[] DoDijkstra(int start, List<int>[] graph)
        {
            // node count
            int v = graph.Length;
            // distTo[i] means the cost lessest route.
            var distTo = new int[v];
            Array.Fill(distTo, int.MaxValue);
            distTo[start] = 0;

            var pq = new PriorityQueue<State, int>();
            // begin BFS from start
            pq.Enqueue(new State(start, 0), 0);

            while (pq.Count > 0)
            {
                State curState = pq.Dequeue();
                int curNodeID = curState.id;
                int curDistFromStart = curState.distFromStart;

                if (curDistFromStart > distTo[curNodeID])
                {
                    // ready have short path to curNode
                    continue;
                }

                foreach (var nextNodeID in Adj(curNodeID))
                {
                    int distToNextNode = distTo[nextNodeID] + Weight(curNodeID, nextNodeID);
                    if (distTo[nextNodeID] > distToNextNode)
                    {
                        distTo[nextNodeID] = distToNextNode;
                        pq.Enqueue(new State(nextNodeID, distToNextNode), distToNextNode);
                    }
                }
            }
            return distTo;

        }
        public class State
        {
            // 图节点的 id
            public int id;
            // 从 start 节点到当前节点的距离
            public int distFromStart;

            public State(int id, int distFromStart)
            {
                this.id = id;
                this.distFromStart = distFromStart;
            }
        }
    }
}
