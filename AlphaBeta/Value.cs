//-----------------------------------------------------------------------
// <copyright file="Value.cs" author="Danylo Fitel">
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
        /// The empty cell.
        /// </summary>
        Empty,

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
