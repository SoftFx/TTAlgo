using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Core.Lib
{
    public class NoTimeoutByRefObject : MarshalByRefObject
    {
        public override object InitializeLifetimeService()
        {
            // returning null here will prevent the lease manager
            // from deleting the object.
            return null;
        }
    }
}
