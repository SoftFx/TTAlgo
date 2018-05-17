using Caliburn.Micro;
using Machinarium.Qnil;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Metadata;
using TickTrader.Algo.Core.Repository;
using TickTrader.Algo.Common.Model.Setup;
using TickTrader.Algo.Core;
using TickTrader.Algo.Api;
using TickTrader.Algo.Common.Info;

namespace TickTrader.BotTerminal
{
    internal class SetupPluginViewModel : Screen, IWindowModel
    {
        private Logger _logger;
        private bool _dlgResult;
        private bool _runBot;
        private SetupContextInfo _setupContext;
        private PluginCatalogItem _selectedPlugin;
        private AccountKey _selectedAccount;


        public IAlgoAgent Agent { get; }

        public IObservableList<AccountKey> Accounts { get; }

        public AccountKey SelectedAccount
        {
            get { return _selectedAccount; }
            set
            {
                if (_selectedAccount == value)
                    return;

                _selectedAccount = value;
                NotifyOfPropertyChange(nameof(SelectedAccount));
            }
        }

        public IObservableList<PluginCatalogItem> Plugins { get; }

        public PluginCatalogItem SelectedPlugin
        {
            get { return _selectedPlugin; }
            set
            {
                if (_selectedPlugin == value)
                    return;

                _selectedPlugin = value;
                NotifyOfPropertyChange(nameof(SelectedPlugin));
                UpdateSetup();
            }
        }

        public PluginSetupViewModel Setup { get; private set; }

        public TradeBotModel Bot { get; }

        public bool PluginIsStopped => Bot == null ? true : Bot.State == BotModelStates.Stopped;

        public bool CanOk => Setup.IsValid && PluginIsStopped;

        public bool RunBot
        {
            get { return _runBot; }
            set
            {
                if (_runBot == value)
                    return;

                _runBot = value;
                NotifyOfPropertyChange(nameof(RunBot));
            }
        }

        public string PluginType { get; }


        public event Action<SetupPluginViewModel, bool> Closed = delegate { };


        private SetupPluginViewModel()
        {
            _logger = NLog.LogManager.GetCurrentClassLogger();
            RunBot = true;
        }

        public SetupPluginViewModel(IAlgoAgent agent, PluginKey key, AlgoTypes type, SetupContextInfo setupContext = null) : this()
        {
            Agent = agent;
            _setupContext = setupContext;

            switch (type)
            {
                case AlgoTypes.Robot:
                    Plugins = Agent.Catalog.BotTraders.AsObservable();
                    break;
                case AlgoTypes.Indicator:
                    Plugins = Agent.Catalog.Indicators.AsObservable();
                    break;
                default:
                    Plugins = Agent.Catalog.PluginList.AsObservable();
                    break;
            }

            SelectedPlugin = Plugins.FirstOrDefault(i => i.Key == key);

            PluginType = GetPluginTypeDisplayName(type);

            DisplayName = $"Setting New {PluginType}";

            Agent.Catalog.PluginList.Updated += AllPlugins_Updated;

            Init();
        }

        public SetupPluginViewModel(TradeBotModel bot) : this()
        {
            Bot = bot;
            //Setup = bot.Setup.Clone(PluginSetupMode.Edit) as TradeBotSetupViewModel;
            PluginType = GetPluginTypeDisplayName(Setup.Descriptor.Type);

            DisplayName = $"Settings - {bot.InstanceId}";

            Bot.StateChanged += BotStateChanged;

            Init();
        }


        public void Reset()
        {
            Setup.Reset();
        }

        public void Ok()
        {
            _dlgResult = true;
            TryClose();
        }

        public void Cancel()
        {
            _dlgResult = false;
            TryClose();
        }

        public override void CanClose(Action<bool> callback)
        {
            callback(true);
            Closed(this, _dlgResult);
            Dispose();
        }


        private void BotStateChanged(TradeBotModel obj)
        {
            NotifyOfPropertyChange(nameof(PluginIsStopped));
            NotifyOfPropertyChange(nameof(CanOk));
        }

        private void Init()
        {
            Setup.ValidityChanged += Validate;
            Validate();

            _logger.Debug($"Init {Setup.Descriptor.DisplayName} "
                 + Setup.Parameters.Count() + " params "
                 + Setup.Inputs.Count() + " inputs "
                 + Setup.Outputs.Count() + " outputs ");
        }

        private void Validate()
        {
            NotifyOfPropertyChange(nameof(CanOk));
        }

        private void AllPlugins_Updated(ListUpdateArgs<PluginCatalogItem> args)
        {
            if (args.Action == DLinqAction.Replace)
            {
                if (args.NewItem.Key == SelectedPlugin.Key)
                {
                    SelectedPlugin = args.NewItem;
                    Init();
                }
            }
            else if (args.Action == DLinqAction.Remove)
            {
                if (args.NewItem.Key == SelectedPlugin.Key)
                    TryClose();
            }
        }

        private void Dispose()
        {
            if (Agent.Catalog != null)
                Agent.Catalog.PluginList.Updated -= AllPlugins_Updated;
            if (Bot != null)
                Bot.StateChanged -= BotStateChanged;
            if (Setup != null)
                Setup.ValidityChanged -= Validate;
        }

        private string GetPluginTypeDisplayName(AlgoTypes type)
        {
            switch (type)
            {
                case AlgoTypes.Robot: return "Bot";
                case AlgoTypes.Indicator: return "Indicator";
                default: return "PluginType";
            }
        }

        private async void UpdateSetup()
        {
            if (SelectedPlugin != null)
            {
                var metadata = await Agent.GetSetupMetadata(SelectedAccount, _setupContext);
                Setup = AlgoSetupFactory.CreateSetup(SelectedPlugin.Info, metadata, Agent.IdProvider);
                NotifyOfPropertyChange(nameof(Setup));
            }
        }
    }
}
