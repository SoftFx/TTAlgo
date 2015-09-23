using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Lib;

namespace TickTrader.Algo.Samples
{
    class Program
    {
        static void Main(string[] args)
        {
            List<int> list = new List<int>(1);
            list.Add(1);
            list.Add(2);
            list.Add(3);
            list.Add(4);
            list.Add(5);

            ListTester tester = new ListTester();

            tester.Enqueue(1);
            tester.Enqueue(2);
            tester.Enqueue(3);
            tester.Enqueue(4);
            tester.Dequeue();
            tester.Dequeue();
            tester.Enqueue(5);
            tester.Enqueue(6);
            tester.Enqueue(7);
            tester.Dequeue();
            tester.Dequeue();
            tester.Dequeue();
            tester.Dequeue();
            tester.Dequeue();
            tester.Enqueue(8);
            tester.Enqueue(9);
            
            tester.Dequeue();
            tester.Dequeue();

            Console.Read();

            //SmaIndicator indicator = new SmaIndicator();
        }

        
    }

    internal class ListTester
    {
        private CircularList<int> list = new CircularList<int>();

        public void Enqueue(int item)
        {
            Console.WriteLine("Enqueue " + item);
            list.Enqueue(item);
            printEnumarable();
            printList();
        }

        public void Dequeue()
        {
            Console.WriteLine("Dequeue " + list.Dequeue());
            printEnumarable();
            printList();
        }

        private void printEnumarable()
        {
            foreach (int item in list)
                Console.Write(item + " ");
            Console.WriteLine();
        }

        private void printList()
        {
            for (int i = 0; i < list.Count; i++)
                Console.Write(list[i] + " ");
            Console.WriteLine();
        }
    }
}
