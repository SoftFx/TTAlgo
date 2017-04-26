using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TickTrader.DedicatedServer.DS.Exceptions
{
    public class DuplicateBotIdException : DSException
    {
        public DuplicateBotIdException(string message) : base(message)
        {
            Code = ExceptionCodes.DuplicateBot;
        }
    }
}
