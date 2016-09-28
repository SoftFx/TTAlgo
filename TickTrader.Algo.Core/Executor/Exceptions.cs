using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Core
{
    public class AlgoException : Exception
    {
        public AlgoException(string message) : base(message)
        {
        }
    }

    public class ExecutorException : AlgoException
    {
        public ExecutorException(string msg) : base(msg)
        {
        }
    }
}
