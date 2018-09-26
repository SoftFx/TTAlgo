using Caliburn.Micro;
using Machinarium.Qnil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TickTrader.Algo.Common.Info;
using TickTrader.Algo.Common.Model;
using TickTrader.Algo.Common.Model.Config;
using TickTrader.Algo.Common.Model.Setup;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Metadata;
using TickTrader.BotTerminal.Lib;

namespace TickTrader.BotTerminal
{
    internal class BotManagerViewModel : PropertyChangedBase
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();


        private IShell _shell;
        private PreferencesStorageModel _preferences;


        public IVarList<BotControlViewModel> Bots { get; }

        public LocalAlgoAgent Agent => _shell.Agent;

        public TraderClientModel ClientModel => Agent.ClientModel;

        public AlgoEnvironment AlgoEnv { get; }


        private BotManager BotManagerModel => Agent.BotManager;


        public BotManagerViewModel(AlgoEnvironment algoEnv, PersistModel storage)
        {
            AlgoEnv = algoEnv;
            _preferences = storage.PreferencesStorage.StorageModel;
            _shell = algoEnv.Shell;

            Bots = BotManagerModel.Bots.OrderBy((id, bot) => id).Select(b => new BotControlViewModel(b, AlgoEnv, this, false, false));
        }


        public void OpenBotSetup(PluginInfo item, IAlgoSetupContext context = null)
        {
            AlgoEnv.LocalAgentVM.OpenBotSetup(item);
        }

        public void OpenBotSetup(TradeBotModel model)
        {
            AlgoEnv.LocalAgentVM.OpenBotSetup(model.ToInfo());
        }

        public void SaveBotsSnapshot(ProfileStorageModel profileStorage)
        {
            try
            {
                profileStorage.Bots = BotManagerModel.Bots.Snapshot.Values.Select(b => new TradeBotStorageEntry
                {
                    Started = b.State == PluginStates.Running || b.State == PluginStates.Reconnecting,
                    Config = b.Config,
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

        public void CloseBot(string instanceId)
        {
            BotManagerModel.RemoveBot(instanceId);
        }

        public void CloseAllBots(CancellationToken token)
        {
            foreach (var instanceId in BotManagerModel.Bots.Snapshot.Keys.ToList())
            {
                if (token.IsCancellationRequested)
                    return;

                BotManagerModel.RemoveBot(instanceId);
            }
        }


        private void AddBot(PluginConfig config, bool runBot)
        {
            Agent.AddBot(null, config);
            if (runBot)
                Agent.StartBot(config.InstanceId);
        }

        private void UpdateBot(AgentPluginSetupViewModel setupModel)
        {
            BotManagerModel.ChangeBotConfig(setupModel.Bot.InstanceId, setupModel.GetConfig());
        }

        private void BotClosed(BotControlViewModel sender)
        {
            BotManagerModel.RemoveBot(sender.Model.InstanceId);
        }

        private void RestoreTradeBot(TradeBotStorageEntry entry)
        {
            if (entry.Config == null)
            {
                _logger.Error("Trade bot not configured!");
            }
            if (entry.Config.Key == null)
            {
                _logger.Error("Trade bot key missing!");
            }

            AddBot(entry.Config, entry.Started && _preferences.RestartBotsOnStartup);
        }
    }
}
