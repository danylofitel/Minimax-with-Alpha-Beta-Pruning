//-----------------------------------------------------------------------
// <copyright file="TicTacToeNode.cs" author="Danylo Fitel">
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
    /// Tic-Tac-Toe game node.
    /// </summary>
    public class TicTacToeNode : INode
    {
        /// <summary>
        /// The state table.
        /// </summary>
        private readonly TicTacToeTable stateTable;

        /// <summary>
        /// The winner.
        /// </summary>
        private readonly Lazy<Value> winner;

        /// <summary>
        /// The children of current node.
        /// </summary>
        private readonly Lazy<IReadOnlyList<TicTacToeNode>> children;

        /// <summary>
        /// The heuristic value of current node.
        /// </summary>
        private readonly Lazy<int> heuristics;

        /// <summary>
        /// Initializes a new instance of the <see cref="TicTacToeNode"/> class
        /// with an initial game state.
        /// </summary>
        public TicTacToeNode() :
            this(new TicTacToeTable(), Value.Maximizing)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TicTacToeNode"/> class.
        /// </summary>
        private TicTacToeNode(TicTacToeTable table, Value player)
        {
            stateTable = table;

            winner = new Lazy<Value>(() => IsFinished(),
                LazyThreadSafetyMode.ExecutionAndPublication);

            children = new Lazy<IReadOnlyList<TicTacToeNode>>(() => GetChildren(),
                LazyThreadSafetyMode.ExecutionAndPublication);

            heuristics = new Lazy<int>(() => GetHeuristics(),
                LazyThreadSafetyMode.ExecutionAndPublication);

            Debug.Assert(player != Value.None);

            Player = player;
            Opponent = player == Value.Maximizing
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
        /// Gets the list of children nodes.
        /// </summary>
        public IReadOnlyList<INode> Children
        {
            get
            {
                return children.Value;
            }
        }

        /// <summary>
        /// Gets the heuristics value of current position.
        /// </summary>
        public int Heuristics
        {
            get
            {
                return heuristics.Value;
            }
        }

        /// <summary>
        /// Compares to another Tic-Tac-Toe node.
        /// </summary>
        /// <param name="other">The other node.</param>
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
            for (int i = 0; i < TicTacToeTable.Size; ++i)
            {
                sb.Append($"{i} ");
            }

            sb.AppendLine();
            for (int i = 0; i < TicTacToeTable.Size; ++i)
            {
                sb.Append($"{i}|");
                for (int j = 0; j < TicTacToeTable.Size; ++j)
                {
                    Value value = stateTable.GetValue(i, j);
                    if (value == Value.None)
                    {
                        sb.Append('-');
                    }
                    else if (value == Value.Maximizing)
                    {
                        sb.Append('X');
                    }
                    else
                    {
                        sb.Append('O');
                    }

                    if (j < TicTacToeTable.Size - 1)
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
        /// Computes the child nodes.
        /// </summary>
        /// <returns>The list of child nodes.</returns>
        private IReadOnlyList<TicTacToeNode> GetChildren()
        {
            if (winner.Value != Value.None)
            {
                return new List<TicTacToeNode>().AsReadOnly();
            }

            return GetFreeCells().Select(move =>
            {
                return new TicTacToeNode(GetTableForMove(move), Opponent);
            }).ToList().AsReadOnly();
        }

        /// <summary>
        /// Gets the free cells.
        /// </summary>
        /// <returns>Enumeration of free cells.</returns>
        private IEnumerable<Position> GetFreeCells()
        {
            for (int i = 0; i < TicTacToeTable.Size; ++i)
            {
                for (int j = 0; j < TicTacToeTable.Size; ++j)
                {
                    if (stateTable.GetValue(i, j) == Value.None)
                    {
                        yield return new Position(i, j);
                    }
                }
            }
        }

        /// <summary>
        /// Determines whether the game is finished.
        /// </summary>
        /// <returns>True if the game is over.</returns>
        private Value IsFinished()
        {
            // Check if at least one line is filled by the same player.
            foreach (var line in TicTacToeConstants.Lines)
            {
                Position first = line[0];
                Value current = stateTable.GetValue(first.Item1, first.Item2);

                for (int i = 1; current != Value.None && i < TicTacToeTable.Size; ++i)
                {
                    Value val = stateTable.GetValue(line[i].Item1, line[i].Item2);
                    if (val != current)
                    {
                        current = Value.None;
                        break;
                    }
                }

                if (current != Value.None)
                {
                    return current;
                }
            }

            return Value.None;
        }

        /// <summary>
        /// Gets the table after given move.
        /// </summary>
        /// <param name="move">The move.</param>
        /// <returns>The table after move.</returns>
        private TicTacToeTable GetTableForMove(Position move)
        {
            TicTacToeTable newTable = stateTable;
            newTable.SetValue(Player, move.Item1, move.Item2);
            return newTable;
        }

        /// <summary>
        /// Computes the heuristics of current node.
        /// </summary>
        /// <returns>Heuristic value of current node</returns>
        private int GetHeuristics()
        {
            if (winner.Value == Value.Maximizing)
            {
                return TicTacToeConstants.VictoryPoints;
            }
            else if (winner.Value == Value.Minimizing)
            {
                return -TicTacToeConstants.VictoryPoints;
            }
            else
            {
                return GetWinPathsCount() * TicTacToeConstants.PotentialWinningLinePoints;
            }
        }

        /// <summary>
        /// Gets score for potential win paths.
        /// </summary>
        /// <returns>Difference between players' number of paths not yet occupied by opponent.</returns>
        private int GetWinPathsCount()
        {
            int xCount = 0;
            int oCount = 0;

            foreach (var line in TicTacToeConstants.Lines)
            {
                bool containsX = false;
                bool containsO = false;

                foreach (Value cell in line.Select(
                    position => stateTable.GetValue(position.Item1, position.Item2)))
                {
                    containsX |= cell == Value.Maximizing;
                    containsO |= cell == Value.Minimizing;
                }

                xCount += containsX ? 1 : 0;
                oCount += containsO ? 1 : 0;
            }

            return xCount - oCount;
        }
    }
}
