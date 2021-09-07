using System;
using TickTrader.Algo.Core.Lib;

namespace TickTrader.Algo.Logging
{
    public sealed class ConsoleLoggerAdapter : IAlgoLogger
    {
        private readonly string _loggerName;


        private ConsoleLoggerAdapter(string loggerName)
        {
            _loggerName = loggerName;
        }


        public static IAlgoLogger Create(string componentName) => new ConsoleLoggerAdapter(componentName);


        public void Debug(string msg)
        {
            Console.WriteLine($"{_loggerName} -> {msg}");
        }

        public void Debug(string msgFormat, params object[] msgParams)
        {
            Console.WriteLine($"{_loggerName} -> {string.Format(msgFormat, msgParams)}");
        }

        public void Error(string msg)
        {
            Console.WriteLine($"{_loggerName} -> {msg}");
        }

        public void Error(Exception ex)
        {
            Console.WriteLine($"{_loggerName} -> {ex.Message}");
        }

        public void Error(Exception ex, string msg)
        {
            Console.WriteLine($"{_loggerName} -> {msg} | {ex.Message}");
        }

        public void Error(Exception ex, string msgFormat, params object[] msgParams)
        {
            Console.WriteLine($"{_loggerName} -> {string.Format(msgFormat, msgParams)} | {ex.Message}");
        }

        public void Info(string msg)
        {
            Console.WriteLine($"{_loggerName} -> {msg}");
        }

        public void Info(string msgFormat, params object[] msgParams)
        {
            Console.WriteLine($"{_loggerName} -> {string.Format(msgFormat, msgParams)}");
        }
    }
}
