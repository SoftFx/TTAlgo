using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TickTrader.DedicatedServer.DS.Exceptions
{
    public class PackageNotFoundException : DSException
    {
        public PackageNotFoundException(string message) : base(message)
        {
            Code = ExceptionCodes.PackageNotFound;
        }
    }
}
