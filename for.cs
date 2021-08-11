using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hello
{
    class Program
    {
        static void Main(string[] args)
        {
            int total = 0;

            for (int i = 0; i <= 10; i++)
            {
                total += i;
            }

            Console.WriteLine("total:   " + total);

            // ------------------------------------

            for (int i = 2; i < 10; i++)
            {
                for (int j = 1; j < 10; j++)
                {
                    Console.WriteLine("{0} X {1} = {2}", i, j, (i*j));
                    if (j == 9) Console.WriteLine(); //빈 줄 생성 (가독성)
                }
            }
        }
    }
}
