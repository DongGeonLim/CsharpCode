using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace hello
{
    class Program
    {
        static void Main(string[] args)
        {
            int time = 9;
            while (true)
            {
                Thread.Sleep(777);
                Console.Clear();

                Console.WriteLine("집가고싶다 ({0})", time);
                
                if (time > 18)
                {
                    Console.Clear();
                    Console.WriteLine("집가야지 ㅋㅋ");
                    break;
                }
                else
                {
                    time++;
                }
            }

        }
    }
}
