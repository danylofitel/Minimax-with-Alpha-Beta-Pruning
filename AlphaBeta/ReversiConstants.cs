//-----------------------------------------------------------------------
// <copyright file="ReversiConstants.cs" author="Danylo Fitel">
// All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace AlphaBeta
{
    using System.Collections.Generic;
    using Position = System.Tuple<int, int>;

    /// <summary>
    /// Constants for Reversi game.
    /// </summary>
    internal static class ReversiConstants
    {
        /// <summary>
        /// Gets the mobility points per cell.
        /// </summary>
        public const int MobilityPoints = 5;

        /// <summary>
        /// Gets the stability points per cell.
        /// </summary>
        public const int StabilityPoints = 100;

        /// <summary>
        /// Gets the victory points.
        /// 64 * (Mobility + Stability) is significantly less than Victory
        /// </summary>
        public const int VictoryPoints = 1000000;

        /// <summary>
        /// 8 possible directions from any cell.
        /// </summary>
        public static readonly IReadOnlyList<Position> Directions = new List<Position>
        {
            new Position(-1, -1),   new Position(-1, 0),    new Position(-1, 1),
            new Position(0, -1),                            new Position(0, 1),
            new Position(1, -1),    new Position(1, 0),     new Position(1, 1)
        }.AsReadOnly();

        /// <summary>
        /// 4 directions in which the cell must be limited at least from one side with a stable cell
        /// of the same color in order to be stable.
        /// </summary>
        public static readonly IReadOnlyList<Position> StabilityDirections = new List<Position>
        {
            // Horizontal.
            new Position(0, 1),

            // Vertical.
            new Position(1, 0),

            // Diagonal left-to-right.
            new Position(1, 1),

            // Diagonal right-to-left.
            new Position(1, -1)
        }.AsReadOnly();
    }
}
