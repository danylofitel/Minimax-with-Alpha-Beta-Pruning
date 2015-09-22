//-----------------------------------------------------------------------
// <copyright file="ReversiNode.cs" author="Danylo Fitel">
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;

namespace AlphaBeta
{
    using Position = Tuple<int, int>;

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
        /// The game position.
        /// </summary>
        private readonly ReversiValue[][] Table;

        /// <summary>
        /// Flags indicating which cells are stable.
        /// </summary>
        private readonly bool[][] StabilityTable;

        /// <summary>
        /// The children nodes.
        /// </summary>
        private readonly Lazy<IReadOnlyList<ReversiNode>> children;

        /// <summary>
        /// Heuristics of current position.
        /// </summary>
        private readonly Lazy<int> heuristics;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReversiNode" /> class.
        /// </summary>
        public ReversiNode(): this(new ReversiValue[Size][], new bool[Size][], true)
        {
            for (int i = 0; i < Size; ++i)
            {
                Table[i] = new ReversiValue[Size];
                StabilityTable = new bool[Size][];
            }

            int middle = Size / 2 - 1;

            Table[middle][middle + 1] = ReversiValue.Maximizing;
            Table[middle + 1][middle] = ReversiValue.Maximizing;

            Table[middle][middle] = ReversiValue.Minimizing;
            Table[middle + 1][middle + 1] = ReversiValue.Minimizing;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReversiNode" /> class.
        /// </summary>
        /// <param name="table">The game state.</param>
        /// <param name="stable">The flags of stable cells.</param>
        /// <param name="maximizing">Indicates whether it is maximizing player's turn.</param>
        private ReversiNode(ReversiValue[][] table, bool[][] stable, bool maximizing)
        {
            Table = table;
            StabilityTable = stable;

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
        /// Computes all children nodes of current node.
        /// </summary>
        /// <returns>Children nodes.</returns>
        private IReadOnlyList<ReversiNode> GetChildren()
        {
            return GetValidMoves().Select(move =>
            {
                ReversiValue[][] table = GetTableForMove(move);
                bool[][] stabilityTable = GetStabilityTableForMove(move);

                return new ReversiNode(table, stabilityTable, !Maximizing);
            }).ToList().AsReadOnly();
        }

        /// <summary>
        /// Returns the table after given move.
        /// </summary>
        /// <returns>The table after given move.</returns>
        private ReversiValue[][] GetTableForMove(Tuple<Position, IEnumerable<Position>> move)
        {
            ReversiValue[][] table = new ReversiValue[Size][];
            for (int i = 0; i < Size; ++i)
            {
                table[i] = new ReversiValue[Size];
                for (int j = 0; j < Size; ++j)
                {
                    table[i][j] = Table[i][j];
                }
            }

            Position cell = move.Item1;
            table[cell.Item1][cell.Item2] = Player;

            // TODO

            return table;
        }

        /// <summary>
        /// Returns the stability table after given move.
        /// </summary>
        /// <returns>The stability table after given move.</returns>
        private bool[][] GetStabilityTableForMove(Tuple<Position, IEnumerable<Position>> move)
        {
            bool[][] stable = new bool[Size][];
            for (int i = 0; i < Size; ++i)
            {
                stable[i] = new bool[Size];
                for (int j = 0; j < Size; ++j)
                {
                    stable[i][j] = StabilityTable[i][j];
                }
            }

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
                    if (Table[i][j] == ReversiValue.Empty)
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
            Position neighbor = new Position(
                move.Item1 + direction.Item1,
                move.Item2 + direction.Item2);

            bool opponentCellsCaptured = false;

            while (
                neighbor.Item1 >= 0 && neighbor.Item1 < Size &&
                neighbor.Item1 >= 0 && neighbor.Item2 < Size)
            {
                ReversiValue value = Table[neighbor.Item1][neighbor.Item2];

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
            return Table.Sum(row => row.Sum(cell =>
            {
                switch (cell)
                {
                    case ReversiValue.Maximizing:
                        return 1;
                    case ReversiValue.Minimizing:
                        return -1;
                    case ReversiValue.Empty:
                    default:
                        return 0;
                }
            })) * VictoryPoints;
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
                for (int j = 0; i < Size; ++j)
                {
                    if (StabilityTable[i][j])
                    {
                        score += Table[i][j] == ReversiValue.Maximizing ? 1 : -1;
                    }
                }
            }

            return score * StabilityPoints;
        }
    }
}
