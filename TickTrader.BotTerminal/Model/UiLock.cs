using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.BotTerminal
{
    internal class UiLock : PropertyChangedBase
    {
        private int lockCount;

        public bool IsLocked { get { return lockCount > 0; } }
        public bool IsNotLocked { get { return lockCount == 0; } }

        public void Lock()
        {
            lockCount++;
            if (lockCount == 1)
            {
                NotifyOfPropertyChange(nameof(IsLocked));
                NotifyOfPropertyChange(nameof(IsNotLocked));
            }
        }

        public void Release()
        {
            lockCount--;
            if (lockCount == 0)
            {
                NotifyOfPropertyChange(nameof(IsLocked));
                NotifyOfPropertyChange(nameof(IsNotLocked));
            }
        }
    }
}
