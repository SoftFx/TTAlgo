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
        private WindowManager wndManager;

        public BotControlViewModel(TradeBotModel model, ToolWindowsManager wndManager, bool runBot, bool openState)
        {
            this.Model = model;
            this.wndManager = wndManager;

            model.StateChanged += BotStateChanged;

            if (runBot)
            {
                StartStop();
            }
            if (openState)
            {
                OpenState();
            }
        }

        public async void StartStop()
        {
            if (Model.State == BotModelStates.Running)
                await Model.Stop();
            else if (Model.State == BotModelStates.Stopped)
                Model.Start();
        }

        public TradeBotModel Model { get; private set; }

        public void Close()
        {
            Model.Remove();
            Closed(this);
        }

        public event Action<BotControlViewModel> Closed = delegate { };

        public bool IsStarted { get { return Model.State == BotModelStates.Running || Model.State == BotModelStates.Stopping; } }
        public bool CanBeClosed { get { return Model.State == BotModelStates.Stopped; } }
        public bool CanStartStop { get { return Model.State == BotModelStates.Running || Model.State == BotModelStates.Stopped; } }

        public bool CanOpenSettings { get { return Model.State == BotModelStates.Stopped; } }

        public void OpenState()
        {
            var wnd = wndManager.GetWindowModel(Model);
            if (wnd != null)
                wnd.Activate();
            else
                wndManager.OpenWindow(Model, new BotStateViewModel(Model, wndManager));
        }

        public void OpenSettings()
        {
            var key = $"BotSettings {Model.InstanceId}";

            var wnd = wndManager.GetWindow(key);
            if (wnd != null)
            {
                wnd.Activate();
            }
            else
            {
                var pSetup = new PluginSetupViewModel(Model);
                pSetup.Closed += PluginSetupViewClosed;

                wndManager.OpenWindow(key, pSetup);
            }
        }

        private void PluginSetupViewClosed(PluginSetupViewModel setupVM, bool dialogResult)
        {
            if (dialogResult)
            {
                Model.Configurate(setupVM.Setup, setupVM.Permissions, setupVM.Isolated);
            }
        }

        private void BotStateChanged(TradeBotModel model)
        {
            NotifyOfPropertyChange(nameof(CanOpenSettings));
            NotifyOfPropertyChange(nameof(CanBeClosed));
            NotifyOfPropertyChange(nameof(CanStartStop));
            NotifyOfPropertyChange(nameof(IsStarted));
        }

        public void Dispose()
        {
            Model.StateChanged -= BotStateChanged;
        }
    }
}
