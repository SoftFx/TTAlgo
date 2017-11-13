using System;

namespace TickTrader.BotAgent.BA.Exceptions
{
    public class BAException : ApplicationException
    {
        public BAException() { }

        public BAException(string message) : base(message) { }

        public int Code { get; protected set; }
    }
}
