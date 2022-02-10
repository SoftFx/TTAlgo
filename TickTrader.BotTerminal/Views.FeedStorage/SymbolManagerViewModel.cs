using Caliburn.Micro;
using Machinarium.Qnil;
using Machinarium.Var;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using TickTrader.Algo.Account;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;
using TickTrader.FeedStorage;
using TickTrader.FeedStorage.Api;

namespace TickTrader.BotTerminal
{
    internal class SymbolManagerViewModel : Screen, IWindowModel
    {
        private bool _canUpdateSizes = true;
        private TraderClientModel _clientModel;
        private FeedHistoryProviderModel.Handler _historyProvider;
        private WindowManager _wndManager;
        private ISymbolCatalog _catalog;
        private VarContext _varContext = new VarContext();
        private VarDictionary<string, SymbolInfo> _onlineSymbols = new VarDictionary<string, SymbolInfo>();
        private List<ManagedSymbolViewModel> _customManagedSymbols;
        private List<ManagedSymbolViewModel> _onlineManagedSymbols;

        public SymbolManagerViewModel(TraderClientModel clientModel, ISymbolCatalog catalog, WindowManager wndManager)
        {
            _clientModel = clientModel;
            _historyProvider = clientModel.FeedHistory;
            _wndManager = wndManager;
            _catalog = catalog;

            DisplayName = "Symbol Manager";

            //var onlineCacheKeys = _historyProvider.Cache;
            //var customCacheKeys = customStorage;

            //var onlineSeries = onlineCacheKeys.Transform(k => new CacheSeriesInfoViewModel(k, this, _historyProvider.Cache, false));
            //var customSeries = customCacheKeys.Transform(k => new CacheSeriesInfoViewModel(k, this, customStorage, true));

            _onlineManagedSymbols = catalog.OnlineCollection.Symbols.Select(v => new ManagedSymbolViewModel(this, v, "Online Symbols")).ToList();
            _customManagedSymbols = catalog.CustomCollection.Symbols.Select(v => new ManagedSymbolViewModel(this, v, "Custom Symbols")).ToList();

            var symbolsBySecurity = _onlineManagedSymbols.GroupBy(v => v.Security);
            var groupsBySecurity = symbolsBySecurity.Select(v => new ManageSymbolGrouping(v.Key, v.ToList())).ToList();

            var onlineGroup = new ManageSymbolGrouping("Online", _onlineManagedSymbols, groupsBySecurity);
            var customGroup = new ManageSymbolGrouping("Custom", _customManagedSymbols);

            RootGroups = new ManageSymbolGrouping[] { customGroup, onlineGroup };
            SelectedGroup = _varContext.AddProperty<ManageSymbolGrouping>(customGroup);
            Symbols = _varContext.AddProperty<ObservableCollection<ManagedSymbolViewModel>>();
            SymbolFilter = _varContext.AddProperty<string>();
            SelectedSymbol = _varContext.AddProperty<ManagedSymbolViewModel>();
            CacheSeries = SelectedSymbol.Var.Ref(s => s.Series);

            _varContext.TriggerOnChange(SelectedGroup.Var, a => ApplySymbolFilter());
            _varContext.TriggerOnChange(SymbolFilter.Var, a => ApplySymbolFilter());

            _varContext.TriggerOn(clientModel.IsConnected, () =>
            {
                _onlineSymbols.Clear();
                foreach (var i in clientModel.Symbols.Snapshot)
                    _onlineSymbols.Add(i.Key, i.Value);
            });

            //_onlineManagedSymbols.EnableAutodispose();
            //_customManagedSymbols.EnableAutodispose();
        }

        public Property<ManageSymbolGrouping> SelectedGroup { get; }
        public ManageSymbolGrouping[] RootGroups { get; }

        public Property<ObservableCollection<ManagedSymbolViewModel>> Symbols { get; }
        public Var<IEnumerable<CacheSeriesInfoViewModel>> CacheSeries { get; }
        public Property<ManagedSymbolViewModel> SelectedSymbol { get; }
        public Property<string> SymbolFilter { get; }

        public bool CanUpdateSizes
        {
            get { return _canUpdateSizes; }
            set
            {
                if (_canUpdateSizes != value)
                {
                    _canUpdateSizes = value;
                    NotifyOfPropertyChange(nameof(CanUpdateSizes));
                }
            }
        }

        private void ApplySymbolFilter()
        {
            var symbolSet = SelectedGroup.Value.SymbolList;

            //if (Symbols.Value != null)
            //    Symbols.Value.Dispose();

            string filter = SymbolFilter.Value;

            if (string.IsNullOrWhiteSpace(filter))
                Symbols.Value = new ObservableCollection<ManagedSymbolViewModel>(symbolSet);
            else
                Symbols.Value = new ObservableCollection<ManagedSymbolViewModel>(symbolSet.Where(s => ContainsIgnoreCase(s.Name, filter)));
        }

        private static bool ContainsIgnoreCase(string str, string searchStr)
        {
            return CultureInfo.InvariantCulture.CompareInfo.IndexOf(str, searchStr, CompareOptions.IgnoreCase) >= 0;
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
            Import(SelectedSymbol.Value.Model);
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

        public void Export(CacheSeriesInfoViewModel series)
        {
            _wndManager.ShowDialog(new FeedExportViewModel(series.Model), this);
        }

        public void RemoveSymbol(ISymbolData symbolModel)
        {
            var actionModel = new ActionDialogViewModel("Removing symbol...", () => _catalog.CustomCollection.TryRemoveSymbol(symbolModel.Name));
            _wndManager.ShowDialog(actionModel, this);
        }

        public void RemoveSeries(CacheSeriesInfoViewModel series)
        {
            var actionModel = new ActionDialogViewModel("Removing series...", () => series.Remove());
            _wndManager.ShowDialog(actionModel, this);
        }

        private async void DoUpdateSizes()
        {
            CanUpdateSizes = false;

            var toUpdate = new List<ManagedSymbolViewModel>();
            toUpdate.AddRange(_onlineManagedSymbols);
            toUpdate.AddRange(_customManagedSymbols);

            foreach (var item in toUpdate)
                item.ResetSize();

            foreach (var item in toUpdate)
                await item.UpdateSize();

            CanUpdateSizes = true;
        }

        private bool HasSymbol(string smbName)
        {
            smbName = smbName.Trim();
            return _catalog.OnlineCollection[smbName/*new SymbolKey(smbName, SymbolConfig.Types.SymbolOrigin.Online)*/] != null &&
                   _catalog.CustomCollection[smbName/*new SymbolKey(smbName, SymbolConfig.Types.SymbolOrigin.Custom)*/] != null;
        }
    }

    internal class CacheSeriesInfoViewModel : ObservableObject
    {
        private SymbolManagerViewModel _parent;

        public CacheSeriesInfoViewModel(SymbolStorageSeries series, SymbolManagerViewModel parent)
        {
            _parent = parent;
            Model = series;
            Key = series.Key;
            Symbol = Key.Symbol;
            Cfg = Key.TimeFrame + " " + Key.MarketSide;
        }

        public FeedCacheKey Key { get; }
        public string Symbol { get; }
        public string Cfg { get; }
        public double? Size { get; private set; }
        public SymbolStorageSeries Model { get; }

        public async Task UpdateSize()
        {
            var newSize = await Model.GetCollectionSize();
            Size = Math.Round(newSize.Value / (1024 * 1024), 2);
            NotifyOfPropertyChange(nameof(Size));
        }

        public void ResetSize()
        {
            Size = null;
            NotifyOfPropertyChange(nameof(Size));
        }

        public void Export()
        {
            _parent.Export(this);
        }

        public void Remove()
        {
            Model.Remove();
        }
    }

    internal class ManagedSymbolViewModel : ObservableObject, IDisposable
    {
        private IVarSet<CacheSeriesInfoViewModel> _series;

        public ManagedSymbolViewModel(SymbolManagerViewModel parent, ISymbolData model, string category)
        {
            Model = model;
            Parent = parent;
            Category = category;

            _series = model.SeriesCollection.Select(s => new CacheSeriesInfoViewModel(s, parent));
            Series = _series.TransformToList().AsObservable();
        }

        protected SymbolManagerViewModel Parent { get; private set; }

        public ISymbolData Model { get; }
        public string Description => Model.Info.Description;
        public string Security => Model.Info.Security;
        public string Name => Model.Name;
        public string Category { get; }
        public bool IsCustom => Model.IsCustom;
        public bool IsOnline => !Model.IsCustom;
        public double? DiskSize { get; private set; }

        public IEnumerable<CacheSeriesInfoViewModel> Series { get; private set; }

        public virtual void Dispose()
        {
            _series.Dispose();
        }

        public async Task UpdateSize()
        {
            var toUpdate = Series.ToList();

            double totalSize = 0;

            foreach (var s in toUpdate)
            {
                await s.UpdateSize();
                totalSize += (s.Size ?? 0);
            }

            DiskSize = totalSize;
            NotifyOfPropertyChange(nameof(DiskSize));
        }

        public void ResetSize()
        {
            DiskSize = null;
            NotifyOfPropertyChange(nameof(DiskSize));
            Series.ForEach(s => s.ResetSize());
        }

        public void Download()
        {
            Parent.Download(Model);
        }

        public void Remove()
        {
            Parent.RemoveSymbol(Model);
        }

        public void Import()
        {
            Parent.Import(Model);
        }

        public void Edit()
        {
            Parent.EditSymbol(Model);
        }

        public void Copy()
        {
            Parent.CopySymbol(Model);
        }
    }

    internal class ManageSymbolGrouping
    {
        public ManageSymbolGrouping(string name, IList<ManagedSymbolViewModel> symbols,
            IList<ManageSymbolGrouping> childGroups = null)
        {
            GroupName = name;
            SymbolList = symbols;
            Symbols = new ObservableCollection<ManagedSymbolViewModel>(symbols);
            GroupList = childGroups;

            if (childGroups != null)
                Childs = new ObservableCollection<ManageSymbolGrouping>(childGroups);
        }

        public string GroupName { get; }
        public IList<ManagedSymbolViewModel> SymbolList { get; }
        public IList<ManageSymbolGrouping> GroupList { get; }
        public ObservableCollection<ManageSymbolGrouping> Childs { get; }
        public ObservableCollection<ManagedSymbolViewModel> Symbols { get; }
    }
}
