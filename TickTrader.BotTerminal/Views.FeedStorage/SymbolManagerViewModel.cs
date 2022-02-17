using Caliburn.Micro;
using Machinarium.Qnil;
using Machinarium.Var;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
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

            FilterString = _varContext.AddProperty<string>(string.Empty);
            SelectedSymbol = _varContext.AddProperty<SymbolViewModel>();
            SelectedGroup = _varContext.AddProperty<CollectionViewGroup>();

            AllSymbolsView = CollectionViewSource.GetDefaultView(_allSymbols);

            AllSymbolsView.GroupDescriptions.Add(new PropertyGroupDescription(SymbolViewModel.TypeHeader));
            AllSymbolsView.GroupDescriptions.Add(new PropertyGroupDescription(SymbolViewModel.SecurityHeader));
            AllSymbolsView.Filter = FilterGroup;

            _varContext.TriggerOnChange(FilterString.Var, _ => AllSymbolsView.Refresh());

            //_varContext.TriggerOn(clientModel.IsConnected, () =>
            //{
            //    _onlineSymbols.Clear();
            //    foreach (var i in clientModel.Symbols.Snapshot)
            //        _onlineSymbols.Add(i.Key, i.Value);
            //});
        }

        private bool FilterGroup(object obj)
        {
            return !(obj is SymbolViewModel smb) || string.Compare(smb.Name, FilterString.Value.Trim(), true, CultureInfo.InvariantCulture) >= 0;
        }

        public void UpdateSizes()
        {
            DoUpdateSizes();
        }

        public void Download()
        {
            Download(SelectedSymbol.Value?.Model);
        }

        public void Download(ISymbolData symbol)
        {
            _wndManager.ShowDialog(new FeedDownloadViewModel(_clientModel, _catalog, symbol), this);
        }

        public void Import()
        {
            Import(SelectedSymbol.Value?.Model);
        }

        public void Import(ISymbolData symbol)
        {
            //using (var symbolList = _customManagedSymbols.TransformToList())
            _wndManager.ShowDialog(new FeedImportViewModel(_catalog, symbol), this);
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

        public void Export(SeriesViewModel series)
        {
            //_wndManager.ShowDialog(new FeedExportViewModel(series.Model), this);
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

        private async void DoUpdateSizes()
        {
            var toUpdate = new List<SymbolViewModel>();
            //toUpdate.AddRange(_onlineManagedSymbols);
            //toUpdate.AddRange(_customManagedSymbols);

            foreach (var item in toUpdate)
                item.ResetSize();

            foreach (var item in toUpdate)
                await item.UpdateSize();
        }

        private bool HasSymbol(string smbName)
        {
            smbName = smbName.Trim();
            return _catalog.OnlineCollection[smbName] != null && _catalog.CustomCollection[smbName] != null;
        }
    }
}