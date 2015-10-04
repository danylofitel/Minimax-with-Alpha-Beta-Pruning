//-----------------------------------------------------------------------
// <copyright file="Program.cs" author="Danylo Fitel">
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Linq;

namespace AlphaBeta
{
    public class Program
    {
        public static void Main(string[] args)
        {
            AlphaBetaDemo(new TicTacToeNode(), 9U);
            AlphaBetaDemo(new ReversiNode(), 5U);
        }

        private static void AlphaBetaDemo<Node>(Node state, uint depth) where Node : INode
        {
            AlphaBeta<Node> search = new AlphaBeta<Node>(depth);

            while (state.Children.Any())
            {
                Console.WriteLine(state);
                state = search.Best(state);
            }

            Console.WriteLine(state);

            Value winner = state.Heuristics > 0 ? Value.Maximizing
                : state.Heuristics < 0 ? Value.Minimizing
                : Value.None;

            Console.WriteLine($"Game over. Winner: {winner}.");
            Console.WriteLine();
        }
    }
}
