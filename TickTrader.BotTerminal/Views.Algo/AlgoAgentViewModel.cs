using Caliburn.Micro;
using Machinarium.Qnil;
using NLog;
using System;
using System.Threading.Tasks;
using TickTrader.Algo.Common.Info;
using TickTrader.Algo.Common.Model.Config;
using TickTrader.Algo.Common.Model.Setup;

namespace TickTrader.BotTerminal
{
    internal class AlgoAgentViewModel : PropertyChangedBase
    {
        private static readonly ILogger _logger = NLog.LogManager.GetCurrentClassLogger();

        private IShell _shell;
        private IAlgoAgent _agentModel;


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


        public AlgoAgentViewModel(IShell shell, IAlgoAgent agentModel)
        {
            _shell = shell;
            _agentModel = agentModel;

            Plugins = _agentModel.Plugins.OrderBy((k, v) => k).Select(p => new AlgoPluginViewModel(p, this));
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
                _shell.ToolWndManager.OpenMdiWindow("AccountSetupWindow", model);
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
                var model = new SetupPluginViewModel(_agentModel, bot);
                _shell.ToolWndManager.OpenMdiWindow("AlgoSetupWindow", model);
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
                var model = new SetupPluginViewModel(_agentModel, plugin.Key, plugin.Descriptor.Type, null);
                _shell.ToolWndManager.OpenMdiWindow("AlgoSetupWindow", model);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to open bot setup");
            }
        }
    }
}
