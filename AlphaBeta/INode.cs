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
    /// </summary>
    public interface INode : IComparable<INode>
    {
        /// <summary>
        /// Gets the list of children nodes.
        /// </summary>
        IReadOnlyList<INode> Children { get; }

        /// <summary>
        /// Gets the heuristics value of current position.
        /// </summary>
        int Heuristics { get; }
    }
}
