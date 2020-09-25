using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
}
