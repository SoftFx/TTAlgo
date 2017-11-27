using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TickTrader.BotAgent.BA.Exceptions
{
    public class DuplicateBotIdException : BAException
    {
        public DuplicateBotIdException(string message) : base(message)
        {
            Code = ExceptionCodes.DuplicateBot;
        }
    }
}
