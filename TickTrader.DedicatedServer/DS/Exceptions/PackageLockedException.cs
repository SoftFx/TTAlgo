using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TickTrader.DedicatedServer.DS.Exceptions
{
    public class PackageLockedException : DSException
    {
        public PackageLockedException(string message) : base(message)
        {
            Code = ExceptionCodes.PackageNotFound;
        }
    }
}
