using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace performace
{
    class Program
    {
        static void Main(string[] args)
        {
            List<int> numbers = new List<int>();
            int n = 1000000;

            for (int x = 0; x < n;x++ )
            {
                numbers.Add(x);                    
            }

        }
    }
}
