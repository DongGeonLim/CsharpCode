using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hello
{
    class Program
    {
        //1.
        static void InitTitle() //리턴형 X, 파라미터 X, 코드를 분류하기 위해 사용
        {
            Console.WriteLine("짝수 & 홀수 보여주기(0~100)");
        }

        static void PrintEven() //
        {
            for (int i = 0; i <= 100; i++)
            {
                if (i % 2 == 0)
                    Console.Write(" 짝수: {0}", i);
                if (i % 10 == 0 && i != 0)
                    Console.WriteLine();
            }

        }

        static void PrintOdd() //
        {
            for (int i = 0; i <= 100; i++)
            {
                if (i % 2 != 0)
                    Console.Write(" 홀수: {0}", i);
                if (i % 10 == 1 && i != 1)
                    Console.WriteLine();
            }
        }

        //----------------------------------

        //2.
        static int Add() //리턴형 O, 파라미터 X
        {
            int a = 0;
            return ++a;
        }

        static int InputNum() //
        {
            Console.Write("입력하려는 정수를 넣어주세연");
            int num = int.Parse(Console.ReadLine());
            return num;
        }

        //----------------------------------

        //3.
        static int Square(int i) //리턴형 O, 파라미터 O
        {
            int input = i;
            return input * input;
        }

        //----------------------------------

        //4.
        static public void ValueSwap(int a, int b) //리턴형 X, 파라미터 O
        {
            int temp = a; //temp로 데이터 잠시 보관
            a = b;
            b = temp;

            Console.WriteLine("ValueSwap");
            Console.WriteLine("num1: {0}     num2: {1}", a, b);
        }

        static public void RefSwap(ref int a, ref int b) //
        {
            int temp = a; //temp로 데이터 잠시 보관
            a = b;
            b = temp;

            Console.WriteLine("RefSwap");
            Console.WriteLine("num1: {0}     num2: {1}", a, b);
        }

        //메인 함수

        static void Main(string[] args)
        {
            //1
            InitTitle();
            PrintEven();
            PrintOdd();

            //--------------------------

            //2
            int num = 0;
            num += Add();

            Console.WriteLine("Num: " + num);
            Console.WriteLine("입력하신 정수는 {0}", InputNum());

            //--------------------------

            //3
            int a = 2;
            int resultA = Square(a); //F12, Alt+F12 (함수 단축키)
            Console.WriteLine("resultA: " + resultA);

            int b = 4;
            int resultB = Square(b);
            Console.WriteLine("resultB: " + resultB);

            //--------------------------

            //4
            int num1 = 100;
            int num2 = 900;

            ValueSwap(num1, num2);
            Console.WriteLine("num1: {0}     num2: {1}", num1, num2);

            RefSwap(ref num1, ref num2);
            Console.WriteLine("num1: {0}     num2: {1}", num1, num2);
        }
    }
}
