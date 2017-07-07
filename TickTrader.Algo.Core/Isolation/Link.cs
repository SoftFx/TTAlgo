using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Lib;

namespace TickTrader.Algo.Core
{
    internal class SubDomainLink<T> : CrossDomainObject, ILink<T>
    {
        private OuputProxy _output = new OuputProxy();

        public ILinkOutput<T> Output => _output;

        public override void Dispose()
        {
            base.Dispose();
            _output.Dispose();
        }

        public void Write(T msg)
        {
            throw new NotImplementedException();
        }

        private class OuputProxy : CrossDomainObject, ILinkOutput<T>
        {
            public event Action<T> MsgReceived;
        }
    }

    internal class NoIsolationLinkT<T> : ILink<T>, ILinkOutput<T>
    {
        private event Action<T> _received;

        public ILinkOutput<T> Output => this;

        event Action<T> ILinkOutput<T>.MsgReceived
        {
            add { _received += value; }
            remove { _received -= value; }
        }

        public void Dispose()
        {
        }

        public void Write(T msg)
        {
            _received?.Invoke(msg);
        }
    }
}
