//-----------------------------------------------------------------------
// <copyright file="ReversiNode.cs" author="Danylo Fitel">
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Position = System.Tuple<int, int>;

namespace AlphaBeta
{
    /// <summary>
    /// Possible game cell values.
    /// </summary>
    public enum ReversiValue : int
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

    /// <summary>
    /// An immutable representation of Reversi game position.
    /// </summary>
    public class ReversiNode : INode
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
        /// 4 corner cells.
        /// </summary>
        private static readonly IReadOnlyList<Position> CornerCells = new List<Position>
        {
            new Position(0, 0),             new Position(0, Size - 1),
            new Position(Size - 1, 0),      new Position(Size - 1, Size - 1)
        }.AsReadOnly();

        /// <summary>
        /// 9 possible directions from any cell.
        /// </summary>
        private static readonly IReadOnlyList<Position> Directions = new List<Position>
        {
            new Position(-1, -1),    new Position(-1, 0),    new Position(-1, 1),
            new Position(0, -1),     new Position(0, 0),     new Position(0, 1),
            new Position(1, -1),     new Position(1, 0),     new Position(1, 1)
        }.AsReadOnly();

        /// <summary>
        /// The flags indicating which cells are used (0 is empty, 1 is used).
        /// </summary>
        private readonly ulong OccupiedCellsTable;

        /// <summary>
        /// The flags indicating which player's disks are at game cells (1 is maximizing player's cell).
        /// </summary>
        private readonly ulong PlayersTable;

        /// <summary>
        /// Flags indicating which cells are stable.
        /// </summary>
        private readonly ulong StabilityTable;

        /// <summary>
        /// The children nodes.
        /// </summary>
        private readonly Lazy<IReadOnlyList<ReversiNode>> children;

        /// <summary>
        /// Heuristics of current position.
        /// </summary>
        private readonly Lazy<int> heuristics;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReversiNode" /> class with the starting position.
        /// </summary>
        public ReversiNode() : this(0x1818000000UL, 0x810000000UL, 0UL, true)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReversiNode" /> class.
        /// </summary>
        /// <param name="table">The game state.</param>
        /// <param name="stabilityTable">The flags of stable cells.</param>
        /// <param name="maximizing">Indicates whether it is maximizing player's turn.</param>
        private ReversiNode(
            ulong occupiedCellsTable,
            ulong playersTable,
            ulong stabilityTable,
            bool maximizing)
        {
            OccupiedCellsTable = occupiedCellsTable;
            PlayersTable = playersTable;
            StabilityTable = stabilityTable;

            Maximizing = maximizing;
            Player = maximizing ? ReversiValue.Maximizing : ReversiValue.Minimizing;
            Opponent = maximizing ? ReversiValue.Minimizing : ReversiValue.Maximizing;

            children = new Lazy<IReadOnlyList<ReversiNode>>(() => GetChildren());
            heuristics = new Lazy<int>(() => GetHeuristics());
        }

        /// <summary>
        /// Gets a value indicating whether it is maximizing's player turn.
        /// </summary>
        public bool Maximizing { get; private set; }

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
        /// Compares to another Reversi node.
        /// </summary>
        /// <param name="other">The other node.</param>
        /// <returns>Comparison result.</returns>
        public int CompareTo(INode other)
        {
            return Heuristics.CompareTo(other.Heuristics);
        }

        /// <summary>
        /// Gets the table value at given coordinates using bit manipulation.
        /// </summary>
        /// <param name="i">The row.</param>
        /// <param name="j">The column.</param>
        /// <returns>Table value at given coordinates.</returns>
        private ReversiValue GetValue(int i, int j)
        {
            if (!GetBit(OccupiedCellsTable, i, j))
            {
                return ReversiValue.Empty;
            }

            return GetBit(PlayersTable, i, j) == true
                ? ReversiValue.Maximizing
                : ReversiValue.Minimizing;
        }

        /// <summary>
        /// Computes all children nodes of current node.
        /// </summary>
        /// <returns>Children nodes.</returns>
        private IReadOnlyList<ReversiNode> GetChildren()
        {
            return GetValidMoves().Select(move =>
            {
                Tuple<ulong, ulong> table = GetTableForMove(move);
                ulong stabilityTable = GetStabilityTableForMove(move);

                return new ReversiNode(table.Item1, table.Item2, stabilityTable, !Maximizing);
            }).ToList().AsReadOnly();
        }

        /// <summary>
        /// Returns the table after given move.
        /// </summary>
        /// <returns>The table after given move.</returns>
        private Tuple<ulong, ulong> GetTableForMove(Tuple<Position, IEnumerable<Position>> move)
        {
            ulong occupiedCellsTable = OccupiedCellsTable;
            ulong playersTable = PlayersTable;

            Position cell = move.Item1;
            SetBit(ref occupiedCellsTable, cell.Item1, cell.Item2, true);
            SetBit(ref playersTable, cell.Item1, cell.Item2, Player == ReversiValue.Maximizing);

            foreach (Position direction in move.Item2)
            {
                int dx = cell.Item1 + direction.Item1;
                int dy = cell.Item2 + direction.Item2;

                while (dx >= 0 && dx < Size && dy >= 0 && dy < Size && GetValue(dx, dy) == Opponent)
                {
                    SetBit(ref playersTable, dx, dy, Player == ReversiValue.Maximizing);
                    dx += direction.Item1;
                    dy += direction.Item2;
                }
            }

            return new Tuple<ulong, ulong>(occupiedCellsTable, playersTable);
        }

        /// <summary>
        /// Returns the stability table after given move.
        /// </summary>
        /// <returns>The stability table after given move.</returns>
        private ulong GetStabilityTableForMove(Tuple<Position, IEnumerable<Position>> move)
        {
            ulong stable = StabilityTable;

            Position cell = move.Item1;
            if (CornerCells.Contains(cell))
            {
                // TODO
            }

            return stable;
        }

        /// <summary>
        /// Candidates the cells.
        /// </summary>
        /// <returns>List of valid moves.</returns>
        private IEnumerable<Tuple<Position, IEnumerable<Position>>> GetValidMoves()
        {
            foreach (Position cell in GetFreeCells())
            {
                IEnumerable<Position> capturedDirections = GetDirectionsCapturedByMove(cell);
                if (capturedDirections.Any())
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
            for (int i = 0; i < Size; ++i)
            {
                for (int j = 0; j < Size; ++j)
                {
                    if (GetValue(i, j) == ReversiValue.Empty)
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
            return Directions.Where(direction => MoveCapturesDirection(move, direction));
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

            while (dx >= 0 && dx < Size && dy >= 0 && dy < Size)
            {
                ReversiValue value = GetValue(dx, dy);

                if (!opponentCellsCaptured)
                {
                    if (value == ReversiValue.Empty)
                    {
                        return false;
                    }
                    else if (value == Opponent)
                    {
                        opponentCellsCaptured = true;
                        continue;
                    }
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
            if (Children.Count == 0)
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
            int occupiedCells = SparseBitcount(OccupiedCellsTable);
            int maxPlayerCells = SparseBitcount(PlayersTable);
            int minPlayerCells = occupiedCells - maxPlayerCells;

            return (maxPlayerCells - minPlayerCells) * VictoryPoints;
        }

        /// <summary>
        /// Computes the mobility heuristics.
        /// </summary>
        /// <returns>Mobility heuristics.</returns>
        private int GetMobilityScore()
        {
            return Children.Count * (Maximizing ? 1 : -1) * MobilityPoints;
        }

        /// <summary>
        /// Computes the stability heuristics.
        /// </summary>
        /// <returns>Stability heuristics.</returns>
        private int GetStabilityScore()
        {
            int score = 0;

            for (int i = 0; i < Size; ++i)
            {
                for (int j = 0; j < Size; ++j)
                {
                    if (((StabilityTable >> i * Size + j) & 1UL) == 1UL)
                    {
                        score += GetValue(i, j) == ReversiValue.Maximizing ? 1 : -1;
                    }
                }
            }

            return score * StabilityPoints;
        }

        /// <summary>
        /// Gets the specific bit.
        /// </summary>
        /// <param name="table">The table.</param>
        /// <param name="i">The row.</param>
        /// <param name="j">The column.</param>
        /// <returns>Bit value.</returns>
        private static bool GetBit(ulong table, int i, int j)
        {
            return ((table >> i * Size + j) & 1UL) == 1UL;
        }

        /// <summary>
        /// Sets the specific bit.
        /// </summary>
        /// <param name="table">The table.</param>
        /// <param name="i">The row.</param>
        /// <param name="j">The column.</param>
        /// <param name="value">New value of the bit.</param>
        private static void SetBit(ref ulong table, int i, int j, bool value)
        {
            if (value)
            {
                table |= 1UL << i * Size + j;
            }
            else
            {
                table &= ~(1UL << i * Size + j);
            }
        }

        /// <summary>
        /// Counts the number of set bits in an unsigned long.
        /// </summary>
        /// <param name="n">The n.</param>
        /// <returns>Number of set bits.</returns>
        private static int SparseBitcount(ulong n)
        {
            int count = 0;
            while (n != 0)
            {
                count++;
                n &= (n - 1);
            }

            return count;
        }
    }
}
