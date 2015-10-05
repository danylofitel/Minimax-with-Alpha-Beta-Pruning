//-----------------------------------------------------------------------
// <copyright file="Value.cs" author="Danylo Fitel">
// All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace AlphaBeta
{
    /// <summary>
    /// Possible game values.
    /// </summary>
    public enum Value
    {
        /// <summary>
        /// Not specified player or empty cell.
        /// </summary>
        None,

        /// <summary>
        /// The maximizing player which starts the game.
        /// </summary>
        Maximizing,

        /// <summary>
        /// The minimizing player.
        /// </summary>
        Minimizing
    }
}
