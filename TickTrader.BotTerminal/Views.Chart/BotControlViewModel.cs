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

        public BotControlViewModel(TradeBotModel model, WindowManager wndManager, bool runBot, bool openState)
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

        public bool IsStarted => Model.IsRunning;
        public bool CanBeClosed { get { return Model.State == BotModelStates.Stopped; } }
        public bool CanStartStop { get { return Model.State == BotModelStates.Running || Model.State == BotModelStates.Stopped; } }
        public bool CanStart => Model.State == BotModelStates.Stopped;
        public bool CanStop => Model.State == BotModelStates.Running;
        public BotModelStates State => Model.State;

        public bool CanOpenSettings { get { return Model.State == BotModelStates.Stopped; } }

        public void OpenState()
        {
            wndManager.OpenOrActivateWindow(Model, () => new BotStateViewModel(Model, wndManager));
        }

        public void OpenSettings()
        {
            var key = $"BotSettings {Model.InstanceId}";

            wndManager.OpenOrActivateWindow(key, () =>
            {
                var pSetup = new PluginSetupViewModel(Model);
                pSetup.Closed += PluginSetupViewClosed;
                return pSetup;
            });
        }

        private void PluginSetupViewClosed(PluginSetupViewModel setupVM, bool dialogResult)
        {
            if (dialogResult)
            {
                Model.Configurate(setupVM.Setup);
            }
        }

        private void BotStateChanged(TradeBotModel model)
        {
            NotifyOfPropertyChange(nameof(CanOpenSettings));
            NotifyOfPropertyChange(nameof(CanBeClosed));
            NotifyOfPropertyChange(nameof(CanStartStop));
            NotifyOfPropertyChange(nameof(IsStarted));
            NotifyOfPropertyChange(nameof(CanStart));
            NotifyOfPropertyChange(nameof(CanStop));
            NotifyOfPropertyChange(nameof(State));
        }

        public void Dispose()
        {
            Model.StateChanged -= BotStateChanged;
        }
    }
}
