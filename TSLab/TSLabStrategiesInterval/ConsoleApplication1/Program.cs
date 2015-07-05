using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            int shares = 1;
            int closedPositions = 4;
            int sharesCounter = 3;
            shares = shares + (int)Math.Floor((double)(closedPositions / sharesCounter)) * shares;


        }
    }
}
