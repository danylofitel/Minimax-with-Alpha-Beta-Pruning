//-----------------------------------------------------------------------
// <copyright file="INode.cs" author="Danylo Fitel">
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace AlphaBeta
{
    /// <summary>
    /// Interface for an immutable state of a zero-sum game for 2 players.
    /// The node implements IComparable to allow sorting decisions in the search tree.
    /// Default implementation of comparison would simply compare heuristic values.
    /// </summary>
    public interface INode : IComparable<INode>
    {
        /// <summary>
        /// Gets the current player.
        /// </summary>
        Value Player { get; }

        /// <summary>
        /// Gets the opponent of current player.
        /// </summary>
        Value Opponent { get; }

        /// <summary>
        /// Gets the list of children nodes.
        /// </summary>
        IReadOnlyList<INode> Children { get; }

        /// <summary>
        /// Gets the heuristics value of current position
        /// from maximizing player's perspective.
        /// </summary>
        int Heuristics { get; }
    }
}
