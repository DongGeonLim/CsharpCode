using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace amadong
{
    internal class Program
    {
        static void Main(string[] args)
        {
            int[,] arrNum = new int[3, 2];
            int[,] arrNum2 = new int[,] { { 0, 1 }, { 2, 3 }, { 4, 5 } };
            int[,] arrNum3 = { { 0, 1 }, { 2, 3 }, { 4, 5 } };

            foreach(int temp in arrNum2)
            {
                Console.Write("  " + temp);
            }
            Console.WriteLine("\narrMulti.Length: " + arrNum2.Length);

            foreach (int temp in arrNum3)
            {
                Console.Write("  " + temp);
            }
            Console.WriteLine("\narrMulti.Length: " + arrNum3.Length);
        }
    }
}
