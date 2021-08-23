using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hello
{
    class Program
    {
        static void InitNum(out int addNum)
        {
            addNum = 100;
        }

        static void InitRefNum(ref int refNum)
        {
            refNum = 100;
        }

        //--------------------------------------------------

        static void PrintValue(int a, int b, int c = 100, int d = 0) // int c 건너뛰고 int b = 100; 해버리면 오류남.
        // int b가 default가 많이 필요할 것 같으면 int b를 가장 마지막에 쓰면 됨. (지금 d 자리)
        {
            Console.WriteLine("PrintValue: {0}  {1}  {2}  {3}", a, b, c, d);
        }

        static void Main(string[] args)
        {
            int a; // 초기화 안했음
            int b;
            b = 0; // 초기화

            InitNum(out a); // a에 100을 대입했음. (주소값을 넘겼기 때문)
            Console.WriteLine("a: " + a);

            InitRefNum(ref b); // b를 초기화 하지 않고는 사용 못함.
            Console.WriteLine("b: " + b);

            //--------------------------------------------------

            PrintValue(0, 0, 0, 0);
            PrintValue(100, 2, 1); // 3개 넣어도 4개 나오고
            PrintValue(300, 300); // 2개 넣어도 4개 나오는 편리함
        }
    }
}
