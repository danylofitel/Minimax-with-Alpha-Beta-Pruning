//-----------------------------------------------------------------------
// <copyright file="TicTacToeTable.cs" author="Danylo Fitel">
// </copyright>
//-----------------------------------------------------------------------

namespace AlphaBeta
{
    /// <summary>
    /// Memory efficient representation of Tic-Tac-Toe game board.
    /// </summary>
    internal struct TicTacToeTable
    {
        /// <summary>
        /// The set of game cells.
        /// </summary>
        private uint table;

        /// <summary>
        /// Gets the board size. Classic version is 3x3.
        /// </summary>
        public static int Size { get; } = 3;

        /// <summary>
        /// The cell count.
        /// </summary>
        private static int CellCount { get; } = Size * Size;

        /// <summary>
        /// Gets the bit mask covering filled cells table.
        /// </summary>
        private static uint FilledCellsMask { get; } = 0x1FFU;

        /// <summary>
        /// Gets the value of specified cell.
        /// </summary>
        /// <param name="row">The row.</param>
        /// <param name="column">The column.</param>
        /// <returns>Cell value.</returns>
        public Value GetValue(int row, int column)
        {
            int index = row * Size + column;

            if (((table >> index) & 1U) == 1U)
            {
                return ((table >> index + CellCount) & 1U) == 1U
                    ? Value.Maximizing
                    : Value.Minimizing;
            }

            return Value.Empty;
        }

        /// <summary>
        /// Sets the given value at specified cell.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="row">The row.</param>
        /// <param name="column">The column.</param>
        public void SetValue(Value value, int row, int column)
        {
            int index = row * Size + column;

            uint freeCellsMask = 1U << index;
            uint valuesMask = 1U << (index + CellCount);

            if (value == Value.Empty)
            {
                table &= ~freeCellsMask;
                table &= ~valuesMask;
            }
            else
            {
                table |= freeCellsMask;
                if (value == Value.Maximizing)
                {
                    table |= valuesMask;
                }
                else
                {
                    table &= ~valuesMask;
                }
            }
        }

        /// <summary>
        /// Gets the number of filled cells.
        /// </summary>
        /// <returns>The number of filled cells.</returns>
        public int GetFilledCells()
        {
            return HammingWeightBitcount(table & FilledCellsMask);
        }

        /// <summary>
        /// Gets the score of player X.
        /// </summary>
        /// <returns>The score of player X.</returns>
        public int GetScoreX()
        {
            return HammingWeightBitcount(table >> CellCount);
        }

        /// <summary>
        /// Gets the score of player O.
        /// </summary>
        /// <returns>The score of player O.</returns>
        public int GetScoreO()
        {
            return GetFilledCells() - GetScoreX();
        }

        /// <summary>
        /// Counts the number of set bits in an unsigned int using Hamming's weight algorithm.
        /// </summary>
        /// <param name="map">The bitmap.</param>
        /// <returns>Number of set bits.</returns>
        private static int HammingWeightBitcount(uint map)
        {
            map = map - ((map >> 1) & 0x55555555);
            map = (map & 0x33333333) + ((map >> 2) & 0x33333333);
            return (int)(unchecked(((map + (map >> 4)) & 0x0F0F0F0F) * 0x01010101) >> 24);
        }
    }
}
