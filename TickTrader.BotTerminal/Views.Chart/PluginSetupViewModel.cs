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
    internal class PluginSetupViewModel : Screen, IWindowModel
    {
        private Logger _logger;
        private bool _dlgResult;
        private PluginCatalog _catalog;
        private TradeBotModel _bot;
        private bool _runBot;


        public PluginSetupModel Setup { get; }

        public PluginCatalogItem PluginItem { get; private set; }

        public bool PluginIsStopped => _bot == null ? true : _bot.State == BotModelStates.Stopped;

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


        public event Action<PluginSetupViewModel, bool> Closed = delegate { };


        private PluginSetupViewModel()
        {
            _logger = NLog.LogManager.GetCurrentClassLogger();
            RunBot = true;
        }

        public PluginSetupViewModel(AlgoEnvironment algoEnv, PluginCatalogItem item, IAlgoSetupFactory setupFactory) : this()
        {
            _catalog = algoEnv.Repo;
            PluginItem = item;
            Setup = setupFactory.CreateSetup(item.Ref);

            DisplayName = $"Setting New Bot - {item.DisplayName}";

            _catalog.AllPlugins.Updated += AllPlugins_Updated;

            Init();
        }

        public PluginSetupViewModel(TradeBotModel bot) : this()
        {
            _bot = bot;
            Setup = bot.Setup.Clone(PluginSetupMode.Edit) as TradeBotSetupModel;

            DisplayName = $"Settings - {bot.InstanceId}";

            _bot.StateChanged += BotStateChanged;

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
            if (_bot != null)
                _bot.StateChanged -= BotStateChanged;
            if (Setup != null)
                Setup.ValidityChanged -= Validate;
        }
    }
}
