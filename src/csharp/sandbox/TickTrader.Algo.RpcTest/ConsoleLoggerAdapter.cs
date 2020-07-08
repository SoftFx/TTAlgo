using System;
using TickTrader.Algo.Core;

namespace TickTrader.Algo.RpcTest
{
    class ConsoleLoggerAdapter : IAlgoCoreLogger
    {
        private readonly string _name;

        public ConsoleLoggerAdapter(string name)
        {
            _name = name;
        }

        public void Debug(string msg)
        {
            Console.WriteLine($"{_name} DEBUG: {msg}");
        }

        public void Debug(string msgFormat, params object[] msgParams)
        {
            Console.WriteLine($"{_name} DEBUG: {string.Format(msgFormat, msgParams)}");
        }

        public void Error(string msg)
        {
            Console.WriteLine($"{_name} ERROR: {msg}");
        }

        public void Error(Exception ex)
        {
            Console.WriteLine($"{_name} ERROR: {ex}");
        }

        public void Error(string msg, Exception ex)
        {
            Console.WriteLine($"{_name} ERROR: {msg}");
            Console.WriteLine(ex);
        }

        public void Error(Exception ex, string msgFormat, params object[] msgParams)
        {
            Console.WriteLine($"{_name} ERROR: {string.Format(msgFormat, msgParams)}");
            Console.WriteLine(ex);
        }

        public void Info(string msg)
        {
            Console.WriteLine($"{_name} INFO: {msg}");
        }

        public void Info(string msgFormat, params object[] msgParams)
        {
            Console.WriteLine($"{_name} INFO: {string.Format(msgFormat, msgParams)}");
        }
    }
}
