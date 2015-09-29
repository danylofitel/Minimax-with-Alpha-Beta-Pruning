//-----------------------------------------------------------------------
// <copyright file="ReversiNode.cs" author="Danylo Fitel">
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Position = System.Tuple<int, int>;

namespace AlphaBeta
{
    /// <summary>
    /// An immutable representation of Reversi game position.
    /// </summary>
    public class ReversiNode : INode
    {
        /// <summary>
        /// Indicates whether the current player can pass a move.
        /// </summary>
        private readonly bool canPass;

        /// <summary>
        /// The game state.
        /// </summary>
        private readonly ReversiTable stateTable;

        /// <summary>
        /// The number of child nodes used by heuristics.
        /// Initialized during the first computation of children
        /// to avoid repetition when computing heuristics.
        /// </summary>
        private int? childrenCount = null;

        /// <summary>
        /// The game state heuristics.
        /// </summary>
        private readonly Lazy<int> heuristics;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReversiNode" /> class with the starting position.
        /// </summary>
        public ReversiNode() : this(
            new ReversiTable(ReversiConstants.OccupiedCellsBitMap, ReversiConstants.PlayersCellsBitMap, 0UL),
            ReversiValue.Maximizing)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReversiNode" /> class.
        /// </summary>
        /// <param name="table">The game state.</param>
        /// <param name="stabilityTable">The flags of stable cells.</param>
        /// <param name="currentPlayer">Current player.</param>
        /// <param name="canPass">Indicates whether the current player can pass a move.</param>
        private ReversiNode(
            ReversiTable table,
            ReversiValue currentPlayer,
            bool playerCanPass = true)
        {
            canPass = playerCanPass;
            stateTable = table;

            Debug.Assert(currentPlayer != ReversiValue.Empty);

            Player = currentPlayer;
            Opponent = currentPlayer == ReversiValue.Maximizing
                ? ReversiValue.Minimizing
                : ReversiValue.Maximizing;

            // Will be initialized during first computation of child nodes.
            childrenCount = null;
            heuristics = new Lazy<int>(() => GetHeuristics());
        }

        /// <summary>
        /// Gets the current player.
        /// </summary>
        public ReversiValue Player { get; private set; }

        /// <summary>
        /// Gets the current player's opponent.
        /// </summary>
        public ReversiValue Opponent { get; private set; }

        /// <summary>
        /// Children nodes.
        /// </summary>
        public IReadOnlyList<INode> Children
        {
            get
            {
                // The children nodes are not saved after computation
                // because of significant memory usage since unit the end
                // of alpha-beta search all nodes in the tree are referenced
                // and can't be garbage collected.
                return GetChildren();
            }
        }

        /// <summary>
        /// Heuristics of current position.
        /// </summary>
        public int Heuristics
        {
            get
            {
                return heuristics.Value;
            }
        }

        /// <summary>
        /// Gets the cell value.
        /// </summary>
        /// <param name="row">The row.</param>
        /// <param name="column">The column.</param>
        /// <returns>The cell value.</returns>
        /// <exception cref="ArgumentOutOfRangeException">row, column;Row or column index out of range.</exception>
        public ReversiValue GetValue(int row, int column)
        {
            if (row < 0 || row >= ReversiConstants.Size ||
                column < 0 || column >= ReversiConstants.Size)
            {
                throw new ArgumentOutOfRangeException("row, column", "Row or column index out of range.");
            }

            return stateTable.GetValue(row, column);
        }

        /// <summary>
        /// Compares to another Reversi node.
        /// </summary>
        /// <param name="other">The other node to compare with.</param>
        /// <returns>Comparison result.</returns>
        public int CompareTo(INode other)
        {
            return Heuristics.CompareTo(other.Heuristics);
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance. Temporarily added for debugging purposes.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder("   ");
            for (int i = 0; i < ReversiConstants.Size; ++i)
            {
                sb.Append($"{i} ");
            }

            sb.AppendLine();
            for (int i = 0; i < ReversiConstants.Size; ++i)
            {
                sb.Append($"|{i}|");
                for (int j = 0; j < ReversiConstants.Size; ++j)
                {
                    ReversiValue value = stateTable.GetValue(i, j);
                    if (value == ReversiValue.Empty)
                    {
                        sb.Append('_');
                    }
                    else if (value == ReversiValue.Maximizing)
                    {
                        sb.Append('B');
                    }
                    else
                    {
                        sb.Append('W');
                    }

                    if (j < ReversiConstants.Size - 1)
                    {
                        sb.Append(" ");
                    }
                }

                sb.Append('|');
                sb.AppendLine();
            }

            sb.AppendLine();
            for (int i = 0; i < ReversiConstants.Size; ++i)
            {
                sb.Append($"|{i}|");
                for (int j = 0; j < ReversiConstants.Size; ++j)
                {
                    bool stable = stateTable.GetStable(i, j);
                    if (stable)
                    {
                        sb.Append('S');
                    }
                    else
                    {
                        sb.Append(' ');
                    }

                    if (j < ReversiConstants.Size - 1)
                    {
                        sb.Append(" ");
                    }
                }

                sb.Append('|');
                sb.AppendLine();
            }

            return sb.ToString();
        }

        /// <summary>
        /// Initializes the number of children used by heuristics.
        /// </summary>
        private void InitializeChildren()
        {
            if (!childrenCount.HasValue)
            {
                GetChildren();
            }
        }

        /// <summary>
        /// Computes all children nodes of current node.
        /// </summary>
        /// <returns>Children nodes.</returns>
        private IReadOnlyList<ReversiNode> GetChildren()
        {
            // If it has been computed previously that the node has no children,
            // return empty list.
            if (childrenCount.HasValue && childrenCount.Value == 0)
            {
                return new List<ReversiNode>().AsReadOnly();
            }

            var children = GetValidMoves().Select(move =>
            {
                ReversiTable table = GetTableForMove(move);
                ReversiTable stabilityTable = UpdateStability(table);

                return new ReversiNode(stabilityTable, Opponent);
            }).ToList();

            // Check whether it is a terminal node or whether the current player passes.
            if (children.Count == 0 && canPass)
            {
                ReversiNode nextNode = new ReversiNode(stateTable, Opponent, false);

                if (nextNode.Children.Count > 0)
                {
                    children.Add(nextNode);
                }
            }

            // Initialize the children counter if the children have not been computed yet.
            if (!childrenCount.HasValue)
            {
                lock (this)
                {
                    if (!childrenCount.HasValue)
                    {
                        childrenCount = children.Count;
                    }
                }
            }

            return children.AsReadOnly();
        }

        /// <summary>
        /// Returns the table after given move.
        /// </summary>
        /// <param name="move">The move.</param>
        /// <returns>The table after given move.</returns>
        private ReversiTable GetTableForMove(Tuple<Position, IEnumerable<Position>> move)
        {
            Position cell = move.Item1;

            ReversiTable tableAfterMove = stateTable;
            tableAfterMove.SetValue(Player, cell.Item1, cell.Item2);

            foreach (Position direction in move.Item2)
            {
                int dx = cell.Item1 + direction.Item1;
                int dy = cell.Item2 + direction.Item2;

                while (
                    dx >= 0 && dx < ReversiConstants.Size &&
                    dy >= 0 && dy < ReversiConstants.Size &&
                    stateTable.GetValue(dx, dy) == Opponent)
                {
                    tableAfterMove.SetValue(Player, dx, dy);
                    dx += direction.Item1;
                    dy += direction.Item2;
                }
            }

            return tableAfterMove;
        }

        /// <summary>
        /// Returns the table with updated stability values after given move.
        /// </summary>
        /// <param name="tableAfterMove">Table after the move with original stability state.</param>
        /// <param name="move">The move.</param>
        /// <returns>The table after given move with updated stability values.</returns>
        private ReversiTable UpdateStability(ReversiTable tableAfterMove)
        {
            // Start with all currently unstable cells.
            Queue<Position> unstableCells = new Queue<Position>(GetUnstableCells(tableAfterMove).ToList());

            // Process all unstable cells.
            while (unstableCells.Count > 0)
            {
                Position candidate = unstableCells.Dequeue();

                if (!tableAfterMove.GetStable(candidate.Item1, candidate.Item2) &&
                    IsStable(candidate, tableAfterMove))
                {
                    // Mark the cell as stable.
                    tableAfterMove.SetStable(true, candidate.Item1, candidate.Item2);

                    // All unstable neighbors need to be checked again.
                    foreach (Position delta in ReversiConstants.Directions)
                    {
                        Position neighbor = new Position(candidate.Item1 + delta.Item1, candidate.Item2 + delta.Item2);
                        if (neighbor.Item1 >= 0 && neighbor.Item1 < ReversiConstants.Size &&
                            neighbor.Item2 >= 0 && neighbor.Item2 < ReversiConstants.Size &&
                            tableAfterMove.GetValue(neighbor.Item1, neighbor.Item2) != ReversiValue.Empty &&
                            !tableAfterMove.GetStable(neighbor.Item1, neighbor.Item2))
                        {
                            unstableCells.Enqueue(neighbor);
                        }
                    }
                }
            }

            return tableAfterMove;
        }

        /// <summary>
        /// Gets the list of currently unstable occupied cells that are potentially stable.
        /// </summary>
        /// <param name="table">The table.</param>
        /// <returns>The list of unstable cells.</returns>
        private IEnumerable<Position> GetUnstableCells(ReversiTable table)
        {
            for (int i = 0; i < ReversiConstants.Size; ++i)
            {
                for (int j = 0; j < ReversiConstants.Size; ++j)
                {
                    if (table.GetValue(i, j) != ReversiValue.Empty && !table.GetStable(i, j))
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
        private bool IsStable(Position cell, ReversiTable table)
        {
            if (table.GetValue(cell.Item1, cell.Item2) == ReversiValue.Empty)
            {
                return false;
            }

            foreach (Position delta in ReversiConstants.StabilityDirections)
            {
                ReversiValue player = table.GetValue(cell.Item1, cell.Item2);
                Position positive = new Position(cell.Item1 + delta.Item1, cell.Item2 + delta.Item2);
                Position negative = new Position(cell.Item1 - delta.Item1, cell.Item2 - delta.Item2);

                // If one of the cell is on the edge, the direction is stable.
                if (positive.Item1 < 0 || positive.Item1 >= ReversiConstants.Size ||
                    positive.Item2 < 0 || positive.Item2 >= ReversiConstants.Size ||
                    negative.Item1 < 0 || negative.Item1 >= ReversiConstants.Size ||
                    negative.Item2 < 0 || negative.Item2 >= ReversiConstants.Size)
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
                    if (table.GetValue(positive.Item1, positive.Item2) == ReversiValue.Empty)
                    {
                        return false;
                    }

                    positive = new Position(positive.Item1 + delta.Item1, positive.Item2 + delta.Item2);
                }
                while (positive.Item1 >= 0 && positive.Item1 < ReversiConstants.Size &&
                    positive.Item2 >= 0 && positive.Item2 < ReversiConstants.Size);

                do
                {
                    if (table.GetValue(negative.Item1, negative.Item2) == ReversiValue.Empty)
                    {
                        return false;
                    }

                    negative = new Position(negative.Item1 - delta.Item1, negative.Item2 - delta.Item2);
                }
                while (negative.Item1 >= 0 && negative.Item1 < ReversiConstants.Size &&
                    negative.Item2 >= 0 && negative.Item2 < ReversiConstants.Size);

                // The line is filled.
                return true;
            }

            // No unstable directions found.
            return true;
        }

        /// <summary>
        /// Candidates the cells.
        /// </summary>
        /// <returns>List of valid moves.</returns>
        private IEnumerable<Tuple<Position, IEnumerable<Position>>> GetValidMoves()
        {
            foreach (Position cell in GetFreeCells())
            {
                IList<Position> capturedDirections = GetDirectionsCapturedByMove(cell).ToList();
                if (capturedDirections.Count > 0)
                {
                    yield return new Tuple<Position, IEnumerable<Position>>(cell, capturedDirections);
                }
            }
        }

        /// <summary>
        /// Finds all empty cells.
        /// </summary>
        /// <returns>The list of empty cells.</returns>
        private IEnumerable<Position> GetFreeCells()
        {
            for (int i = 0; i < ReversiConstants.Size; ++i)
            {
                for (int j = 0; j < ReversiConstants.Size; ++j)
                {
                    if (stateTable.GetValue(i, j) == ReversiValue.Empty)
                    {
                        yield return new Position(i, j);
                    }
                }
            }
        }

        /// <summary>
        /// Gets directions in which the specified move flips opponent's cells.
        /// </summary>
        /// <param name="move">The move represented by cell coordinates.</param>
        /// <returns>Directions in which the specified move flips opponent's cells.</returns>
        private IEnumerable<Position> GetDirectionsCapturedByMove(Position move)
        {
            return ReversiConstants.Directions.Where(direction => MoveCapturesDirection(move, direction));
        }

        /// <summary>
        /// Determines whether the specified move flips opponent's cells in given direction.
        /// </summary>
        /// <param name="move">The move represented by cell coordinates.</param>
        /// <param name="direction">The direction represented by coordinate deltas.</param>
        /// <returns>True if the move flips opponent's cells in given direction, false otherwise.</returns>
        private bool MoveCapturesDirection(Position move, Position direction)
        {
            int dx = move.Item1 + direction.Item1;
            int dy = move.Item2 + direction.Item2;

            bool opponentCellsCaptured = false;

            while (dx >= 0 && dx < ReversiConstants.Size && dy >= 0 && dy < ReversiConstants.Size)
            {
                ReversiValue value = stateTable.GetValue(dx, dy);

                if (!opponentCellsCaptured)
                {
                    if (value == Opponent)
                    {
                        opponentCellsCaptured = true;
                        continue;
                    }
                    else
                    {
                        return false;
                    }
                }

                if (value == ReversiValue.Empty)
                {
                    return false;
                }
                if (value == Player)
                {
                    return true;
                }

                dx += direction.Item1;
                dy += direction.Item2;
            }

            return false;
        }

        /// <summary>
        /// Computes the heuristics of current position.
        /// </summary>
        /// <returns>Heuristic value of current position.</returns>
        private int GetHeuristics()
        {
            InitializeChildren();

            if (childrenCount == 0)
            {
                return GetScore();
            }

            return GetMobilityScore() + GetStabilityScore();
        }

        /// <summary>
        /// Computes the final game score.
        /// </summary>
        /// <returns>Final score.</returns>
        private int GetScore()
        {
            int occupiedCells = stateTable.OccupiedCells();
            int maxPlayerCells = stateTable.MaximizingPlayerCells();
            int minPlayerCells = occupiedCells - maxPlayerCells;

            return (maxPlayerCells - minPlayerCells) * ReversiConstants.VictoryPoints;
        }

        /// <summary>
        /// Computes the mobility heuristics.
        /// </summary>
        /// <returns>Mobility heuristics.</returns>
        private int GetMobilityScore()
        {
            return childrenCount.Value
                * (Player == ReversiValue.Maximizing ? 1 : -1)
                * ReversiConstants.MobilityPoints;
        }

        /// <summary>
        /// Computes the stability heuristics.
        /// </summary>
        /// <returns>Stability heuristics.</returns>
        private int GetStabilityScore()
        {
            int score = 0;

            for (int i = 0; i < ReversiConstants.Size; ++i)
            {
                for (int j = 0; j < ReversiConstants.Size; ++j)
                {
                    if (stateTable.GetStable(i, j))
                    {
                        score += stateTable.GetValue(i, j) == ReversiValue.Maximizing ? 1 : -1;
                    }
                }
            }

            return score * ReversiConstants.StabilityPoints;
        }
    }
}
