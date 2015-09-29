//-----------------------------------------------------------------------
// <copyright file="ReversiTable.cs" author="Danylo Fitel">
// </copyright>
//-----------------------------------------------------------------------

namespace AlphaBeta
{
    /// <summary>
    /// Memory efficient representation of Reversi game board.
    /// </summary>
    internal struct ReversiTable
    {
        /// <summary>
        /// The flags indicating which cells are used (0 is empty, 1 is used).
        /// </summary>
        private ulong OccupiedCellsTable;

        /// <summary>
        /// The flags indicating which player's disks are at game cells (1 is maximizing player's cell).
        /// </summary>
        private ulong PlayersTable;

        /// <summary>
        /// Flags indicating which cells are stable.
        /// </summary>
        private ulong StabilityTable;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReversiTable"/> struct.
        /// </summary>
        /// <param name="occupiedCellsTable">The occupied cells table.</param>
        /// <param name="playersTable">The players table.</param>
        /// <param name="stabilityTable">The stability table.</param>
        public ReversiTable(ulong occupiedCellsTable, ulong playersTable, ulong stabilityTable)
        {
            OccupiedCellsTable = occupiedCellsTable;
            PlayersTable = playersTable;
            StabilityTable = stabilityTable;
        }

        /// <summary>
        /// Gets the value by coordinates.
        /// </summary>
        /// <param name="row">The row.</param>
        /// <param name="column">The column.</param>
        /// <returns>Cell value.</returns>
        public ReversiValue GetValue(int row, int column)
        {
            if (((OccupiedCellsTable >> row * ReversiConstants.Size + column) & 1UL) == 1UL)
            {
                return ((PlayersTable >> row * ReversiConstants.Size + column) & 1UL) == 1UL
                    ? ReversiValue.Maximizing
                    : ReversiValue.Minimizing;
            }

            return ReversiValue.Empty;
        }

        /// <summary>
        /// Sets the value by coordinates.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="row">The row.</param>
        /// <param name="column">The column.</param>
        public void SetValue(ReversiValue value, int row, int column)
        {
            ulong mask = 1UL << (row * ReversiConstants.Size + column);

            if (value == ReversiValue.Empty)
            {
                OccupiedCellsTable &= ~mask;
                PlayersTable &= ~mask;
            }
            else
            {
                OccupiedCellsTable |= mask;
                if (value == ReversiValue.Maximizing)
                {
                    PlayersTable |= mask;
                }
                else
                {
                    PlayersTable &= ~mask;
                }
            }
        }

        /// <summary>
        /// Gets the stability value by coordinates.
        /// </summary>
        /// <param name="row">The row.</param>
        /// <param name="column">The column.</param>
        /// <returns>Cell stability value.</returns>
        public bool GetStable(int row, int column)
        {
            return ((StabilityTable >> row * ReversiConstants.Size + column) & 1UL) == 1UL;
        }

        /// <summary>
        /// Sets the stability value by coordinates.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="row">The row.</param>
        /// <param name="column">The column.</param>
        public void SetStable(bool value, int row, int column)
        {
            ulong mask = 1UL << (row * ReversiConstants.Size + column);

            if (value)
            {
                StabilityTable |= mask;
            }
            else
            {
                StabilityTable &= ~mask;
            }
        }

        /// <summary>
        /// The number of occupied cells.
        /// </summary>
        /// <returns></returns>
        public int OccupiedCells()
        {
            return HammingWeightBitcount(OccupiedCellsTable);
        }

        /// <summary>
        /// The number of cells occupied by maximizing player.
        /// </summary>
        /// <returns></returns>
        public int MaximizingPlayerCells()
        {
            return HammingWeightBitcount(PlayersTable);
        }

        /// <summary>
        /// The number of cells occupied by minimizing player.
        /// </summary>
        /// <returns></returns>
        public int MinimizingPlayerCells()
        {
            return OccupiedCells() - MaximizingPlayerCells();
        }

        /// <summary>
        /// Counts the number of set bits in an unsigned long using Hamming's weight algorithm.
        /// </summary>
        /// <param name="map">The bitmap.</param>
        /// <returns>Number of set bits.</returns>
        private static int HammingWeightBitcount(ulong map)
        {
            map = map - ((map >> 1) & 0x5555555555555555UL);
            map = (map & 0x3333333333333333UL) + ((map >> 2) & 0x3333333333333333UL);
            return (int)(unchecked(((map + (map >> 4)) & 0xF0F0F0F0F0F0F0FUL) * 0x101010101010101UL) >> 56);
        }
    }
}
