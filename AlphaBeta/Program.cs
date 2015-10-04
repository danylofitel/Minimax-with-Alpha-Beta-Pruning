//-----------------------------------------------------------------------
// <copyright file="Program.cs" author="Danylo Fitel">
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;

namespace AlphaBeta
{
    public class Program
    {
        public static void Main(string[] args)
        {
            ReversiDemo();
        }

        private static void TicTacToeDemo()
        {
            AlphaBeta<TicTacToeNode> search = new AlphaBeta<TicTacToeNode>(9);
            TicTacToeNode state = new TicTacToeNode();

            bool maximizing = true;
            while (true)
            {
                Console.WriteLine(state);

                if (state.Children.Any())
                {
                    state = search.Best(state, maximizing);
                    maximizing = !maximizing;
                }
                else
                {
                    Value winner = state.Heuristics > 0 ? Value.Maximizing
                        : state.Heuristics < 0 ? Value.Minimizing
                        : Value.Empty;
                    Console.WriteLine($"Game over. Winner: {winner}.");
                    return;
                }
            }
        }

        private static void ReversiDemo()
        {
            AlphaBeta<ReversiNode> search = new AlphaBeta<ReversiNode>(6);
            ReversiNode state = new ReversiNode();

            bool maximizing = true;
            while (true)
            {
                Console.WriteLine(state);

                IReadOnlyList<ReversiNode> children = state.Children
                    .Select(node => node as ReversiNode)
                    .ToList().AsReadOnly();
                if (children.Count == 0)
                {
                    if (state.Heuristics > 0)
                    {
                        Console.WriteLine("Black won.");
                    }
                    else if (state.Heuristics < 0)
                    {
                        Console.WriteLine("White won.");
                    }
                    else
                    {
                        Console.WriteLine("It's a tie.");
                    }

                    return;
                }

                if (maximizing)
                {
                    //do
                    //{
                    //    Console.WriteLine("Your move");
                    //    string input = Console.ReadLine();
                    //    int row = int.Parse(input.Substring(0, 1));
                    //    int column = int.Parse(input.Substring(1, 1));

                    //    var moves = children.ToList();
                    //    if (moves.Count == 1)
                    //    {
                    //        state = moves.First();
                    //    }

                    //    state = children.FirstOrDefault(node =>
                    //        node.GetValue(row, column) == ReversiValue.Maximizing);
                    //} while (state == null);

                    state = search.Best(state, state.Player == Value.Maximizing);
                }
                else
                {
                    state = search.Best(state, state.Player == Value.Maximizing);
                }

                maximizing = !maximizing;
            }
        }
    }
}
