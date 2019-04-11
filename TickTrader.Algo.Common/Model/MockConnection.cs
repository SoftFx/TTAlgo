using Machinarium.Var;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Common.Lib;

namespace TickTrader.Algo.Common.Model
{
    public class MockConnection : IConnectionStatusInfo
    {
        private BoolProperty _connected = new BoolProperty();

        public bool IsConnecting => false;
        public BoolVar IsConnected => _connected.Var;

        public void EmulateConnect()
        {
            _connected.Set();
            Connected?.Invoke();
        }

        public void EmulateDisconnect()
        {
            _connected.Clear();
            Disconnected?.Invoke();
        }

        public event AsyncEventHandler Initializing { add { } remove { } }
        public event Action IsConnectingChanged { add { } remove { } }
        public event Action Connected;
        public event AsyncEventHandler Deinitializing { add { } remove { } }
        public event Action Disconnected;
    }
}
