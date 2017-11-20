using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TickTrader.BotAgent.BA.Exceptions
{
    public class PackageNotFoundException : BAException
    {
        public PackageNotFoundException(string message) : base(message)
        {
            Code = ExceptionCodes.PackageNotFound;
        }
    }
}
