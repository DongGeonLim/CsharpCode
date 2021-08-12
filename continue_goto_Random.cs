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
           for (int i = 0; i < 10; i++)
           {
            if (i == 5)
            {
                Console.WriteLine();
                continue;
            }
            Console.WriteLine("i:  " + i);
           }

           //------------------------

           for (int p = 0; p < 10; p++)
           {
            if (p == 5)
            {
                goto AA;
            }
            if (p == 7)
            {
                goto BB;
            }
           }
        AA:
        Console.WriteLine("AA~~~~~~");

        BB:
        Console.WriteLine("BB!!!!!!!!");

        // (+)

        Random rnd = new Random();
        int a = rnd.Next(0,100);
        int b = rnd.Next(0,100);

        Console.WriteLine("a: {0} b: {1}", a, b);

        }
    }
}
