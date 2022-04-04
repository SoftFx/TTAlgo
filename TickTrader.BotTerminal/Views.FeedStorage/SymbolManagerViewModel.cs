using Caliburn.Micro;
using Machinarium.Qnil;
using Machinarium.Var;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;
using TickTrader.Algo.Core.Lib;
using TickTrader.FeedStorage.Api;


namespace TickTrader.BotTerminal.SymbolManager
{
    internal sealed class SymbolManagerViewModel : Screen, IWindowModel
    {
        private readonly VarContext _varContext = new VarContext();

        private readonly ObservableCollection<SymbolViewModel> _allSymbols;

        private readonly TraderClientModel _clientModel;
        private readonly WindowManager _wndManager;
        private readonly ISymbolCatalog _catalog;


        public ICollectionView AllSymbolsView { get; }

        public Property<CollectionViewGroup> SelectedGroup { get; }

        public Property<SymbolViewModel> SelectedSymbol { get; }

        public Property<string> FilterString { get; }


        public SymbolManagerViewModel(TraderClientModel clientModel, ISymbolCatalog catalog, WindowManager wndManager)
        {
            _allSymbols = new ObservableCollection<SymbolViewModel>(catalog.AllSymbols.Select(u => new SymbolViewModel(this, u)));

            _clientModel = clientModel;
            _wndManager = wndManager;
            _catalog = catalog;

            DisplayName = "Symbol Manager";

            FilterString = _varContext.AddProperty(string.Empty);
            SelectedSymbol = _varContext.AddProperty<SymbolViewModel>();
            SelectedGroup = _varContext.AddProperty<CollectionViewGroup>();

            AllSymbolsView = CollectionViewSource.GetDefaultView(_allSymbols);

            AllSymbolsView.SortDescriptions.Add(new SortDescription(SymbolViewModel.SecurityHeader, ListSortDirection.Ascending));

            AllSymbolsView.GroupDescriptions.Add(new PropertyGroupDescription(SymbolViewModel.TypeHeader));
            AllSymbolsView.GroupDescriptions.Add(new PropertyGroupDescription(SymbolViewModel.SecurityHeader));
            AllSymbolsView.Filter = FilterGroup;

            SubscribeHandlersToCollection(_catalog.OnlineCollection);
            SubscribeHandlersToCollection(_catalog.CustomCollection);

            _varContext.TriggerOnChange(FilterString.Var, _ => AllSymbolsView.Refresh());
        }


        private void SubscribeHandlersToCollection(ISymbolCollection collection)
        {
            collection.SymbolAdded += AddSymbolHandler;
            collection.SymbolRemoved += RemoveSymbolHandler;
            collection.SymbolUpdated += UpdateSymbolHandler;
        }

        private void UnsubscribeHandlersToCollection(ISymbolCollection collection)
        {
            collection.SymbolAdded -= AddSymbolHandler;
            collection.SymbolRemoved -= RemoveSymbolHandler;
            collection.SymbolUpdated -= UpdateSymbolHandler;
        }


        private void AddSymbolHandler(ISymbolData smb) => _allSymbols.Add(new SymbolViewModel(this, smb));

        private void RemoveSymbolHandler(ISymbolData smb)
        {
            var symbolViewModel = _allSymbols.FirstOrDefault(u => u.Name == smb.Name);

            if (symbolViewModel != null)
                _allSymbols.Remove(symbolViewModel);
        }

        private void UpdateSymbolHandler(ISymbolData oldSymbol, ISymbolData newSymbol)
        {
            RemoveSymbolHandler(oldSymbol);
            AddSymbolHandler(newSymbol);
        }

        private bool FilterGroup(object obj)
        {
            return !(obj is SymbolViewModel smb) || smb.Name.IndexOf(FilterString.Value, StringComparison.OrdinalIgnoreCase) >= 0;
        }


        public void Download() => Download(SelectedSymbol.Value?.Model ?? _allSymbols.FirstOrDefault()?.Model);

        public void Import() => Import(SelectedSymbol.Value?.Model ?? _allSymbols.FirstOrDefault()?.Model);


        public async void Download(ISymbolData symbol)
        {
            await _wndManager.ShowDialog(new FeedDownloadViewModel(_catalog, symbol, this), this);
        }

        public async void Import(ISymbolData symbol)
        {
            await _wndManager.ShowDialog(new FeedImportViewModel(_catalog, symbol), this);
        }

        public async void Export(ISeriesKey key)
        {
            ISymbolData smb = _catalog.OnlineCollection[key.Symbol]; //TO DO: change after include origin in ISeriesKey

            if (smb == null || !smb.Series.ContainsKey(key))
                smb = _catalog.CustomCollection[key.Symbol];

            await _wndManager.ShowDialog(new FeedExportViewModel(smb.Series[key]), this);
        }


        public void AddSymbol()
        {
            var model = new SymbolCfgEditorViewModel(null, _clientModel.SortedCurrenciesNames, HasSymbol);

            if (_wndManager.ShowDialog(model, this).Result == true)
            {
                var actionModel = new ActionDialogViewModel("Adding symbol...", () => _catalog.CustomCollection.TryAddSymbol(model));
                _wndManager.ShowDialog(actionModel, this);
            }
        }

        public void EditSymbol(ISymbolData symbol)
        {
            var model = new SymbolCfgEditorViewModel(symbol.Info, _clientModel.SortedCurrenciesNames, HasSymbol);

            if (_wndManager.ShowDialog(model, this).Result == true)
            {
                var actionModel = new ActionDialogViewModel("Saving symbol settings...", () => _catalog.CustomCollection.TryUpdateSymbol(model));
                _wndManager.ShowDialog(actionModel, this);
            }
        }

        public void CopySymbol(ISymbolData symbol)
        {
            var model = new SymbolCfgEditorViewModel(symbol.Info, _clientModel.SortedCurrenciesNames, HasSymbol, true);

            if (_wndManager.ShowDialog(model, this).Result == true)
            {
                var actionModel = new ActionDialogViewModel("Saving symbol settings...", () => _catalog.CustomCollection.TryAddSymbol(model));
                _wndManager.ShowDialog(actionModel, this);
            }
        }


        public void RemoveSymbol(ISymbolData symbolModel)
        {
            var actionModel = new ActionDialogViewModel("Removing symbol...", () => _catalog.CustomCollection.TryRemoveSymbol(symbolModel.Name));
            _wndManager.ShowDialog(actionModel, this);
        }

        public void RemoveSeries(SeriesViewModel series)
        {
            var actionModel = new ActionDialogViewModel("Removing series...", () => series.Remove());
            _wndManager.ShowDialog(actionModel, this);
        }


        private bool HasSymbol(string smbName)
        {
            smbName = smbName.Trim();
            return _catalog.OnlineCollection[smbName] != null && _catalog.CustomCollection[smbName] != null;
        }

        public override Task<bool> CanCloseAsync(CancellationToken cancellationToken = default)
        {
            UnsubscribeHandlersToCollection(_catalog.OnlineCollection);
            UnsubscribeHandlersToCollection(_catalog.CustomCollection);

            return base.CanCloseAsync(cancellationToken);
        }
    }
}