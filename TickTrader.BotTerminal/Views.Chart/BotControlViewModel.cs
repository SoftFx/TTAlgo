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
        private IShell _shell;


        public TradeBotModel Model { get; private set; }

        public bool IsStarted => Model.IsRunning;

        public bool CanBeClosed => Model.State == BotModelStates.Stopped;

        public bool CanStartStop => Model.State == BotModelStates.Running || Model.State == BotModelStates.Stopped;

        public bool CanStart => Model.State == BotModelStates.Stopped;

        public bool CanStop => Model.State == BotModelStates.Running;

        public BotModelStates State => Model.State;

        public bool CanOpenSettings => Model.State == BotModelStates.Stopped;

        public bool CanOpenChart => Model.PluginRef.Descriptor.SetupMainSymbol;


        public event Action<BotControlViewModel> Closed = delegate { };


        public BotControlViewModel(TradeBotModel model, IShell shell, bool runBot, bool openState)
        {
            Model = model;
            _shell = shell;

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


        public void Dispose()
        {
            Model.StateChanged -= BotStateChanged;
        }

        public async void StartStop()
        {
            if (Model.State == BotModelStates.Running)
                await Model.Stop();
            else if (Model.State == BotModelStates.Stopped)
                Model.Start();
        }

        public void Close()
        {
            Model.Remove();
            Closed(this);
        }

        public void OpenState()
        {
            _shell.ToolWndManager.OpenOrActivateWindow(Model, () => new BotStateViewModel(Model, _shell.ToolWndManager));
        }

        public void OpenSettings()
        {
            var key = $"BotSettings {Model.InstanceId}";

            _shell.ToolWndManager.OpenOrActivateWindow(key, () =>
            {
                var pSetup = new PluginSetupViewModel(Model);
                pSetup.Closed += PluginSetupViewClosed;
                return pSetup;
            });
        }

        public void OpenChart()
        {
            _shell.ShowChart(Model.Setup.MainSymbol.Name, Model.Setup.SelectedTimeFrame.Convert());
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
    }
}
