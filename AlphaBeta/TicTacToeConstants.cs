//-----------------------------------------------------------------------
// <copyright file="TicTacToeConstants.cs" author="Danylo Fitel">
// All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace AlphaBeta
{
    using System.Collections.Generic;
    using Position = System.Tuple<int, int>;

    /// <summary>
    /// Tic-Tac-Toe constants.
    /// </summary>
    internal static class TicTacToeConstants
    {
        /// <summary>
        /// Gets the potential winning line points.
        /// </summary>
        public const int PotentialWinningLinePoints = 1;

        /// <summary>
        /// Gets the points for victory.
        /// </summary>
        public const int VictoryPoints = 1000;

        /// <summary>
        /// All Tic-Tac-Toe game lines.
        /// </summary>
        public static readonly IReadOnlyList<IReadOnlyList<Position>> Lines =
            new List<IReadOnlyList<Position>>
            {
                new List<Position> { new Position(0, 0), new Position(0, 1), new Position(0, 2) }.AsReadOnly(),
                new List<Position> { new Position(1, 0), new Position(1, 1), new Position(1, 2) }.AsReadOnly(),
                new List<Position> { new Position(2, 0), new Position(2, 1), new Position(2, 2) }.AsReadOnly(),
                new List<Position> { new Position(0, 0), new Position(1, 0), new Position(2, 0) }.AsReadOnly(),
                new List<Position> { new Position(0, 1), new Position(1, 1), new Position(2, 1) }.AsReadOnly(),
                new List<Position> { new Position(0, 2), new Position(1, 2), new Position(2, 2) }.AsReadOnly(),
                new List<Position> { new Position(0, 0), new Position(1, 1), new Position(2, 2) }.AsReadOnly(),
                new List<Position> { new Position(2, 0), new Position(1, 1), new Position(0, 2) }.AsReadOnly(),
            }.AsReadOnly();
    }
}
