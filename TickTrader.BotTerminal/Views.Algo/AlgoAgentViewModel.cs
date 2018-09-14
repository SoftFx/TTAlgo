using Caliburn.Micro;
using Machinarium.Qnil;
using NLog;
using System;
using System.Threading.Tasks;
using TickTrader.Algo.Common.Info;
using TickTrader.Algo.Common.Model.Config;
using TickTrader.Algo.Common.Model.Setup;
using TickTrader.Algo.Core.Metadata;

namespace TickTrader.BotTerminal
{
    internal class AlgoAgentViewModel : PropertyChangedBase
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


        public AlgoAgentViewModel(IAlgoAgent agentModel, AlgoEnvironment algoEnv)
        {
            _agentModel = agentModel;
            _algoEnv = algoEnv;

            Plugins = _agentModel.Plugins.OrderBy((k, v) => v.Descriptor.UiDisplayName).Select(p => new AlgoPluginViewModel(p, this));
            Packages = _agentModel.Packages.OrderBy((k, v) => k).Select(p => new AlgoPackageViewModel(p, this));
            Bots = _agentModel.Bots.OrderBy((k, v) => k).Select(p => new AlgoBotViewModel(p, this));
            Accounts = _agentModel.Accounts.OrderBy((k, v) => k).Select(p => new AlgoAccountViewModel(p, this));

            PluginList = Plugins.AsObservable();
            PackageList = Packages.AsObservable();
            BotList = Bots.AsObservable();
            AccountList = Accounts.AsObservable();
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

        public async Task RemoveBot(string botId, bool cleanLog = false, bool cleanAlgoData = false)
        {
            try
            {
                await _agentModel.RemoveBot(botId, cleanLog, cleanAlgoData);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Failed to remove bot on {_agentModel.Name}");
            }
        }

        public async Task RemoveAccount(AccountKey account)
        {
            try
            {
                await _agentModel.RemoveAccount(account);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Failed to remove account on {_agentModel.Name}");
            }
        }

        public async Task TestAccount(AccountKey account)
        {
            try
            {
                await _agentModel.TestAccount(account);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Failed to test account on {_agentModel.Name}");
            }
        }

        public async Task RemovePackage(PackageKey package)
        {
            try
            {
                await _agentModel.RemovePackage(package);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Failed to remove package on {_agentModel.Name}");
            }
        }


        public void OpenAccountSetup(AccountModelInfo account)
        {
            try
            {
                var model = new BAAccountDialogViewModel(_agentModel, account);
                _algoEnv.Shell.ToolWndManager.OpenMdiWindow("AccountSetupWindow", model);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to open account setup");
            }
        }

        public void OpenBotSetup(BotModelInfo bot)
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

        public void OpenBotSetup(PluginInfo plugin)
        {
            try
            {
                var model = new AgentPluginSetupViewModel(_algoEnv, Name, null, plugin.Key, plugin.Descriptor.Type, null);
                _algoEnv.Shell.ToolWndManager.OpenMdiWindow("AlgoSetupWindow", model);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to open bot setup");
            }
        }

        public void OpenBotSetup()
        {
            try
            {
                var model = new AgentPluginSetupViewModel(_algoEnv, Name, null, null, AlgoTypes.Robot, null);
                _algoEnv.Shell.ToolWndManager.OpenMdiWindow("AlgoSetupWindow", model);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to open bot setup");
            }
        }

        public void OpenUploadPackageDialog()
        {
            try
            {
                var model = new UploadPackageViewModel(_algoEnv, Name);
                _algoEnv.Shell.ToolWndManager.OpenMdiWindow("AlgoUploadPackageWindow", model);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to open upload package dialog");
            }
        }

        public void OpenUploadPackageDialog(PackageKey packageKey)
        {
            try
            {
                var model = new UploadPackageViewModel(_algoEnv, packageKey, Name);
                _algoEnv.Shell.ToolWndManager.OpenMdiWindow("AlgoUploadPackageWindow", model);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to open upload package dialog");
            }
        }

        public void OpenDownloadPackageDialog()
        {
            try
            {
                var model = new DownloadPackageViewModel(_algoEnv, Name);
                _algoEnv.Shell.ToolWndManager.OpenMdiWindow("AlgoDownloadPackageWindow", model);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to open download package dialog");
            }
        }

        public void OpenDownloadPackageDialog(PackageKey packageKey)
        {
            try
            {
                var model = new DownloadPackageViewModel(_algoEnv, packageKey, Name);
                _algoEnv.Shell.ToolWndManager.OpenMdiWindow("AlgoDownloadPackageWindow", model);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to open download package dialog");
            }
        }
    }
}
