
using Caliburn.Micro;
using Machinarium.Qnil;
using Machinarium.Var;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Common.Model;

namespace TickTrader.BotTerminal
{
    internal class SymbolManagerViewModel : Screen, IWindowModel
    {
        private bool _canUpdateSizes = true;
        private TraderClientModel _clientModel;
        private FeedHistoryProviderModel _historyProvider;
        private WindowManager _wndManager;
        private CustomFeedStorage _customStorage;
        private IDynamicListSource<CacheSeriesInfoViewModel> _allSeries;
        private VarContext _varContext = new VarContext();
        private IDynamicDictionarySource<string, ManagedSymbolViewModel> _customSymbols;

        public SymbolManagerViewModel(TraderClientModel clientModel, CustomFeedStorage customStorage, WindowManager wndManager)
        {
            _clientModel = clientModel;
            _historyProvider = clientModel.History;
            _wndManager = wndManager;
            _customStorage = customStorage;

            DisplayName = "Symbol Manager";

            var onlineCacheKeys = _historyProvider.Cache.GetKeysSyncCopy(new DispatcherSync());
            var customCacheKeys = customStorage.GetKeysSyncCopy(new DispatcherSync());

            var onlineCacheViewModels = onlineCacheKeys.Transform(k => new CacheSeriesInfoViewModel(k, this, _historyProvider.Cache, false));
            var customCacheViewModels = customCacheKeys.Transform(k => new CacheSeriesInfoViewModel(k, this, customStorage, true));

            var onlineSymbols = clientModel.Symbols.Select((k, v) => new ManagedSymbolViewModel(v, this));
            _customSymbols = customStorage.GetSymbolsSyncCopy(new DispatcherSync()).Select((k, v) => new ManagedSymbolViewModel(v, this));

            var symbolsBySecurity = onlineSymbols.GroupBy((k, v) => v.Security);
            var groupsBySecurity = symbolsBySecurity.TransformToList((k, v) => new ManageSymbolGrouping(v.GroupKey, v.TransformToList((k1, v1) => v1)));

            var onlineGroup = new ManageSymbolGrouping("Online", onlineSymbols.TransformToList(), groupsBySecurity);
            var customGroup = new ManageSymbolGrouping("Custom", _customSymbols.TransformToList());

            RootGroups = new ManageSymbolGrouping[] { customGroup, onlineGroup };
            SelectedGroup = _varContext.AddProperty<ManageSymbolGrouping>(customGroup);
            Symbols = _varContext.AddProperty<IObservableListSource<ManagedSymbolViewModel>>();
            SymbolFilter = _varContext.AddProperty<string>();
            CacheSeries = _varContext.AddProperty<IObservableListSource<CacheSeriesInfoViewModel>>();
            SelectedSymbol = _varContext.AddProperty<ManagedSymbolViewModel>();

            _allSeries = Dynamic.Union(onlineCacheViewModels, customCacheViewModels).TransformToList();

            _varContext.TriggerOnChange(SelectedGroup.Var, a => ApplySymbolFilter());
            _varContext.TriggerOnChange(SymbolFilter.Var, a => ApplySymbolFilter());
            _varContext.TriggerOnChange(SelectedSymbol.Var, a => UpdateCachedSeries());
        }

        public Property<ManageSymbolGrouping> SelectedGroup { get; }
        public ManageSymbolGrouping[] RootGroups { get; }

        public Property<IObservableListSource<ManagedSymbolViewModel>> Symbols { get; }
        public Property<IObservableListSource<CacheSeriesInfoViewModel>> CacheSeries { get; }
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

            if (Symbols.Value != null)
                Symbols.Value.Dispose();

            string filter = SymbolFilter.Value;

            if (string.IsNullOrWhiteSpace(filter))
                Symbols.Value = symbolSet.AsObservable();
            else
                Symbols.Value = symbolSet.Where(s => ContainsIgnoreCase(s.Name, filter)).Chain().AsObservable();
        }

        private void UpdateCachedSeries()
        {
            if (CacheSeries.Value != null)
                CacheSeries.Value.Dispose();

            if (SelectedSymbol.Value != null)
            {
                var filterSmb = SelectedSymbol.Value;
                CacheSeries.Value = _allSeries.Where(l => l.BelongsTo(filterSmb)).Chain().AsObservable();
            }
            else
                CacheSeries.Value = null;
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
            Download(SelectedSymbol.Value);
        }

        public void Download(ManagedSymbolViewModel symbol)
        {
            _wndManager.ShowDialog(new FeedDownloadViewModel(_clientModel, symbol?.Model as SymbolModel));
        }

        public void Import()
        {
            Import(SelectedSymbol.Value);
        }

        public void Import(ManagedSymbolViewModel symbol)
        {
            _wndManager.ShowDialog(new FeedImportViewModel());
        }

        public void AddSymbol()
        {
            using (var currencies = _clientModel.Currencies.TransformToList((k, v) => k).Chain().AsObservable())
            {
                var model = new SymbolCfgEditorViewModel(null, currencies, _customSymbols.Snapshot.ContainsKey);

                if (_wndManager.ShowDialog(model) == true)
                {
                    var actionModel = new ActionDialogViewModel("Adding symbol...", () => _customStorage.Add(model.GetResultingSymbol()));
                    _wndManager.ShowDialog(actionModel);
                }
            }
        }

        public void Export(CacheSeriesInfoViewModel series)
        {
            _wndManager.ShowDialog(new FeedExportViewModel(series.Key, _clientModel));
        }

        public void RemoveSymbol(ManagedSymbolViewModel symbolModel)
        {
            var actionModel = new ActionDialogViewModel("Removing symbol...",
                    () => _customStorage.Remove(symbolModel.Name));
            _wndManager.ShowDialog(actionModel);
        }

        private async void DoUpdateSizes()
        {
            CanUpdateSizes = false;

            var toUpdate = _allSeries.Snapshot.ToArray();

            foreach (var item in toUpdate)
                item.ResetSize();

            foreach (var item in toUpdate)
                await item.UpdateSize();

            CanUpdateSizes = true;
        }
    }

    internal class CacheSeriesInfoViewModel : ObservableObject
    {
        private SymbolManagerViewModel _parent;
        private FeedCache _cache;

        public CacheSeriesInfoViewModel(FeedCacheKey key, SymbolManagerViewModel parent, FeedCache cache, bool isCustom)
        {
            _parent = parent;
            _cache = cache;
            Key = key;
            Symbol = key.Symbol;
            Cfg = key.Frame + " " + key.PriceType;
        }

        public FeedCacheKey Key { get; }
        public string Symbol { get; }
        public string Cfg { get; }
        public double? Size { get; private set; }
        public bool IsCustom { get; private set; }
        public bool IsOnline => !IsCustom;

        public async Task UpdateSize()
        {
            var newSize = await Task.Factory.StartNew(() => _cache.GetCollectionSize(Key));
            Size = Math.Round(newSize.Value / (1024 * 1024), 2);
            NotifyOfPropertyChange(nameof(Size));
        }

        public bool BelongsTo(ManagedSymbolViewModel symbolViewModel)
        {
            return symbolViewModel.IsCustom == IsCustom
                && symbolViewModel.Name == Key.Symbol;
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
    }

    internal class ManagedSymbolViewModel
    {
        private SymbolManagerViewModel _parent;
        private ISymbolModel _model;

        public ManagedSymbolViewModel(ISymbolModel symbolModel, SymbolManagerViewModel parent)
        {
            _model = symbolModel;
            _parent = parent;
            Category = symbolModel.IsUserCreated ? "Custom Symbols" : "Online Symbols";
        }

        public string Description => _model.Description;
        public string Security => _model.Security;
        public string Name => _model.Name;
        public string Category { get; }
        public bool IsCustom => _model.IsUserCreated;
        public bool IsOnline => !_model.IsUserCreated;
        public ISymbolModel Model => _model;

        public void Remove()
        {
            _parent.RemoveSymbol(this);
        }

        public void Import()
        {
            _parent.Import(this);
        }

        public void Download()
        {
            _parent.Download(this);
        }
    }

    internal class ManageSymbolGrouping
    {
        public ManageSymbolGrouping(string name, IDynamicListSource<ManagedSymbolViewModel> symbols,
            IDynamicListSource<ManageSymbolGrouping> childGroups = null)
        {
            GroupName = name;
            SymbolList = symbols;
            Symbols = symbols.AsObservable();
            GroupList = childGroups;
            Childs = childGroups?.AsObservable();
        }

        public string GroupName { get; }
        public IDynamicListSource<ManagedSymbolViewModel> SymbolList { get; }
        public IDynamicListSource<ManageSymbolGrouping> GroupList { get; }
        public IObservableListSource<ManageSymbolGrouping> Childs { get; }
        public IObservableListSource<ManagedSymbolViewModel> Symbols { get; }
    }
}
