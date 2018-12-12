using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace TickTrader.BotTerminal
{
    public class UiUpdateTimer : IDisposable
    {
        private DispatcherTimer _timer;
        private Action _updateAction;

        public UiUpdateTimer(Action updateAction)
        {
            _updateAction = updateAction ?? throw new ArgumentNullException("updateAction");

            _timer = new DispatcherTimer(TimeSpan.FromMilliseconds(500), DispatcherPriority.Input,
                OnTick, Dispatcher.CurrentDispatcher);

            _timer.Start();
        }

        private void OnTick(object sender, EventArgs args)
        {
            _updateAction();
        }

        public void Dispose()
        {
            _timer.Stop();
        }
    }
}
