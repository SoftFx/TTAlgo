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
        void Info(string msg);
        void Error(string msg, Exception ex);
    }
}
