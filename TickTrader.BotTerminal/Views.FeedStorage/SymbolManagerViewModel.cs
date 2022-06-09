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
using TickTrader.Algo.Domain;
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

            _varContext.TriggerOnChange(FilterString.Var, _ => AllSymbolsView.Refresh());
        }

        protected override Task OnActivateAsync(CancellationToken cancellationToken)
        {
            if (_catalog is not null)
            {
                SubscribeHandlersToCollection(_catalog.OnlineCollection);
                SubscribeHandlersToCollection(_catalog.CustomCollection);
            }

            return base.OnActivateAsync(cancellationToken);
        }

        protected override Task OnDeactivateAsync(bool close, CancellationToken cancellationToken)
        {
            if (close && _catalog is not null)
            {
                UnsubscribeHandlersToCollection(_catalog.OnlineCollection);
                UnsubscribeHandlersToCollection(_catalog.CustomCollection);
            }

            return base.OnDeactivateAsync(close, cancellationToken);
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
            OnUIThread(() => // for disconnect handling
            {
                var symbolViewModel = _allSymbols.FirstOrDefault(u => u.Name == smb.Name);

                if (symbolViewModel != null)
                    _allSymbols.Remove(symbolViewModel);
            });
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
            var smb = key.Origin.IsOnline() ? _catalog.OnlineCollection[key.Symbol] : _catalog.CustomCollection[key.Symbol];

            await _wndManager.ShowDialog(new FeedExportViewModel(smb.Series[key]), this);
        }


        public async void AddSymbol()
        {
            var model = new SymbolCfgEditorViewModel(null, _clientModel.SortedCurrenciesNames, HasSymbol);

            if (_wndManager.ShowDialog(model, this).Result == true)
            {
                var actionModel = new ActionDialogViewModel("Adding symbol...", () => _catalog.CustomCollection.TryAddSymbol(model.GetSymbolInfo()));
                await _wndManager.ShowDialog(actionModel, this);
            }
        }

        public async void EditSymbol(ISymbolData symbol)
        {
            var model = new SymbolCfgEditorViewModel(symbol.Info, _clientModel.SortedCurrenciesNames, HasSymbol);

            if (_wndManager.ShowDialog(model, this).Result == true)
            {
                var actionModel = new ActionDialogViewModel("Saving symbol settings...", () => _catalog.CustomCollection.TryUpdateSymbol(model.GetSymbolInfo()));
                await _wndManager.ShowDialog(actionModel, this);
            }
        }

        public async void CopySymbol(ISymbolData symbol)
        {
            var model = new SymbolCfgEditorViewModel(symbol.Info, _clientModel.SortedCurrenciesNames, HasSymbol, true);

            if (_wndManager.ShowDialog(model, this).Result == true)
            {
                var actionModel = new ActionDialogViewModel("Saving symbol settings...", () => _catalog.CustomCollection.TryAddSymbol(model.GetSymbolInfo()));
                await _wndManager.ShowDialog(actionModel, this);
            }
        }


        public async void RemoveSymbol(ISymbolData symbolModel)
        {
            var actionModel = new ActionDialogViewModel("Removing symbol...", () => _catalog.CustomCollection.TryRemoveSymbol(symbolModel.Name));
            await _wndManager.ShowDialog(actionModel, this);
        }

        public async void RemoveSeries(SeriesViewModel series)
        {
            var actionModel = new ActionDialogViewModel("Removing series...", () => series.Remove());
            await _wndManager.ShowDialog(actionModel, this);
        }


        private bool HasSymbol(string smbName)
        {
            smbName = smbName.Trim();
            return _catalog.OnlineCollection[smbName] != null || _catalog.CustomCollection[smbName] != null;
        }
    }
}