using Caliburn.Micro;
using Machinarium.Qnil;
using Machinarium.Var;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Common.Model;
using TickTrader.Algo.Common.Model.Setup;
using TickTrader.Algo.Core.Metadata;

namespace TickTrader.BotTerminal
{
    internal class BacktesterViewModel : Screen, IWindowModel, IAlgoSetupFactory
    {
        private AlgoEnvironment _env;
        private IShell _shell;
        private IVarList<SymbolModel> _availableSymbols;

        public BacktesterViewModel(AlgoEnvironment env, CustomFeedStorage.Handler _customStorage, IShell shell)
        {
            _env = env;
            _shell = shell;

            ProgressMonitor = new ActionViewModel();
            Symbols = new ObservableCollection<BacktesterSymbolSetupViewModel>();

            //_availableSymbols = env.Symbols;

            var mainSymbol = new BacktesterSymbolSetupViewModel(true, null);
            mainSymbol.OnAdd += AddSymbol;
            Symbols.Add(mainSymbol);

            SelectedPlugin = new Property<AlgoItemViewModel>();
            IsPluginSelected = SelectedPlugin.Var.IsNotNull();

            AddSymbol();

            Plugins = env.Repo.AllPlugins
                .Where((k, p) => !string.IsNullOrEmpty(k.FileName))
                .Select((k, p) => new AlgoItemViewModel(p))
                .OrderBy((k, p) => p.Name)
                .AsObservable();

            env.Repo.AllPlugins.Updated += a =>
            {
                if (a.Action == DLinqAction.Remove && a.OldItem.Key == SelectedPlugin.Value?.PluginItem.Key)
                    SelectedPlugin.Value = null;
            };
        }

        public ActionViewModel ProgressMonitor { get; private set; }
        public IObservableList<AlgoItemViewModel> Plugins { get; private set; }
        public Property<AlgoItemViewModel> SelectedPlugin { get; private set; }
        public BoolVar IsPluginSelected { get; private set; }
        public ObservableCollection<BacktesterSymbolSetupViewModel> Symbols { get; private set; }

        public void OpenPluginSetup()
        {
            var setup = new PluginSetupViewModel(_env, SelectedPlugin.Value.PluginItem, this);
            _shell.ToolWndManager.OpenMdiWindow("AlgoSetupWindow", setup);
        }

        private void AddSymbol()
        {
            var smb = new BacktesterSymbolSetupViewModel(false, null);
            smb.Removed += Smb_Removed;

            Symbols.Add(smb);
        }

        PluginSetup IAlgoSetupFactory.CreateSetup(AlgoPluginRef catalogItem)
        {
            return new BarBasedPluginSetup(catalogItem, "", Algo.Api.BarPriceType.Ask, _env);
        }

        private void Smb_Removed(BacktesterSymbolSetupViewModel smb)
        {
            Symbols.Remove(smb);
            smb.Removed -= Smb_Removed;
        }
    }
}
