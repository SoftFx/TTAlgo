using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Metadata;

namespace TickTrader.BotTerminal
{
    internal class BotControlViewModel : PropertyChangedBase
    {
        private ToolWindowsManager wndManager;

        public BotControlViewModel(TradeBotModel2 model, ToolWindowsManager wndManager, bool runBot)
        {
            this.Model = model;
            this.wndManager = wndManager;

            model.StateChanged += Model_StateChanged;

            if (runBot)
            {
                StartStop();
            }
            OpenState();
        }

        public async void StartStop()
        {
            if (Model.State == BotModelStates.Running)
                await Model.Stop();
            else if (Model.State == BotModelStates.Stopped)
                Model.Start();
        }

        public TradeBotModel2 Model { get; private set; }

        public void Close()
        {
            Model.Remove();
            Closed(this);
        }

        public event Action<BotControlViewModel> Closed = delegate { };

        public bool IsStarted { get { return Model.State == BotModelStates.Running || Model.State == BotModelStates.Stopping; } }
        public bool CanBeClosed { get { return Model.State == BotModelStates.Stopped; } }
        public bool CanStartStop { get { return Model.State == BotModelStates.Running || Model.State == BotModelStates.Stopped; } }

        public void OpenState()
        {
            var wnd = wndManager.GetWindow(Model);
            if (wnd != null)
                wnd.Activate();
            else
                wndManager.OpenWindow(Model, new BotStateViewModel(Model));
        }

        private void Model_StateChanged(TradeBotModel2 model)
        {
            NotifyOfPropertyChange(nameof(CanBeClosed));
            NotifyOfPropertyChange(nameof(CanStartStop));
            NotifyOfPropertyChange(nameof(IsStarted));
        }

        public void Dispose()
        {
            Model.StateChanged -= Model_StateChanged;
        }
    }
}
