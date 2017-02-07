using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Core.Lib
{
    public class CrossDomainObject : MarshalByRefObject, IDisposable
    {
        private bool _disposed;

        protected virtual IEnumerable<MarshalByRefObject> NestedCrossDomainObjects
        {
            get { yield break; }
        }

        public override object InitializeLifetimeService()
        {
            // returning null here will prevent the lease manager
            // from deleting the object.
            return null;
        }

        private void Disconnect()
        {
            RemotingServices.Disconnect(this);

            foreach (var nestedObj in NestedCrossDomainObjects)
                RemotingServices.Disconnect(nestedObj);
        }

        public virtual void Dispose()
        {
            GC.SuppressFinalize(this);
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            Disconnect();
            _disposed = true;
        }

        ~CrossDomainObject()
        {
            Dispose(false);
        }
    }
}
