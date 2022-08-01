using Caliburn.Micro;
using Machinarium.Qnil;
using NLog;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Setup;
using TickTrader.Algo.Domain;

namespace TickTrader.BotTerminal
{
    internal class BacktesterPluginSetupViewModel : Screen, IWindowModel
    {
        private static readonly Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private bool _dlgResult;

        public IAlgoAgent Agent { get; }

        public PluginInfo Info { get; private set; }

        public IAlgoSetupMetadata SetupMetadata { get; }

        public SetupContextInfo SetupContext { get; }

        public PluginSetupMode Mode { get; }

        public PluginConfig Config { get; }

        public PluginConfigViewModel Setup { get; private set; }

        public bool PluginIsStopped => true; // Bot == null ? true : Bot.State == BotStates.Offline;

        public bool CanOk => (Setup?.IsValid ?? false) && PluginIsStopped;

        public event Action<BacktesterPluginSetupViewModel, bool> Closed = delegate { };

        private BacktesterPluginSetupViewModel(LocalAlgoAgent2 agent, PluginInfo info, IAlgoSetupMetadata setupMetadata, SetupContextInfo setupContext, PluginSetupMode mode)
        {
            Agent = agent;
            Info = info;
            SetupMetadata = setupMetadata;
            SetupContext = setupContext;
            Mode = mode;

            Agent.Catalog.PluginList.Updated += AllPlugins_Updated;
        }

        public BacktesterPluginSetupViewModel(LocalAlgoAgent2 agent, PluginInfo info, IAlgoSetupMetadata setupMetadata, SetupContextInfo setupContext)
            : this(agent, info, setupMetadata, setupContext, PluginSetupMode.New)
        {
            DisplayName = $"Setting - {info.Descriptor_.DisplayName}";

            UpdateSetup();
        }

        public BacktesterPluginSetupViewModel(LocalAlgoAgent2 agent, PluginInfo info, IAlgoSetupMetadata setupMetadata, SetupContextInfo setupContext, PluginConfig config)
            : this(agent, info, setupMetadata, setupContext, PluginSetupMode.Edit)
        {
            Config = config;

            DisplayName = $"Settings - {Config.InstanceId}";

            UpdateSetup();
        }

        public void Reset()
        {
            Setup.Reset();
        }

        public async void Ok()
        {
            _dlgResult = true;
            await TryCloseAsync();
        }

        public async void Cancel()
        {
            _dlgResult = false;
            await TryCloseAsync();
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
            res.Key = Info.Key;
            return res;
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
                if (args.NewItem.Key.Equals(Info.Key))
                {
                    Info = args.NewItem.Info;

                }
            }
            else if (args.Action == DLinqAction.Remove)
            {
                if (args.OldItem.Key.Equals(Info.Key))
                    TryCloseAsync();
            }
        }

        private void Dispose()
        {
            if (Agent?.Catalog != null)
                Agent.Catalog.PluginList.Updated -= AllPlugins_Updated;
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
            if (Info != null)
            {
                var metadata = GetSetupMetadata();

                if (Setup != null)
                    Setup.ValidityChanged -= Validate;
                Setup = new PluginConfigViewModel(Info, metadata, Agent.IdProvider, Mode)
                {
                    IsFixedFeed = true,
                    IsEmulation = true
                };
                Init();
                if (Config != null)
                    Setup.Load(Config);

                NotifyOfPropertyChange(nameof(Setup));
            }
        }

        private SetupMetadata GetSetupMetadata()
        {
            var agentMetadata = Agent.GetSetupMetadata(null, null).Result;

            var accMetadata = new AccountMetadataInfo { AccountId = AccountId.Pack("backtester", "backtester") };
            accMetadata.Symbols.AddRange(SetupMetadata.Symbols.Select(s => s.ToConfig()));

            return new SetupMetadata(agentMetadata, accMetadata, SetupContext);
        }
    }
}
