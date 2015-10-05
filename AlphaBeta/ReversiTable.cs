//-----------------------------------------------------------------------
// <copyright file="ReversiTable.cs" author="Danylo Fitel">
// All rights reserved.
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
        /// Gets the length and width of the board. 8 is the classic version.
        /// </summary>
        public const int Size = 8;

        /// <summary>
        /// Gets the table of occupied cells at starting position in a form of bit map.
        /// </summary>
        private const ulong OccupiedCellsBitMap = 0x1818000000UL;

        /// <summary>
        /// Gets the table of cells values at starting position in a form of bit map,
        /// 1 stands for maximizing player's cell.
        /// </summary>
        private const ulong PlayersCellsBitMap = 0x810000000UL;

        /// <summary>
        /// Gets the table of cell stability values at starting position in a form of bit map.
        /// </summary>
        private const ulong StabilityCellsBitMap = 0x0UL;

        /// <summary>
        /// The flags indicating which cells are used (0 is empty, 1 is used).
        /// </summary>
        private ulong occupiedCellsTable;

        /// <summary>
        /// The flags indicating which player's disks are at game cells (1 is maximizing player's cell).
        /// </summary>
        private ulong playersTable;

        /// <summary>
        /// Flags indicating which cells are stable.
        /// </summary>
        private ulong stabilityTable;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReversiTable"/> struct.
        /// </summary>
        /// <returns>Reversi table at the beginning of the game.</returns>
        public static ReversiTable InitialState()
        {
            ReversiTable table = new ReversiTable();

            table.occupiedCellsTable = OccupiedCellsBitMap;
            table.playersTable = PlayersCellsBitMap;
            table.stabilityTable = StabilityCellsBitMap;

            return table;
        }

        /// <summary>
        /// Gets the value by coordinates.
        /// </summary>
        /// <param name="row">The row.</param>
        /// <param name="column">The column.</param>
        /// <returns>Cell value.</returns>
        public Value GetValue(int row, int column)
        {
            int index = row * Size + column;

            if (((occupiedCellsTable >> index) & 1UL) == 1UL)
            {
                return ((playersTable >> index) & 1UL) == 1UL
                    ? Value.Maximizing
                    : Value.Minimizing;
            }

            return Value.None;
        }

        /// <summary>
        /// Sets the value by coordinates.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="row">The row.</param>
        /// <param name="column">The column.</param>
        public void SetValue(Value value, int row, int column)
        {
            ulong mask = 1UL << (row * Size + column);

            if (value == Value.None)
            {
                occupiedCellsTable &= ~mask;
                playersTable &= ~mask;
            }
            else
            {
                occupiedCellsTable |= mask;
                if (value == Value.Maximizing)
                {
                    playersTable |= mask;
                }
                else
                {
                    playersTable &= ~mask;
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
            int index = row * Size + column;
            return ((stabilityTable >> index) & 1UL) == 1UL;
        }

        /// <summary>
        /// Sets the stability value by coordinates.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="row">The row.</param>
        /// <param name="column">The column.</param>
        public void SetStable(bool value, int row, int column)
        {
            ulong mask = 1UL << (row * Size + column);

            if (value)
            {
                stabilityTable |= mask;
            }
            else
            {
                stabilityTable &= ~mask;
            }
        }

        /// <summary>
        /// Computes the number of occupied cells.
        /// </summary>
        /// <returns>The number of occupied cells.</returns>
        public int OccupiedCells()
        {
            return HammingWeightBitcount(occupiedCellsTable);
        }

        /// <summary>
        /// Computes the number of cells occupied by maximizing player.
        /// </summary>
        /// <returns>The number of cells occupied by maximizing player.</returns>
        public int MaximizingPlayerCells()
        {
            return HammingWeightBitcount(playersTable);
        }

        /// <summary>
        /// Computes the number of cells occupied by minimizing player.
        /// </summary>
        /// <returns>The number of cells occupied by minimizing player.</returns>
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
