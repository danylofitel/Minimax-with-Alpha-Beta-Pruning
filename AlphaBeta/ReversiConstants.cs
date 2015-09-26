﻿//-----------------------------------------------------------------------
// <copyright file="ReversiConstants.cs" author="Danylo Fitel">
// </copyright>
//-----------------------------------------------------------------------

using System.Collections.Generic;
using Position = System.Tuple<int, int>;

namespace AlphaBeta
{
    /// <summary>
    /// Possible game cell values.
    /// </summary>
    public enum ReversiValue
    {
        /// <summary>
        /// The empty cell.
        /// </summary>
        Empty,

        /// <summary>
        /// The maximizing player which also starts the game.
        /// </summary>
        Maximizing,

        /// <summary>
        /// The minimizing player.
        /// </summary>
        Minimizing
    }

    public static class ReversiConstants
    {
        /// <summary>
        /// Gets the length and width of the board. 8 is the classic version.
        /// </summary>
        public static int Size { get; } = 8;

        /// <summary>
        /// Gets the mobility points per cell.
        /// </summary>
        public static int MobilityPoints { get; } = 5;

        /// <summary>
        /// Gets the stability points per cell.
        /// </summary>
        public static int StabilityPoints { get; } = 25;

        /// <summary>
        /// Gets the corner points per each corner cell.
        /// </summary>
        public static int CornerPoints { get; } = 50;

        /// <summary>
        /// Gets the victory points.
        /// 64 * (5 + 25) + 4 * 50 = 2,120 is significantly less than 1,000,000
        /// </summary>
        public static int VictoryPoints { get; } = 1000000;

        /// <summary>
        /// Gets the table of occupied cells at starting position in a form of bit map.
        /// </summary>
        public static ulong OccupiedCellsBitMap { get; } = 0x1818000000UL;

        /// <summary>
        /// Gets the table of cells values at starting position in a form of bit map,
        /// 1 stands for maximizing player's cell.
        /// </summary>
        public static ulong PlayersCellsBitMap { get; } = 0x810000000UL;

        /// <summary>
        /// Positions of all corner cells.
        /// </summary>
        public static readonly IReadOnlyList<Position> CornerCells = new List<Position>
        {
            new Position(0, 0),             new Position(0, Size - 1),
            new Position(Size - 1, 0),      new Position(Size - 1, Size - 1)
        }.AsReadOnly();

        /// <summary>
        /// 9 possible directions from any cell.
        /// </summary>
        public static readonly IReadOnlyList<Position> Directions = new List<Position>
        {
            new Position(-1, -1),    new Position(-1, 0),    new Position(-1, 1),
            new Position(0, -1),     new Position(0, 0),     new Position(0, 1),
            new Position(1, -1),     new Position(1, 0),     new Position(1, 1)
        }.AsReadOnly();
    }
}