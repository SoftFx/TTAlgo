#if NETFRAMEWORK
using System;
using System.Collections.Generic;
using System.Runtime.Remoting;

namespace TickTrader.Algo.Isolation
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
    }
}
#endif
