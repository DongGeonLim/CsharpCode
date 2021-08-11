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
            Console.Write("정수를 입력하세요");
            string hello = Console.ReadLine();
            int h = int.Parse(hello);

            if (h > 5)
            {
                Console.WriteLine("good");
            }
            else if (h < 5)
            {
                Console.WriteLine("h = {0}", h);
            }
            else
            {
                if(2>1 && 3<4 || 1>2) // => (true && ture) = true, true || false = true
                {
                    Console.WriteLine("hello");
                }
                
            }
        }
    }
}
