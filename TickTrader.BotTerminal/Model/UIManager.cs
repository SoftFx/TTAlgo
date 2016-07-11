using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.BotTerminal
{
    internal class UI
    {
        private static UI instance = new UI();

        private UI() { }

        public static UI Instance { get { return instance; } }
        public bool Locked { get; private set; }

        public void Lock()
        {
            Locked = true;
            StateChanged();
        }

        public void Release()
        {
            Locked = false;
            StateChanged();
        }

        public event Action StateChanged = delegate { };
    }
}
