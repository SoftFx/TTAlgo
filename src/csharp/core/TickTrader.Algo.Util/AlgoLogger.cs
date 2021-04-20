using System;

namespace TickTrader.Algo.Util
{
    public interface IAlgoLogger
    {
        void Debug(string msg);
        void Debug(string msgFormat, params object[] msgParams);
        void Info(string msg);
        void Info(string msgFormat, params object[] msgParams);
        void Error(string msg);
        void Error(Exception ex, string msg);
        void Error(Exception ex, string msgFormat, params object[] msgParams);
    }


    public class AlgoLoggerFactory
    {
        private static Func<string, IAlgoLogger> _factoryFunc;


        public static void Init(Func<string, IAlgoLogger> factoryFunc)
        {
            _factoryFunc = factoryFunc;
        }

        public static IAlgoLogger GetLogger(string loggerName)
        {
            return _factoryFunc(loggerName);
        }

        public static IAlgoLogger GetLogger<T>()
        {
            return _factoryFunc(typeof(T).Name);
        }
    }
}
