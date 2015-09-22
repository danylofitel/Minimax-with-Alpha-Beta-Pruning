//-----------------------------------------------------------------------
// <copyright file="AlphaBeta.cs" author="Danylo Fitel">
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Linq;

namespace AlphaBeta
{
    public class AlphaBeta
    {
        private readonly INode root;
        private readonly uint searchDepth;

        public AlphaBeta(INode initialState, uint depth)
        {
            if (initialState == null)
            {
                throw new ArgumentNullException(nameof(initialState), "Null initial state.");
            }

            if (depth == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(depth), depth, "Zero depth.");
            }

            root = initialState;
            searchDepth = depth;
        }

        public INode Best(bool maximizing)
        {
            INode bestNode = null;
            double bestValue = double.NegativeInfinity;
            foreach (INode child in root.Children)
            {
                double value = Search(
                    child,
                    5,
                    double.NegativeInfinity,
                    double.PositiveInfinity,
                    !maximizing);

                if (child.Heuristics > bestValue)
                {
                    bestNode = child;
                    bestValue = child.Heuristics;
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
