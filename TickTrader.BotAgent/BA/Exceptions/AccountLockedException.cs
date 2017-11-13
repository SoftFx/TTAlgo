using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TickTrader.BotAgent.BA.Exceptions
{
    public class AccountLockedException : BAException
    {
        public AccountLockedException(string message) : base(message)
        {
            Code = ExceptionCodes.AccountIsLocked;
        }
    }
}
