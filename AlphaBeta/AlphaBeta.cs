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
    public class AlphaBeta
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

        public INode Best(INode root, bool maximizing)
        {
            if (root == null)
            {
                throw new ArgumentNullException(nameof(root), "Null node.");
            }

            INode bestNode = null;
            double bestValue = double.NegativeInfinity;

            List<Task<Tuple<double, INode>>> tasks = new List<Task<Tuple<double, INode>>>();
            foreach (INode child in root.Children)
            {
                tasks.Add(Task.Run(() => new Tuple<double, INode>(Search(
                    child,
                    searchDepth - 1,
                    double.NegativeInfinity,
                    double.PositiveInfinity,
                    !maximizing), child)));
            }

            Task.WaitAll(tasks.ToArray());

            foreach (var result in tasks)
            {
                if ((maximizing ? 1 : -1) * result.Result.Item1 > bestValue)
                {
                    bestNode = result.Result.Item2;
                    bestValue = result.Result.Item1;
                }
            }

            return bestNode;
        }

        private double Search(
            INode node,
            uint depth,
            double alpha,
            double beta,
            bool maximizing)
        {
            double currentValue = node.Heuristics;

            if (depth == 0 || !node.Children.Any())
            {
                return currentValue;
            }

            if (maximizing)
            {
                double value = double.NegativeInfinity;

                foreach (INode child in node.Children)
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

                foreach (INode child in node.Children)
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
