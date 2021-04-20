using System;
using TickTrader.Algo.Util;

namespace TickTrader.Algo.Core
{
    public interface IAlgoCoreLogger
    {
        void Debug(string msg);
        void Debug(string msgFormat, params object[] msgParams);
        void Info(string msg);
        void Info(string msgFormat, params object[] msgParams);
        void Error(string msg);
        void Error(Exception ex);
        void Error(string msg, Exception ex);
        void Error(Exception ex, string msgFormat, params object[] msgParams);
    }

    public class CoreLoggerFactory
    {
        private static Func<string, IAlgoCoreLogger> _factoryFunc;

        public static IAlgoCoreLogger GetLogger(string className)
        {
            return _factoryFunc(className);
        }

        public static IAlgoCoreLogger GetLogger<T>()
        {
            return _factoryFunc(typeof(T).Name);
        }

        public static IAlgoCoreLogger GetLogger<T>(int loggerId)
        {
            return _factoryFunc($"{typeof(T).Name} {loggerId}");
        }

        public static void Init(Func<string, IAlgoCoreLogger> factoryFunc)
        {
            _factoryFunc = factoryFunc;
        }
    }

    public class DebugLogger : IAlgoCoreLogger, IAlgoLogger
    {
        private readonly string _loggerName;


        public DebugLogger(string loggerName)
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

        public void Error(string msg, Exception ex)
        {
            Error(ex, msg);
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
