using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlphaBeta
{
    public class Program
    {
        public static void Main(string[] args)
        {
            AlphaBeta search = new AlphaBeta(new ReversiNode(), 5);
            INode best = search.Best(true);
        }
    }
}
