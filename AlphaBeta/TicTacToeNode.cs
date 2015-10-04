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
            Value rows = RowFilled();
            if (rows != Value.None)
            {
                return rows;
            }

            Value columns = ColumnFilled();
            if (columns != Value.None)
            {
                return columns;
            }

            return DiagonalFilled();
        }

        /// <summary>
        /// Checks if any row is filled.
        /// </summary>
        /// <returns>True if at least one row is filled.</returns>
        private Value RowFilled()
        {
            for (int row = 0; row < TicTacToeTable.Size; ++row)
            {
                Value first = stateTable.GetValue(row, 0);
                for (int column = 1; first != Value.None && column < TicTacToeTable.Size; ++column)
                {
                    if (stateTable.GetValue(row, column) != first)
                    {
                        first = Value.None;
                    }
                }

                if (first != Value.None)
                {
                    return first;
                }
            }

            // No filled rows found.
            return Value.None;
        }

        /// <summary>
        /// Checks if any column is filled.
        /// </summary>
        /// <returns>True if at least one column is filled.</returns>
        private Value ColumnFilled()
        {
            for (int column = 0; column < TicTacToeTable.Size; ++column)
            {
                Value first = stateTable.GetValue(0, column);

                for (int row = 1; first != Value.None && row < TicTacToeTable.Size; ++row)
                {
                    if (stateTable.GetValue(row, column) != first)
                    {
                        first = Value.None;
                    }
                }

                if (first != Value.None)
                {
                    return first;
                }
            }

            // No filled columns found.
            return Value.None;
        }

        /// <summary>
        /// Checks if any diagonals are filled.
        /// </summary>
        /// <returns>True if at least one diagonal is filled.</returns>
        private Value DiagonalFilled()
        {
            Value first = stateTable.GetValue(0, 0);
            for (int i = 1; first != Value.None && i < TicTacToeTable.Size; ++i)
            {
                if (stateTable.GetValue(i, i) != first)
                {
                    first = Value.None;
                }
            }

            if (first != Value.None)
            {
                return first;
            }

            first = stateTable.GetValue(0, TicTacToeTable.Size - 1);
            for (int i = 1; first != Value.None && i < TicTacToeTable.Size; ++i)
            {
                if (stateTable.GetValue(i, TicTacToeTable.Size - i - 1) != first)
                {
                    first = Value.None;
                }
            }

            return first;
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
                return 0;
            }
        }
    }
}
