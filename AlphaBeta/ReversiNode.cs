//-----------------------------------------------------------------------
// <copyright file="ReversiNode.cs" author="Danylo Fitel">
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
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
        /// The child nodes.
        /// </summary>
        private readonly Lazy<IReadOnlyList<ReversiNode>> children;

        /// <summary>
        /// The game state heuristics.
        /// </summary>
        private readonly Lazy<int> heuristics;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReversiNode" /> class with the starting position.
        /// </summary>
        public ReversiNode() : this(
            ReversiTable.InitialState(),
            Value.Maximizing)
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
            Value currentPlayer,
            bool playerCanPass = true)
        {
            canPass = playerCanPass;
            stateTable = table;

            heuristics = new Lazy<int>(() => GetHeuristics(),
                LazyThreadSafetyMode.ExecutionAndPublication);

            children = new Lazy<IReadOnlyList<ReversiNode>>(() => GetChildren(),
                LazyThreadSafetyMode.ExecutionAndPublication);

            Debug.Assert(currentPlayer != Value.None);
            Player = currentPlayer;
            Opponent = currentPlayer == Value.Maximizing
                ? Value.Minimizing
                : Value.Maximizing;
        }

        /// <summary>
        /// Gets the current player.
        /// </summary>
        public Value Player { get; private set; }

        /// <summary>
        /// Gets the current player's opponent.
        /// </summary>
        public Value Opponent { get; private set; }

        /// <summary>
        /// Children nodes.
        /// </summary>
        public IReadOnlyList<INode> Children
        {
            get
            {
                return children.Value;
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
        public Value GetValue(int row, int column)
        {
            if (row < 0 || row >= ReversiTable.Size ||
                column < 0 || column >= ReversiTable.Size)
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
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder("  ");
            for (int i = 0; i < ReversiTable.Size; ++i)
            {
                sb.Append($"{i} ");
            }

            sb.AppendLine();
            for (int i = 0; i < ReversiTable.Size; ++i)
            {
                sb.Append($"{i}|");
                for (int j = 0; j < ReversiTable.Size; ++j)
                {
                    Value value = stateTable.GetValue(i, j);
                    if (value == Value.None)
                    {
                        sb.Append('-');
                    }
                    else if (value == Value.Maximizing)
                    {
                        sb.Append('B');
                    }
                    else
                    {
                        sb.Append('W');
                    }

                    if (j < ReversiTable.Size - 1)
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
        /// Computes all children nodes of current node.
        /// </summary>
        /// <returns>Children nodes.</returns>
        private IReadOnlyList<ReversiNode> GetChildren()
        {
            var children = GetValidMoves(Player).Select(move =>
            {
                ReversiTable table = GetTableForMove(move);
                ReversiTable stabilityTable = table.GetTableWithUpdatedStability();

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
                    dx >= 0 && dx < ReversiTable.Size &&
                    dy >= 0 && dy < ReversiTable.Size &&
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
        /// Candidates the cells.
        /// </summary>
        /// <param name="player">The player making the move.</param>
        /// <returns>List of valid moves.</returns>
        private IEnumerable<Tuple<Position, IEnumerable<Position>>> GetValidMoves(Value player)
        {
            foreach (Position cell in GetFreeCells())
            {
                IList<Position> capturedDirections = GetDirectionsCapturedByMove(player, cell).ToList();
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
            for (int i = 0; i < ReversiTable.Size; ++i)
            {
                for (int j = 0; j < ReversiTable.Size; ++j)
                {
                    if (stateTable.GetValue(i, j) == Value.None)
                    {
                        yield return new Position(i, j);
                    }
                }
            }
        }

        /// <summary>
        /// Gets directions in which the specified move flips opponent's cells.
        /// </summary>
        /// <param name="player">The player making the move.</param>
        /// <param name="move">The move represented by cell coordinates.</param>
        /// <returns>Directions in which the specified move flips opponent's cells.</returns>
        private IEnumerable<Position> GetDirectionsCapturedByMove(Value player, Position move)
        {
            return ReversiConstants.Directions.Where(direction => MoveCapturesDirection(player, move, direction));
        }

        /// <summary>
        /// Determines whether the specified move flips opponent's cells in given direction.
        /// </summary>
        /// <param name="player">The player making the move.</param>
        /// <param name="move">The move represented by cell coordinates.</param>
        /// <param name="direction">The direction represented by coordinate deltas.</param>
        /// <returns>True if the move flips opponent's cells in given direction, false otherwise.</returns>
        private bool MoveCapturesDirection(Value player, Position move, Position direction)
        {
            Debug.Assert(player != Value.None);
            Value opponent = player == Value.Maximizing
                ? Value.Minimizing
                : Value.Maximizing;

            int dx = move.Item1 + direction.Item1;
            int dy = move.Item2 + direction.Item2;

            bool opponentCellsCaptured = false;

            while (dx >= 0 && dx < ReversiTable.Size && dy >= 0 && dy < ReversiTable.Size)
            {
                Value value = stateTable.GetValue(dx, dy);

                if (!opponentCellsCaptured)
                {
                    if (value == opponent)
                    {
                        opponentCellsCaptured = true;
                        continue;
                    }
                    else
                    {
                        return false;
                    }
                }

                if (value == Value.None)
                {
                    return false;
                }
                else if (value == player)
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
            if (children.Value.Count == 0)
            {
                return GetScore();
            }

            return GetMobilityScore() + GetStabilityScore();
        }

        /// <summary>
        /// Computes the final game score.
        /// </summary>
        /// <returns>The final score.</returns>
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
            int opponentChildrenCount = GetValidMoves(Opponent).Count();

            return (children.Value.Count - opponentChildrenCount)
                * (Player == Value.Maximizing ? 1 : -1)
                * ReversiConstants.MobilityPoints;
        }

        /// <summary>
        /// Computes the stability heuristics.
        /// </summary>
        /// <returns>Stability heuristics.</returns>
        private int GetStabilityScore()
        {
            return stateTable.GetStabilityScore() * ReversiConstants.StabilityPoints;
        }
    }
}
