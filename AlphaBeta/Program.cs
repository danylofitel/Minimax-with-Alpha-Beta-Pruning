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
            AlphaBeta search = new AlphaBeta(3);
            ReversiNode state = new ReversiNode();

            bool player = true;
            while (true)
            {
                Console.WriteLine(state);

                IReadOnlyList<ReversiNode> children = state.Children
                    .Select(node => node as ReversiNode)
                    .ToList().AsReadOnly();
                if (children.Count == 0)
                {
                    return;
                }

                if (player)
                {
                    //Console.WriteLine("Your move");
                    //string input = Console.ReadLine();
                    //int row = int.Parse(input.Substring(0, 1));
                    //int column = int.Parse(input.Substring(1, 1));

                    //var moves = children.Where(node => node.GetValue(row, column) == ReversiValue.Maximizing);
                    //if (moves.Count() > 0)
                    //{
                    //    state = moves.First();
                    //}
                    state = (ReversiNode)search.Best(state, true);
                }
                else
                {
                    state = (ReversiNode)search.Best(state, false);
                }

                player = !player;
            }
        }
    }
}
