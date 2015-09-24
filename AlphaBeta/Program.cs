//-----------------------------------------------------------------------
// <copyright file="Program.cs" author="Danylo Fitel">
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Diagnostics;

namespace AlphaBeta
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Stopwatch cw = new Stopwatch();
            cw.Start();

            AlphaBeta search = new AlphaBeta(new ReversiNode(), 6);
            INode best = search.Best(true);

            Console.WriteLine(cw.ElapsedMilliseconds);
        }
    }
}
