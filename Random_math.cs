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
            int num = 0;
            int count = 0;
            for (int i = 1; i < 6; i++)
            {
                Random rnd = new Random();
                int a = rnd.Next(0, 100);
                int b = rnd.Next(0, 100);

                int c = a + b;

                Console.Write("{0}: 다음 두 수의 합은 몇? (총 5문제)", i);
                Console.WriteLine("");
                Console.WriteLine("{0} + {1} = ??", a, b);
                num = int.Parse(Console.ReadLine());

                if (num == c)
                {
                    Console.WriteLine("== 정답 ==");
                    count++;
                }
                else
                {
                    Console.WriteLine("오답(정답은: {0})", c);
                }
            }

            Console.WriteLine("{0}문제 정답", count);
        }
    }
}
