using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TickTrader.BotAgent.BA.Exceptions
{
    public class AccountNotFoundException : BAException
    {
        public AccountNotFoundException(string message) : base(message)
        {
            Code = ExceptionCodes.AccountNotFound;
        }
    }
}
