//-----------------------------------------------------------------------
// <copyright file="AlphaBeta.cs" author="Danylo Fitel">
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AlphaBeta
{
    public class AlphaBeta<Node> where Node : INode
    {
        private readonly uint searchDepth;

        public AlphaBeta(uint depth)
        {
            if (depth == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(depth), depth, "Zero depth.");
            }

            searchDepth = depth;
        }

        public Node Best(Node root, bool maximizing)
        {
            if (root == null)
            {
                throw new ArgumentNullException(nameof(root), "Null node.");
            }

            List<Task<Tuple<double, Node>>> tasks = new List<Task<Tuple<double, Node>>>();
            foreach (Node child in root.Children)
            {
                tasks.Add(Task.Run(() => new Tuple<double, Node>(Search(
                    child,
                    searchDepth - 1,
                    double.NegativeInfinity,
                    double.PositiveInfinity,
                    !maximizing), child)));
            }

            Task.WaitAll(tasks.ToArray());

            if (maximizing)
            {
                Node bestNode = default(Node);
                double bestValue = double.NegativeInfinity;

                foreach (var result in tasks)
                {
                    if (result.Result.Item1 > bestValue)
                    {
                        bestNode = result.Result.Item2;
                        bestValue = result.Result.Item1;
                    }
                }

                return bestNode;
            }
            else
            {
                Node bestNode = default(Node);
                double bestValue = double.PositiveInfinity;

                foreach (var result in tasks)
                {
                    if (result.Result.Item1 < bestValue)
                    {
                        bestNode = result.Result.Item2;
                        bestValue = result.Result.Item1;
                    }
                }

                return bestNode;
            }
        }

        private double Search(
            Node node,
            uint depth,
            double alpha,
            double beta,
            bool maximizing)
        {
            if (depth == 0 || !node.Children.Any())
            {
                return node.Heuristics;
            }

            if (maximizing)
            {
                double value = double.NegativeInfinity;

                foreach (Node child in node.Children)
                {
                    value = Math.Max(value, Search(child, depth - 1, alpha, beta, false));
                    alpha = Math.Max(alpha, value);
                    if (beta <= alpha)
                    {
                        // Beta-cutoff
                        break;
                    }
                }

                return value;
            }
            else
            {
                double value = double.PositiveInfinity;

                foreach (Node child in node.Children)
                {
                    value = Math.Min(value, Search(child, depth - 1, alpha, beta, true));
                    beta = Math.Min(beta, value);
                    if (beta <= alpha)
                    {
                        // Alpha-cutoff
                        break;
                    }
                }

                return value;
            }
        }
    }
}
