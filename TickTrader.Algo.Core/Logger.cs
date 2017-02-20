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
        void Error(Exception ex);
        void Error(string msg, Exception ex);
        void Error(Exception ex, string msgFormat, params object[] msgParams);
    }
}
