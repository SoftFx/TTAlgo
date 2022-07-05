using Machinarium.Qnil;
using NLog;
using System;
using System.Linq;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Domain.ServerControl;

namespace TickTrader.BotTerminal
{
    internal class AlgoAgentViewModel
    {
        private static readonly ILogger _logger = NLog.LogManager.GetCurrentClassLogger();

        private IAlgoAgent _agentModel;
        private AlgoEnvironment _algoEnv;

        public IAlgoAgent Model => _agentModel;

        public string Name => _agentModel.Name;

        public IVarList<AlgoPluginViewModel> Plugins { get; }

        public IVarList<AlgoPackageViewModel> Packages { get; }

        public IVarList<AlgoBotViewModel> Bots { get; }

        public IVarList<AlgoAccountViewModel> Accounts { get; }

        public IObservableList<AlgoPluginViewModel> PluginList { get; }

        public IObservableList<AlgoPackageViewModel> PackageList { get; }

        public IObservableList<AlgoBotViewModel> BotList { get; }

        public IObservableList<AlgoAccountViewModel> AccountList { get; }

        public IObservableList<AccountServerViewModel> AccountServerList { get; }


        public AlgoAgentViewModel(IAlgoAgent agentModel, AlgoEnvironment algoEnv)
        {
            _agentModel = agentModel;
            _algoEnv = algoEnv;

            Plugins = _agentModel.Plugins.OrderBy((k, v) => v.Descriptor_.UiDisplayName).Select(p => new AlgoPluginViewModel(p, this));
            Packages = _agentModel.Packages.OrderBy((k, v) => k).Select(p => new AlgoPackageViewModel(p, this)).DisposeItems();
            Bots = _agentModel.Bots.OrderBy((k, v) => k).Select(p => new AlgoBotViewModel(p, this)).DisposeItems();
            Accounts = _agentModel.Accounts.OrderBy((k, v) => k).Select(p => new AlgoAccountViewModel(p, this)).DisposeItems();

            PluginList = Plugins.AsObservable();
            PackageList = Packages.AsObservable();
            BotList = Bots.AsObservable();
            AccountList = Accounts.AsObservable();
            AccountServerList = _agentModel.Accounts.GroupBy((k, v) => GetServerAddress(v)).OrderBy((k, g) => k).Select(g => g.GroupKey)
                .Select(s => new AccountServerViewModel(s, this)).DisposeItems().AsObservable();
        }


        public async Task StartBot(string botId)
        {
            try
            {
                await _agentModel.StartBot(botId);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Failed to start bot on {_agentModel.Name}");
            }
        }

        public async Task StopBot(string botId)
        {
            try
            {
                await _agentModel.StopBot(botId);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Failed to stop bot on {_agentModel.Name}");
            }
        }

        public async Task AddBot(string accountId, PluginConfig config)
        {
            try
            {
                await _agentModel.AddBot(accountId, config);
                OpenBotState(config.InstanceId);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Failed to add bot on {_agentModel.Name}");
            }
        }

        public async Task RemoveBot(string botId, bool cleanLog = false, bool cleanAlgoData = false, bool showDialog = true)
        {
            try
            {
                if (showDialog)
                {
                    var result = _algoEnv.Shell.ShowDialog(DialogButton.YesNo, DialogMode.Question, DialogMessages.GetRemoveTitle("bot"), DialogMessages.GetRemoveMessage("bot"));

                    if (result != DialogResult.OK)
                        return;
                }

                await _agentModel.RemoveBot(botId, cleanLog, cleanAlgoData);
                _algoEnv.Shell.DockManagerService.RemoveView(ContentIdProvider.Generate(Name, botId));
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Failed to remove bot on {_agentModel.Name}");
            }
        }

        public async Task RemoveAccount(string accountId)
        {
            try
            {
                var result = _algoEnv.Shell.ShowDialog(DialogButton.YesNo, DialogMode.Warning, DialogMessages.GetRemoveTitle("account"), DialogMessages.GetRemoveMessage("account"), DialogMessages.RemoveBotSourceWarning);

                if (result != DialogResult.OK)
                    return;

                await _agentModel.RemoveAccount(new RemoveAccountRequest(accountId));
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Failed to remove account on {_agentModel.Name}");
            }
        }

        public async Task TestAccount(string accountId)
        {
            try
            {
                await _agentModel.TestAccount(new TestAccountRequest(accountId));
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Failed to test account on {_agentModel.Name}");
            }
        }

        public async Task RemovePackage(string packageId)
        {
            try
            {
                var result = _algoEnv.Shell.ShowDialog(DialogButton.YesNo, DialogMode.Warning, DialogMessages.GetRemoveTitle("package"), DialogMessages.GetRemoveMessage("package"), DialogMessages.RemoveBotSourceWarning);

                if (result != DialogResult.OK)
                    return;

                var bots = Bots.Where(u => u.Plugin.PackageId == packageId).AsObservable();

                if (bots.Any(u => u.IsRunning))
                {
                    _algoEnv.Shell.ShowDialog(DialogButton.OK, DialogMode.Error, DialogMessages.FailedTitle, DialogMessages.RemovePackageError);
                    return;
                }

                foreach (var id in bots.Select(u => u.InstanceId).ToList())
                    RemoveBot(id, showDialog: false).Forget();

                await _agentModel.RemovePackage(packageId);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Failed to remove package on {_agentModel.Name}");
            }
        }


        public void OpenAccountSetup(AccountModelInfo account, string serverName = null)
        {
            try
            {
                var model = new BAAccountDialogViewModel(_algoEnv, account, this, serverName);

                _algoEnv.Shell.ToolWndManager.ShowDialog(model, _algoEnv.Shell);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to open account setup");
            }
        }

        public void UpdatePluginAccountSettings(AgentPluginSetupViewModel plugin)
        {
            try
            {
                var model = new BAAccountDialogViewModel(_algoEnv, null, this);

                if (_algoEnv.Shell.ToolWndManager.ShowDialog(model, plugin).Result == true && plugin.SelectedAgent == model.AlgoServer.Value)
                    plugin.SetNewAccount(model.Login.Value);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to open account setup");
            }
        }

        public void OpenBotSetup(ITradeBot bot)
        {
            try
            {
                var key = $"{Name} BotSettings {bot.InstanceId}";
                _algoEnv.Shell.ToolWndManager.OpenOrActivateWindow(key, () => new AgentPluginSetupViewModel(_algoEnv, Name, bot));
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to open bot setup");
            }
        }

        public void OpenBotSetup(PluginInfo plugin, SetupContextInfo setupContext)
        {
            try
            {
                var model = new AgentPluginSetupViewModel(_algoEnv, Name, null, plugin.Key, plugin.Descriptor_.Type, setupContext);
                _algoEnv.Shell.ToolWndManager.OpenMdiWindow("AlgoSetupWindow", model);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to open bot setup");
            }
        }

        public void OpenBotSetup(string accountId = null, PluginKey pluginKey = null)
        {
            try
            {
                var model = new AgentPluginSetupViewModel(_algoEnv, Name, accountId, pluginKey, Metadata.Types.PluginType.TradeBot, null);
                _algoEnv.Shell.ToolWndManager.OpenMdiWindow("AlgoSetupWindow", model);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to open bot setup");
            }
        }

        public void OpenUploadPackageDialog(string packageId = null, AlgoAgentViewModel selectedAlgoServer = null)
        {
            try
            {
                var model = new UploadPackageViewModel(selectedAlgoServer ?? this, packageId);
                _algoEnv.Shell.ToolWndManager.OpenMdiWindow("AlgoUploadPackageWindow", model);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to open upload Algo package dialog");
            }
        }

        public void OpenDownloadPackageDialog(string packageId = null)
        {
            try
            {
                var model = new DownloadPackageViewModel(this, packageId);
                _algoEnv.Shell.ToolWndManager.OpenMdiWindow("AlgoDownloadPackageWindow", model);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to open download Algo package dialog");
            }
        }

        public void OpenManageBotFilesDialog()
        {
            try
            {
                var model = new BotFolderViewModel(_algoEnv, Name);
                _algoEnv.Shell.ToolWndManager.OpenMdiWindow("AlgoManageBotFilesWindow", model);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to open manage bot files dialog");
            }
        }

        public void OpenManageBotFilesDialog(string botId, PluginFolderInfo.Types.PluginFolderId folderId)
        {
            try
            {
                var model = new BotFolderViewModel(_algoEnv, Name, botId, folderId);
                _algoEnv.Shell.ToolWndManager.OpenMdiWindow("AlgoManageBotFilesWindow", model);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to open manage bot files dialog");
            }
        }


        public void ShowChart(ITradeBot bot)
        {
            if (bot.IsRemote)
                return;

            try
            {
                _algoEnv.Shell.ShowChart(bot.Config.MainSymbol.Name, bot.Config.Timeframe.Convert());
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to show chart");
            }
        }

        public void OpenBotState(string botId)
        {
            try
            {
                _algoEnv.Shell.DockManagerService.ShowView(ContentIdProvider.Generate(Name, botId));
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to open bot state");
            }
        }

        public void OpenCopyBotInstanceDialog(string botId)
        {
            try
            {
                var model = new CopyBotInstanceViewModel(_algoEnv, Name, botId);
                _algoEnv.Shell.ToolWndManager.OpenMdiWindow("AlgoCopyBotInstanceWindow", model);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to open copy bot instance dialog");
            }
        }

        public override bool Equals(object obj) => obj is AlgoAgentViewModel second ? Name == second.Name : base.Equals(obj);

        public override int GetHashCode() => Name.GetHashCode();


        private string GetServerAddress(AccountModelInfo acc)
        {
            if (AccountId.TryUnpack(acc.AccountId, out var accId))
                return accId.Server;

            return acc.AccountId;
        }
    }
}
