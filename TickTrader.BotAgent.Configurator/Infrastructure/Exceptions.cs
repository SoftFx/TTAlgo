using System;

namespace TickTrader.BotAgent.Configurator
{
    public class WarningException : Exception
    {
        public WarningException(string message) : base(message)
        {}
    }
}
