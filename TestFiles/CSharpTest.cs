using System;

namespace SampleApp
{
    public class Calculator
    {
        private int _memory;

        public Calculator(int initialMemory)
        {
            _memory = initialMemory;
        }

        public int Add(int a, int b)
        {
            return a + b;
        }

        public int Subtract(int a, int b)
        {
            return a - b;
        }

        public void StoreInMemory(int value)
        {
            _memory = value;
        }

        public int RetrieveMemory()
        {
            return _memory;
        }
    }

    class Program
    {
        static void Main()
        {
            var calc = new Calculator(0);
            Console.WriteLine(calc.Add(5, 7));
            Console.WriteLine(calc.Subtract(10, 3));
        }
    }
}
