//-----------------------------------------------------------------------
// <copyright file="ReversiHeuristics.cs" author="Danylo Fitel">
// </copyright>
//-----------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Position = System.Tuple<int, int>;

namespace AlphaBeta
{
    internal static class ReversiHeuristics
    {
        /// <summary>
        /// Returns the table with updated stability values after given move.
        /// </summary>
        /// <param name="table">Table after the move with original stability state.</param>
        /// <param name="move">The move.</param>
        /// <returns>The table after given move with updated stability values.</returns>
        public static ReversiTable GetTableWithUpdatedStability(this ReversiTable table)
        {
            // Start with all currently unstable cells.
            Queue<Position> unstableCells = new Queue<Position>(table.GetUnstableCells().ToList());

            // Process all unstable cells.
            while (unstableCells.Count > 0)
            {
                Position candidate = unstableCells.Dequeue();

                if (!table.GetStable(candidate.Item1, candidate.Item2) &&
                    table.IsStable(candidate))
                {
                    // Mark the cell as stable.
                    table.SetStable(true, candidate.Item1, candidate.Item2);

                    // All unstable neighbors need to be checked again.
                    foreach (Position delta in ReversiConstants.Directions)
                    {
                        Position neighbor = new Position(candidate.Item1 + delta.Item1, candidate.Item2 + delta.Item2);
                        if (neighbor.Item1 >= 0 && neighbor.Item1 < ReversiTable.Size &&
                            neighbor.Item2 >= 0 && neighbor.Item2 < ReversiTable.Size &&
                            table.GetValue(neighbor.Item1, neighbor.Item2) != Value.None &&
                            !table.GetStable(neighbor.Item1, neighbor.Item2))
                        {
                            unstableCells.Enqueue(neighbor);
                        }
                    }
                }
            }

            return table;
        }

        /// <summary>
        /// Gets the list of currently unstable occupied cells that are potentially stable.
        /// </summary>
        /// <param name="table">The table.</param>
        /// <returns>The list of unstable cells.</returns>
        public static IEnumerable<Position> GetUnstableCells(this ReversiTable table)
        {
            for (int i = 0; i < ReversiTable.Size; ++i)
            {
                for (int j = 0; j < ReversiTable.Size; ++j)
                {
                    if (table.GetValue(i, j) != Value.None && !table.GetStable(i, j))
                    {
                        yield return new Position(i, j);
                    }
                }
            }
        }

        /// <summary>
        /// Determines whether the specified cell is stable.
        /// </summary>
        /// <param name="cell">The cell.</param>
        /// <param name="table">The table.</param>
        /// <returns>True if the cell is stable, false otherwise.</returns>
        public static bool IsStable(this ReversiTable table, Position cell)
        {
            if (table.GetValue(cell.Item1, cell.Item2) == Value.None)
            {
                return false;
            }

            foreach (Position delta in ReversiConstants.StabilityDirections)
            {
                Value player = table.GetValue(cell.Item1, cell.Item2);
                Position positive = new Position(cell.Item1 + delta.Item1, cell.Item2 + delta.Item2);
                Position negative = new Position(cell.Item1 - delta.Item1, cell.Item2 - delta.Item2);

                // If one of the cell is on the edge, the direction is stable.
                if (positive.Item1 < 0 || positive.Item1 >= ReversiTable.Size ||
                    positive.Item2 < 0 || positive.Item2 >= ReversiTable.Size ||
                    negative.Item1 < 0 || negative.Item1 >= ReversiTable.Size ||
                    negative.Item2 < 0 || negative.Item2 >= ReversiTable.Size)
                {
                    continue;
                }

                // If at least one of the neighbors is a stable cell belonging to the same player,
                // the direction is stable.
                if ((table.GetStable(positive.Item1, positive.Item2) &&
                        table.GetValue(positive.Item1, positive.Item2) == player) ||
                    (table.GetStable(negative.Item1, negative.Item2) &&
                        table.GetValue(negative.Item1, negative.Item2) == player))
                {
                    continue;
                }

                // If all cells in the line of direction are filled, the direction is stable.
                do
                {
                    if (table.GetValue(positive.Item1, positive.Item2) == Value.None)
                    {
                        return false;
                    }

                    positive = new Position(positive.Item1 + delta.Item1, positive.Item2 + delta.Item2);
                }
                while (positive.Item1 >= 0 && positive.Item1 < ReversiTable.Size &&
                    positive.Item2 >= 0 && positive.Item2 < ReversiTable.Size);

                do
                {
                    if (table.GetValue(negative.Item1, negative.Item2) == Value.None)
                    {
                        return false;
                    }

                    negative = new Position(negative.Item1 - delta.Item1, negative.Item2 - delta.Item2);
                }
                while (negative.Item1 >= 0 && negative.Item1 < ReversiTable.Size &&
                    negative.Item2 >= 0 && negative.Item2 < ReversiTable.Size);

                // The line is filled.
                continue;
            }

            // No unstable directions found.
            return true;
        }

        /// <summary>
        /// Gets the stability score.
        /// </summary>
        /// <param name="table">The table.</param>
        /// <returns>The difference between maximizing and minimizing players' stable cells.</returns>
        public static int GetStabilityScore(this ReversiTable table)
        {
            int score = 0;

            for (int i = 0; i < ReversiTable.Size; ++i)
            {
                for (int j = 0; j < ReversiTable.Size; ++j)
                {
                    if (table.GetStable(i, j))
                    {
                        score += table.GetValue(i, j) == Value.Maximizing ? 1 : -1;
                    }
                }
            }

            return score;
        }
    }
}
