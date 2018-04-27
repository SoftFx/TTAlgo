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

namespace TickTrader.BotTerminal
{
    internal class SetupPluginViewModel : Screen, IWindowModel
    {
        private Logger _logger;
        private bool _dlgResult;
        private PluginCatalog _catalog;
        private bool _runBot;
        private IAlgoSetupContext _setupContext;


        public PluginSetupViewModel Setup { get; }

        public PluginCatalogItem PluginItem { get; private set; }

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

        public SetupPluginViewModel(AlgoEnvironment algoEnv, PluginCatalogItem item, IAlgoSetupContext setupContext) : this()
        {
            _catalog = algoEnv.Repo;
            PluginItem = item;
            _setupContext = setupContext;

            Setup = AlgoSetupFactory.CreateSetup(item.Ref, algoEnv, setupContext);
            PluginType = GetPluginTypeDisplayName(item.Descriptor);

            DisplayName = $"Setting New {PluginType} - {item.DisplayName}";

            _catalog.AllPlugins.Updated += AllPlugins_Updated;

            Init();
        }

        public SetupPluginViewModel(TradeBotModel bot) : this()
        {
            Bot = bot;
            Setup = bot.Setup.Clone(PluginSetupMode.Edit) as TradeBotSetupViewModel;
            PluginType = GetPluginTypeDisplayName(Setup.Metadata);

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

            _logger.Debug($"Init {Setup.Metadata.DisplayName} "
                 + Setup.Parameters.Count() + " params "
                 + Setup.Inputs.Count() + " inputs "
                 + Setup.Outputs.Count() + " outputs ");
        }

        private void Validate()
        {
            NotifyOfPropertyChange(nameof(CanOk));
        }

        private void AllPlugins_Updated(Machinarium.Qnil.DictionaryUpdateArgs<PluginCatalogKey, PluginCatalogItem> args)
        {
            if (args.Action == DLinqAction.Replace)
            {
                if (args.Key == PluginItem.Key)
                {
                    PluginItem = args.NewItem;
                    Init();
                }
            }
            else if (args.Action == DLinqAction.Remove)
            {
                if (args.Key == PluginItem.Key)
                    TryClose();
            }
        }

        private void Dispose()
        {
            if (_catalog != null)
                _catalog.AllPlugins.Updated -= AllPlugins_Updated;
            if (Bot != null)
                Bot.StateChanged -= BotStateChanged;
            if (Setup != null)
                Setup.ValidityChanged -= Validate;
        }

        private string GetPluginTypeDisplayName(PluginMetadata descriptor)
        {
            switch (descriptor.AlgoLogicType)
            {
                case AlgoTypes.Robot:
                    return "Bot";
                case AlgoTypes.Indicator:
                    return "Indicator";
                default:
                    return "PluginType";
            }
        }
    }
}
