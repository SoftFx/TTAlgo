using Caliburn.Micro;
using Machinarium.Qnil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TickTrader.Algo.Common.Model;
using TickTrader.Algo.Common.Model.Setup;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Metadata;
using TickTrader.BotTerminal.Lib;

namespace TickTrader.BotTerminal
{
    internal class BotManagerViewModel : PropertyChangedBase, IAlgoPluginHost
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();


        private BotManager _botManagerModel;
        private IShell _shell;
        private PreferencesStorageModel _preferences;


        public IVarList<BotControlViewModel> Bots { get; }

        public TraderClientModel ClientModel { get; }

        public AlgoEnvironment AlgoEnv { get; }


        public BotManagerViewModel(BotManager botManagerModel, IShell shell, TraderClientModel clientModel, AlgoEnvironment algoEnv, PersistModel storage)
        {
            _botManagerModel = botManagerModel;
            _shell = shell;
            ClientModel = clientModel;
            AlgoEnv = algoEnv;
            _preferences = storage.PreferencesStorage.StorageModel;

            Bots = botManagerModel.Bots.OrderBy((id, bot) => id).Select(b => new BotControlViewModel(b, _shell, this, false, false));
            Bots.Updated += BotsOnUpdated;

            ClientModel.Connected += ClientModelOnConnected;
        }


        public void OpenBotSetup(PluginCatalogItem item, IAlgoSetupContext context = null)
        {
            try
            {
                var model = new PluginSetupViewModel(AlgoEnv, item, context ?? _botManagerModel);
                _shell.ToolWndManager.OpenMdiWindow("AlgoSetupWindow", model);
                model.Closed += AlgoSetupClosed;
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
        }

        public void OpenBotSetup(TradeBotModel model)
        {
            try
            {
                var key = $"BotSettings {model.InstanceId}";

                _shell.ToolWndManager.OpenOrActivateWindow(key, () =>
                {
                    var pSetup = new PluginSetupViewModel(model);
                    pSetup.Closed += AlgoSetupClosed;
                    return pSetup;
                });
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
        }

        public void SaveBotsSnapshot(ProfileStorageModel profileStorage)
        {
            try
            {
                profileStorage.Bots = _botManagerModel.Bots.Snapshot.Values.Select(b => new TradeBotStorageEntry
                {
                    DescriptorId = b.Setup.Descriptor.Id,
                    PluginFilePath = b.PluginFilePath,
                    Started = b.State == BotModelStates.Running,
                    Config = b.Setup.Save(),
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to save bots snapshot");
            }
        }

        public void LoadBotsSnapshot(ProfileStorageModel profileStorage, CancellationToken token)
        {
            try
            {
                if ((profileStorage.Bots?.Count ?? 0) == 0)
                {
                    _logger.Info($"Bots snapshot is empty");
                    return;
                }

                _logger.Info($"Loading bots snapshot({profileStorage.Bots.Count} bots)");

                foreach (var bot in profileStorage.Bots)
                {
                    if (token.IsCancellationRequested)
                    {
                        return;
                    }
                    RestoreTradeBot(bot);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to load bots snapshot");
            }
        }

        public void CloseAllBots(CancellationToken token)
        {
            foreach (var instanceId in _botManagerModel.Bots.Snapshot.Keys.ToList())
            {
                if (token.IsCancellationRequested)
                    return;

                _botManagerModel.RemoveBot(instanceId);
            }
        }


        private void ClientModelOnConnected()
        {
            Connected?.Invoke();
        }

        private void AlgoSetupClosed(PluginSetupViewModel setupModel, bool dlgResult)
        {
            setupModel.Closed -= AlgoSetupClosed;
            if (dlgResult)
            {
                if (setupModel.Setup.IsEditMode)
                {
                    UpdateBot(setupModel);
                }
                else
                {
                    AddBot(setupModel);
                }
            }
        }

        private void AddBot(PluginSetupViewModel setupModel)
        {
            var bot = new TradeBotModel(setupModel, this, new WindowStorageModel { Width = 300, Height = 300 });
            _botManagerModel.AddBot(bot);
            if (setupModel.RunBot)
                bot.Start();
        }

        private void UpdateBot(PluginSetupViewModel setupModel)
        {
            setupModel.Bot.Configurate(setupModel.Setup);
            _botManagerModel.UpdateBot(setupModel.Bot);
        }

        private void BotClosed(BotControlViewModel sender)
        {
            _botManagerModel.RemoveBot(sender.Model.InstanceId);
        }

        private void BotsOnUpdated(ListUpdateArgs<BotControlViewModel> args)
        {
            if (args.Action == DLinqAction.Insert)
            {
                args.NewItem.Closed += BotClosed;
            }
            if (args.Action == DLinqAction.Remove)
            {
                args.OldItem.Closed -= BotClosed;
                args.OldItem.Dispose();
            }
        }

        private void RestoreTradeBot(TradeBotStorageEntry entry)
        {
            var catalogItem = AlgoEnv.Repo.AllPlugins.Where((k, i) => i.CanBeUseForSnapshot(entry)).Snapshot.FirstOrDefault().Value;

            if (catalogItem == null)
            {
                _logger.Error($"Trade bot '{entry.DescriptorId}' from {entry.PluginFilePath} not found!");
                return;
            }
            if (catalogItem.Descriptor.AlgoLogicType != AlgoTypes.Robot)
            {
                _logger.Error($"Plugin '{entry.DescriptorId}' from {entry.PluginFilePath} is not a trade bot!");
                return;
            }

            var setupModel = new PluginSetupViewModel(AlgoEnv, catalogItem, _botManagerModel);
            setupModel.RunBot = entry.Started && _preferences.RestartBotsOnStartup;
            if (entry.Config != null)
            {
                setupModel.Setup.Load(entry.Config);
            }
            AddBot(setupModel);
        }


        #region IAlgoPluginHost

        void IAlgoPluginHost.Lock()
        {
            _shell.ConnectionLock.Lock();
        }

        void IAlgoPluginHost.Unlock()
        {
            _shell.ConnectionLock.Release();
        }

        ITradeExecutor IAlgoPluginHost.GetTradeApi()
        {
            return ClientModel.TradeApi;
        }

        ITradeHistoryProvider IAlgoPluginHost.GetTradeHistoryApi()
        {
            return ClientModel.TradeHistory.AlgoAdapter;
        }

        BotJournal IAlgoPluginHost.Journal => AlgoEnv.BotJournal;

        public virtual void InitializePlugin(PluginExecutor plugin)
        {
            plugin.InvokeStrategy = new PriorityInvokeStartegy();
            plugin.AccInfoProvider = new PluginTradeInfoProvider(ClientModel.Cache, new DispatcherSync());
            var feedProvider = new PluginFeedProvider(ClientModel.Cache, ClientModel.Distributor, ClientModel.FeedHistory, new DispatcherSync());
            plugin.Metadata = feedProvider;
            switch (plugin.TimeFrame)
            {
                case Algo.Api.TimeFrames.Ticks:
                    plugin.InitQuoteStrategy(feedProvider);
                    break;
                default:
                    plugin.InitBarStrategy(feedProvider, Algo.Api.BarPriceType.Bid);
                    break;
            }
            plugin.InitSlidingBuffering(1024);
        }

        public virtual void UpdatePlugin(PluginExecutor plugin)
        {
        }

        bool IAlgoPluginHost.IsStarted => false;

        public event System.Action ParamsChanged = delegate { };
        public event System.Action StartEvent = delegate { };
        public event AsyncEventHandler StopEvent = delegate { return CompletedTask.Default; };
        public event System.Action Connected;

        #endregion
    }
}
