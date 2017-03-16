using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TickTrader.DedicatedServer.DS
{
    public class ModelException : Exception
    {
        public ModelException(string msg) : base(msg)
        {
        }
    }

    public class InvalidStateException : ModelException
    {
        public InvalidStateException(string msg) : base(msg)
        {
        }
    }
}
