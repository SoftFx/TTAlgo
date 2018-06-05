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
using TickTrader.Algo.Common.Model.Config;
using System.Threading;

namespace TickTrader.BotTerminal
{
    internal class SetupPluginViewModel : Screen, IWindowModel
    {
        private static readonly Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private bool _dlgResult;
        private bool _runBot;
        private PluginCatalogItem _selectedPlugin;
        private AccountKey _selectedAccount;
        private CancellationTokenSource _updateSetupCancelSrc;


        public IAlgoAgent Agent { get; }

        public SetupContextInfo SetupContext { get; }

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

        public BotModelInfo Bot { get; private set; }

        public bool PluginIsStopped => Bot == null ? true : Bot.State == BotStates.Offline;

        public bool CanOk => (Setup?.IsValid ?? false) && PluginIsStopped;

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

        public PluginSetupMode Mode { get; }

        public bool CanChangePlugin => Mode == PluginSetupMode.New;


        public event Action<SetupPluginViewModel, bool> Closed = delegate { };


        private SetupPluginViewModel(IAlgoAgent agent, PluginKey key, AlgoTypes type, PluginSetupMode mode)
        {
            Agent = agent;
            Mode = mode;

            Accounts = Agent.Accounts.AsObservable();
            SelectedAccount = Accounts.FirstOrDefault();

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

            SelectedPlugin = key != null ? Plugins.FirstOrDefault(i => i.Key.Equals(key)) : Plugins.First();

            PluginType = GetPluginTypeDisplayName(type);

            RunBot = true;
        }

        public SetupPluginViewModel(IAlgoAgent agent, PluginKey key, AlgoTypes type, SetupContextInfo setupContext)
            : this(agent, key, type, PluginSetupMode.New)
        {
            SetupContext = setupContext;

            DisplayName = $"Setting New {PluginType}";

            Agent.Catalog.PluginList.Updated += AllPlugins_Updated;
        }

        public SetupPluginViewModel(IAlgoAgent agent, BotModelInfo bot)
            : this(agent, bot.Config.Key, AlgoTypes.Robot, PluginSetupMode.Edit)
        {
            Bot = bot;

            DisplayName = $"Settings - {bot.InstanceId}";

            Agent.BotStateChanged += BotStateChanged;
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

        public PluginConfig GetConfig()
        {
            var res = Setup.Save();
            res.Key = SelectedPlugin.Key;
            return res;
        }


        private void BotStateChanged(BotModelInfo modelInfo)
        {
            if (Bot.InstanceId == modelInfo.InstanceId)
            {
                Bot = modelInfo;
                NotifyOfPropertyChange(nameof(PluginIsStopped));
                NotifyOfPropertyChange(nameof(CanOk));
            }
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
                if (args.NewItem.Key.Equals(SelectedPlugin.Key))
                {
                    SelectedPlugin = args.NewItem;
                }
            }
            else if (args.Action == DLinqAction.Remove)
            {
                if (args.OldItem.Key.Equals(SelectedPlugin.Key))
                    TryClose();
            }
        }

        private void Dispose()
        {
            if (Agent?.Catalog != null)
                Agent.Catalog.PluginList.Updated -= AllPlugins_Updated;
            if (Agent != null)
                Agent.BotStateChanged -= BotStateChanged;
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
                _updateSetupCancelSrc?.Cancel();
                _updateSetupCancelSrc = new CancellationTokenSource();

                var metadata = await Agent.GetSetupMetadata(SelectedAccount, SetupContext);

                if (_updateSetupCancelSrc.IsCancellationRequested)
                    return;

                if (Setup != null)
                    Setup.ValidityChanged -= Validate;
                Setup = AlgoSetupFactory.CreateSetup(SelectedPlugin.Info, metadata, Agent.IdProvider, Mode);
                NotifyOfPropertyChange(nameof(Setup));
                Init();

                if (Bot != null)
                    Setup.Load(Bot.Config);
            }
        }
    }
}
