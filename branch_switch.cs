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
            string Grade = null;

            Console.Write("점수를 입력하세요: ");
            num = int.Parse(Console.ReadLine());

            switch(num / 10)
            {
                case 10: // => if((num / 10) == 9 || (num / 10) == 10)
                case 9:
                    Grade = "A";
                    break;
                case 8:
                    Grade = "B";
                    break;
                case 7:
                    Grade = "C";
                    break;
                case 6:
                    Grade = "D";
                    break;
                default:
                    Grade = "F";
                    break;
            }

            Console.Write("결과값은 = {0}", Grade);
        }
    }
}
