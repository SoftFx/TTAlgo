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
        private IAlgoSetupFactory _setupFactory;
        private PluginIdProvider _idProvider;
        private string _instanceId;
        private TradeBotModel _bot;

        public enum PluginSetupMode
        {
            New,
            Edit
        }

        public bool IsEditMode { get { return Mode == PluginSetupMode.Edit; } }
        public bool IsCreationMode { get { return Mode == PluginSetupMode.New; } }
        public PluginSetupMode Mode { get; private set; }
        public bool IsEnabled { get { return _bot == null ? true : _bot.State == BotModelStates.Stopped; } }
        public PluginSetupModel Setup { get; private set; }
        public PluginCatalogItem PluginItem { get; private set; }
        public bool CanOk { get { return Setup.IsValid && IsInstanceIdValid && IsEnabled; } }
        public bool SetupCanBeSkipped => Setup.IsEmpty && Setup.Descriptor.IsValid && Setup.Descriptor.AlgoLogicType != AlgoTypes.Robot;

        public string InstanceId
        {
            get { return _instanceId; }
            set
            {
                _instanceId = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(IsInstanceIdValid));
                Validate();
            }
        }

        public bool IsInstanceIdValid => Mode == PluginSetupMode.Edit ? true : _idProvider.IsValidPluginId(PluginItem.Descriptor, InstanceId);
        public bool Isolated { get; set; }
        public bool RunBot { get; set; }
        public PluginPermissions Permissions { get; set; }

        public event Action<PluginSetupViewModel, bool> Closed = delegate { };

        public IEnumerable<TimeFrames> AvailableTimeFrames => EnumHelper.AllValues<TimeFrames>();
        public IReadOnlyList<ISymbolInfo> Symbols { get; set; }

        private PluginSetupViewModel()
        {
            _logger = NLog.LogManager.GetCurrentClassLogger();
            Isolated = true;
            RunBot = true;
            Permissions = new PluginPermissions();
        }

        public PluginSetupViewModel(AlgoEnvironment algoEnv, PluginCatalogItem item, IAlgoSetupFactory setupFactory) : this()
        {
            Mode = PluginSetupMode.New;
            DisplayName = $"Setting New Bot - {item.DisplayName}";
            PluginItem = item;
            _setupFactory = setupFactory;
            _catalog = algoEnv.Repo;
            _idProvider = algoEnv.IdProvider;
            Symbols = algoEnv.Symbols;

            _catalog.AllPlugins.Updated += AllPlugins_Updated;

            _instanceId = _idProvider.GeneratePluginId(PluginItem.Descriptor);

            Init();
        }

        public PluginSetupViewModel(TradeBotModel bot) : this()
        {
            _bot = bot;
            Mode = PluginSetupMode.Edit;
            DisplayName = $"Settings - {bot.InstanceId}";
            Setup = bot.Setup.Clone() as TradeBotSetupModel;
            InstanceId = bot.InstanceId;
            Permissions = new PluginPermissions { TradeAllowed = bot.Permissions.TradeAllowed };
            Isolated = bot.Isolated;

            _bot.StateChanged += BotStateChanged;

            Init();
        }

        private void BotStateChanged(TradeBotModel obj)
        {
            NotifyOfPropertyChange(nameof(IsEnabled));
            NotifyOfPropertyChange(nameof(CanOk));
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

        private void Init()
        {
            if (Setup != null)
                Setup.ValidityChanged -= Validate;

            if (Mode == PluginSetupMode.New)
                Setup = _setupFactory.CreateSetup(PluginItem.Ref);

            Setup.ValidityChanged += Validate;
            Validate();

            _logger.Debug($"Init {Setup.Descriptor.DisplayName} "
                 + Setup.Parameters.Count() + " params "
                 + Setup.Inputs.Count() + " inputs "
                 + Setup.Outputs.Count() + " outputs ");

            NotifyOfPropertyChange(nameof(Setup));
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
        }
    }
}
