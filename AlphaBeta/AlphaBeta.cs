//-----------------------------------------------------------------------
// <copyright file="AlphaBeta.cs" author="Danylo Fitel">
// All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace AlphaBeta
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    /// <summary>
    /// Implementation of minimax search with alpha-beta pruning.
    /// </summary>
    /// <typeparam name="Node">Search node.</typeparam>
    public class AlphaBeta<Node> where Node : INode
    {
        /// <summary>
        /// The maximum search depth.
        /// </summary>
        private readonly uint searchDepth;

        /// <summary>
        /// Constructs the search class for specific depth.
        /// </summary>
        /// <param name="depth">Search depth.</param>
        public AlphaBeta(uint depth)
        {
            if (depth == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(depth), depth, "Zero depth.");
            }

            searchDepth = depth;
        }

        /// <summary>
        /// Performs minimax search and returns the best child node.
        /// </summary>
        /// <param name="root">Initial state.</param>
        /// <returns>The best child node.</returns>
        public async Task<Node> Best(Node root)
        {
            if (root == null)
            {
                throw new ArgumentNullException(nameof(root), "Null node.");
            }
            else if (root.Player == Value.None)
            {
                throw new ArgumentException(nameof(root.Player), "The player is not specified.");
            }

            bool maximizing = root.Player == Value.Maximizing;

            List<Task<Tuple<double, Node>>> tasks = new List<Task<Tuple<double, Node>>>();
            foreach (Node child in root.Children)
            {
                tasks.Add(Task.Run(() => new Tuple<double, Node>(
                    Search(
                        child,
                        searchDepth - 1,
                        double.NegativeInfinity,
                        double.PositiveInfinity,
                        !maximizing),
                    child)));
            }

            Tuple<double, Node>[] results = await Task.WhenAll(tasks);

            Node bestNode = default(Node);
            if (maximizing)
            {
                double bestValue = double.NegativeInfinity;

                foreach (var result in results)
                {
                    if (result.Item1 > bestValue)
                    {
                        bestNode = result.Item2;
                        bestValue = result.Item1;
                    }
                }
            }
            else
            {
                double bestValue = double.PositiveInfinity;

                foreach (var result in results)
                {
                    if (result.Item1 < bestValue)
                    {
                        bestNode = result.Item2;
                        bestValue = result.Item1;
                    }
                }
            }

            return bestNode;
        }

        /// <summary>
        /// Minimax search with alpha-beta pruning.
        /// </summary>
        /// <param name="node">Initial node.</param>
        /// <param name="depth">Search depth.</param>
        /// <param name="alpha">Alpha value.</param>
        /// <param name="beta">Beta value.</param>
        /// <param name="maximizing">Current player.</param>
        /// <returns>Heuristic value of current node for current player.</returns>
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
