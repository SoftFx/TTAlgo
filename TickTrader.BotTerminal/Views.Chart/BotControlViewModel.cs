using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using TickTrader.Algo.Api;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Metadata;
using Xceed.Wpf.AvalonDock.Layout;

namespace TickTrader.BotTerminal
{
    internal class BotControlViewModel : PropertyChangedBase
    {
        private IShell _shell;
        private BotManagerViewModel _botManager;


        public TradeBotModel Model { get; private set; }

        public bool IsStarted => Model.IsRunning;

        public bool CanBeClosed => Model.State == BotModelStates.Stopped;

        public bool CanStartStop => Model.State == BotModelStates.Running || Model.State == BotModelStates.Stopped;

        public bool CanStart => Model.State == BotModelStates.Stopped;

        public bool CanStop => Model.State == BotModelStates.Running;

        public BotModelStates State => Model.State;

        public bool CanOpenSettings => Model.State == BotModelStates.Stopped;

        public bool CanOpenChart => Model.PluginRef.Descriptor.SetupMainSymbol;


        public BotControlViewModel(TradeBotModel model, IShell shell, BotManagerViewModel botManager, bool runBot, bool openState)
        {
            Model = model;
            _shell = shell;
            _botManager = botManager;

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

        public void Close()
        {
            _botManager.CloseBot(Model.InstanceId);
            Model.StateChanged -= BotStateChanged;
        }

        public void OpenState()
        {

            var stateView = new BotStateViewModel(Model, _shell.ToolWndManager);

            var dock = CustomDockManager.GetInstance();

            var la = new LayoutAnchorable { Title = "New Window", FloatingHeight = 400, FloatingWidth = 500, Content = stateView };
            la.AddToLayout(dock, AnchorableShowStrategy.Right);

            la.Float();
            la.Show();
        }

        public void OpenSettings()
        {
            _botManager.OpenBotSetup(Model);
        }

        public void OpenChart()
        {
            _shell.ShowChart(Model.Setup.MainSymbol.Name, Model.Setup.SelectedTimeFrame.Convert());
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
