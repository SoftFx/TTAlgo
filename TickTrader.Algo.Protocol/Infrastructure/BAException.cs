using System;

namespace TickTrader.Algo.Protocol
{
    public class BAException : Exception
    {
        public BAException(string message) : base(message) { }
    }
}
