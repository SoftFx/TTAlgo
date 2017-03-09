using System;

namespace TickTrader.DedicatedServer.DS.Exceptions
{
    public class DSException : ApplicationException
    {
        public DSException() { }

        public DSException(string message) : base(message) { }

        public int Code { get; protected set; }
    }
}
