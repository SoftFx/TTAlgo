﻿using Caliburn.Micro;
using Machinarium.Qnil;
using NLog;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TickTrader.Algo.Domain;

namespace TickTrader.BotTerminal
{
    internal class LocalPluginSetupViewModel : Screen, IWindowModel
    {
        private static readonly Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private bool _dlgResult;
        private bool _runBot;
        private PluginCatalogItem _selectedPlugin;

        public IAlgoAgent Agent { get; }

        public SetupContextInfo SetupContext { get; }

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

        public PluginConfigViewModel Setup { get; private set; }

        public ITradeBot Bot { get; private set; }

        public bool PluginIsStopped => Bot == null ? true : Bot.State == PluginModelInfo.Types.PluginState.Stopped;

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

        public Metadata.Types.PluginType Type { get; }

        public string PluginType { get; }

        public PluginSetupMode Mode { get; }

        public bool IsNewMode => Mode == PluginSetupMode.New;

        public bool IsEditMode => Mode == PluginSetupMode.Edit;

        public event Action<LocalPluginSetupViewModel, bool> Closed = delegate { };

        private LocalPluginSetupViewModel(LocalAlgoAgent2 agent, PluginKey key, Metadata.Types.PluginType type, SetupContextInfo setupContext, PluginSetupMode mode)
        {
            Agent = agent;
            Mode = mode;
            Type = type;
            SetupContext = setupContext;

            switch (type)
            {
                case Metadata.Types.PluginType.TradeBot:
                    Plugins = Agent.Catalog.BotTraders.AsObservable();
                    break;
                case Metadata.Types.PluginType.Indicator:
                    Plugins = Agent.Catalog.Indicators.AsObservable();
                    break;
                default:
                    Plugins = Agent.Catalog.PluginList.AsObservable();
                    break;
            }

            SelectedPlugin = key != null ? Plugins.FirstOrDefault(i => i.Key.Equals(key)) : Plugins.First();

            PluginType = GetPluginTypeDisplayName(Type);

            RunBot = true;
        }

        public LocalPluginSetupViewModel(LocalAlgoAgent2 agent, PluginKey key, Metadata.Types.PluginType type, SetupContextInfo setupContext)
            : this(agent, key, type, setupContext, PluginSetupMode.New)
        {
            DisplayName = $"Setting New {PluginType}";

            Agent.Catalog.PluginList.Updated += AllPlugins_Updated;
        }

        public LocalPluginSetupViewModel(LocalAlgoAgent2 agent, ITradeBot bot)
            : this(agent, bot.Config.Key, Metadata.Types.PluginType.TradeBot, null, PluginSetupMode.Edit)
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
            TryCloseAsync();
        }

        public void Cancel()
        {
            _dlgResult = false;
            TryCloseAsync();
        }

        public override Task<bool> CanCloseAsync(CancellationToken cancellationToken = default)
        {
            Closed(this, _dlgResult);
            Dispose();

            return base.CanCloseAsync(cancellationToken);
        }

        //public override void CanClose(Action<bool> callback)
        //{
        //    callback(true);
        //    Closed(this, _dlgResult);
        //    Dispose();
        //}

        public PluginConfig GetConfig()
        {
            var res = Setup.Save();
            res.Key = SelectedPlugin.Key;
            return res;
        }


        private void BotStateChanged(ITradeBot bot)
        {
            if (Bot.InstanceId == bot.InstanceId)
            {
                Bot = bot;
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
                    TryCloseAsync();
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

        private string GetPluginTypeDisplayName(Metadata.Types.PluginType type)
        {
            switch (type)
            {
                case Metadata.Types.PluginType.TradeBot: return "Bot";
                case Metadata.Types.PluginType.Indicator: return "Indicator";
                default: return "PluginType";
            }
        }

        private void UpdateSetup()
        {
            if (SelectedPlugin != null)
            {
                var metadata = Agent.GetSetupMetadata(null, SetupContext).Result;

                if (Setup != null)
                    Setup.ValidityChanged -= Validate;
                Setup = new PluginConfigViewModel(SelectedPlugin.Info, metadata, Agent.IdProvider, Mode);
                Init();
                if (Bot != null)
                    Setup.Load(Bot.Config);

                NotifyOfPropertyChange(nameof(Setup));
            }
        }
    }
}
