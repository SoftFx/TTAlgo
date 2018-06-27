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
        private AlgoEnvironment _algoEnv;
        private BotManagerViewModel _botManager;


        public TradeBotModel Model { get; private set; }

        public bool IsStarted => Model.IsRunning;

        public bool CanBeClosed => Model.State == BotModelStates.Stopped;

        public bool CanStartStop => Model.State == BotModelStates.Running || Model.State == BotModelStates.Stopped;

        public bool CanStart => Model.State == BotModelStates.Stopped;

        public bool CanStop => Model.State == BotModelStates.Running;

        public BotModelStates State => Model.State;

        public bool CanOpenSettings => Model.State == BotModelStates.Stopped;

        public bool CanOpenChart => Model.PluginRef?.Metadata.Descriptor.SetupMainSymbol ?? false;


        public BotControlViewModel(TradeBotModel model, AlgoEnvironment algoEnv, BotManagerViewModel botManager, bool runBot, bool openState)
        {
            Model = model;
            _algoEnv = algoEnv;
            _botManager = botManager;

            model.StateChanged += BotStateChanged;

            if (runBot)
            {
                StartStop();
            }

            CreateState();
            if(openState)
                OpenState();
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
            CustomDockManager.GetInstance().RemoveView(Model.InstanceId);
            Model.StateChanged -= BotStateChanged;
        }

        public void CreateState()
        {
            var botId = Model.InstanceId;
            var manager = CustomDockManager.GetInstance();
            if (!manager.HasView(botId))
            {

                var content = new BotStateViewModel(Model, _algoEnv);
                var view = new LayoutAnchorable { Title = botId, FloatingHeight = 300, FloatingWidth = 300, Content = content, ContentId = botId };
                manager.AddView(botId, view);
                manager.ShowView(botId);
            }
        }


        public void OpenState()
        {
            var manager = CustomDockManager.GetInstance();
            if (!manager.HasView(Model.InstanceId))
                CreateState();                
            manager.ShowView(Model.InstanceId);
        }

        public void OpenSettings()
        {
            _botManager.OpenBotSetup(Model);
        }

        public void OpenChart()
        {
            _algoEnv.Shell.ShowChart(Model.Config.MainSymbol, Model.Config.TimeFrame.Convert());
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
