using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Container;
using TickTrader.Algo.Core.Lib;

namespace TickTrader.Algo.Core
{
    internal class SubDomainContext : CrossDomainObject, IIsolationContext
    {
        private AlgoSandbox _activator;

        internal SubDomainContext(AlgoSandbox sandbox)
        {
            _activator = sandbox;
        }

        public T ActivateIsolated<T>() where T : MarshalByRefObject, new()
        {
            return _activator.Activate<T>();
        }

        public ILink<T> CreateInLink<T>()
        {
            throw new NotImplementedException();
        }

        public ILink<T> CreateOutLink<T>()
        {
            throw new NotImplementedException();
        }
    }

    internal class NoIsolation : IIsolationContext
    {
        public T ActivateIsolated<T>() where T : MarshalByRefObject, new()
        {
            return new T();
        }

        public ILink<T> CreateInLink<T>()
        {
            throw new NotImplementedException();
        }

        public ILink<T> CreateOutLink<T>()
        {
            throw new NotImplementedException();
        }
    }
}