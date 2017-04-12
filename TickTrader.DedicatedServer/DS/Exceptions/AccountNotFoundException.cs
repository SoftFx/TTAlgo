using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TickTrader.DedicatedServer.DS.Exceptions
{
    public class AccountNotFoundException : DSException
    {
        public AccountNotFoundException(string message) : base(message)
        {
            Code = ExceptionCodes.AccountNotFound;
        }
    }
}
