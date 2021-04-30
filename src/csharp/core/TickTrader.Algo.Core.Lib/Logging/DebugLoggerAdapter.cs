using System;

namespace TickTrader.Algo.Core.Lib
{
    internal class DebugLoggerAdapter : IAlgoLogger
    {
        private readonly string _loggerName;


        public DebugLoggerAdapter(string loggerName)
        {
            _loggerName = loggerName;
        }

        public void Debug(string msg)
        {
            System.Diagnostics.Debug.WriteLine($"{_loggerName} -> {msg}");
        }

        public void Debug(string msgFormat, params object[] msgParams)
        {
            System.Diagnostics.Debug.WriteLine($"{_loggerName} -> {string.Format(msgFormat, msgParams)}");
        }

        public void Error(string msg)
        {
            System.Diagnostics.Debug.WriteLine($"{_loggerName} -> {msg}");
        }

        public void Error(Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"{_loggerName} -> {ex.Message}");
        }

        public void Error(Exception ex, string msg)
        {
            System.Diagnostics.Debug.WriteLine($"{_loggerName} -> {msg} | {ex.Message}");
        }

        public void Error(Exception ex, string msgFormat, params object[] msgParams)
        {
            System.Diagnostics.Debug.WriteLine($"{_loggerName} -> {string.Format(msgFormat, msgParams)} | {ex.Message}");
        }

        public void Info(string msg)
        {
            System.Diagnostics.Debug.WriteLine($"{_loggerName} -> {msg}");
        }

        public void Info(string msgFormat, params object[] msgParams)
        {
            System.Diagnostics.Debug.WriteLine($"{_loggerName} -> {string.Format(msgFormat, msgParams)}");
        }
    }
}
