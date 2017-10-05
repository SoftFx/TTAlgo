
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
using TickTrader.Algo.Api;
using TickTrader.Algo.Common.Lib;
using TickTrader.Algo.Common.Model;
using TickTrader.Algo.Core;

namespace TickTrader.BotTerminal
{
    internal class SymbolManagerViewModel : Screen, IWindowModel
    {
        private bool _canUpdateSizes = true;
        private TraderClientModel _clientModel;
        private FeedHistoryProviderModel _historyProvider;
        private WindowManager _wndManager;
        private CustomFeedStorage _customStorage;
        //private IDynamicListSource<CacheSeriesInfoViewModel> _allSeries;
        private VarContext _varContext = new VarContext();
        private IDynamicDictionarySource<string, ManagedCustomSymbol> _customSymbols;
        private IDynamicDictionarySource<string, ManagedOnlineSymbol> _onlineSymbols;

        public SymbolManagerViewModel(TraderClientModel clientModel, CustomFeedStorage customStorage, WindowManager wndManager)
        {
            _clientModel = clientModel;
            _historyProvider = clientModel.History;
            _wndManager = wndManager;
            _customStorage = customStorage;

            DisplayName = "Symbol Manager";

            var onlineCacheKeys = _historyProvider.Cache.GetKeysSyncCopy(new DispatcherSync());
            var customCacheKeys = customStorage.GetKeysSyncCopy(new DispatcherSync());

            var onlineSeries = onlineCacheKeys.Transform(k => new CacheSeriesInfoViewModel(k, this, _historyProvider.Cache, false));
            var customSeries = customCacheKeys.Transform(k => new CacheSeriesInfoViewModel(k, this, customStorage, true));

            _onlineSymbols = clientModel.Symbols.Select((k, v) => new ManagedOnlineSymbol((SymbolModel)v, this, onlineSeries));
            _customSymbols = customStorage.GetSymbolsSyncCopy(new DispatcherSync()).Select((k, v) => new ManagedCustomSymbol(v, this, customStorage, customSeries));

            var symbolsBySecurity = _onlineSymbols.GroupBy((k, v) => v.Security);
            var groupsBySecurity = symbolsBySecurity.TransformToList((k, v) => new ManageSymbolGrouping(v.GroupKey, v.TransformToList((k1, v1) => (ManagedSymbol)v1)));

            var onlineGroup = new ManageSymbolGrouping("Online", _onlineSymbols.TransformToList((k,v) => (ManagedSymbol)v), groupsBySecurity);
            var customGroup = new ManageSymbolGrouping("Custom", _customSymbols.TransformToList((k, v) => (ManagedSymbol)v));

            RootGroups = new ManageSymbolGrouping[] { customGroup, onlineGroup };
            SelectedGroup = _varContext.AddProperty<ManageSymbolGrouping>(customGroup);
            Symbols = _varContext.AddProperty<IObservableListSource<ManagedSymbol>>();
            SymbolFilter = _varContext.AddProperty<string>();
            SelectedSymbol = _varContext.AddProperty<ManagedSymbol>();
            CacheSeries = SelectedSymbol.Var.Ref(s => s.Series);

            //_allSeries = Dynamic.Union(onlineSeries, customSeries).TransformToList();

            _varContext.TriggerOnChange(SelectedGroup.Var, a => ApplySymbolFilter());
            _varContext.TriggerOnChange(SymbolFilter.Var, a => ApplySymbolFilter());
        }

        public Property<ManageSymbolGrouping> SelectedGroup { get; }
        public ManageSymbolGrouping[] RootGroups { get; }

        public Property<IObservableListSource<ManagedSymbol>> Symbols { get; }
        public Var<IEnumerable<CacheSeriesInfoViewModel>> CacheSeries { get; }
        public Property<ManagedSymbol> SelectedSymbol { get; }
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
            Download(SelectedSymbol.Value as ManagedOnlineSymbol);
        }

        public void Download(ManagedOnlineSymbol symbol)
        {
            _wndManager.ShowDialog(new FeedDownloadViewModel(_clientModel, symbol?.Model as SymbolModel));
        }

        public void Import()
        {
            Import(SelectedSymbol.Value as ManagedCustomSymbol);
        }

        public void Import(ManagedCustomSymbol symbol)
        {
            using (var symbolList = _customSymbols.TransformToList())
                _wndManager.ShowDialog(new FeedImportViewModel(symbolList, symbol));
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

        public void RemoveSymbol(ManagedCustomSymbol symbolModel)
        {
            var actionModel = new ActionDialogViewModel("Removing symbol...",
                    () => _customStorage.Remove(symbolModel.Name));
            _wndManager.ShowDialog(actionModel);
        }

        private async void DoUpdateSizes()
        {
            CanUpdateSizes = false;

            var toUpdate = new List<ManagedSymbol>();
            toUpdate.AddRange(_onlineSymbols.Snapshot.Values);
            toUpdate.AddRange(_customSymbols.Snapshot.Values);

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

        public async Task UpdateSize()
        {
            var newSize = await Task.Factory.StartNew(() => _cache.GetCollectionSize(Key));
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
    }

    internal abstract class ManagedSymbol : ObservableObject, IDisposable
    {
        private IDynamicSetSource<CacheSeriesInfoViewModel> _series;

        protected void Init(SymbolManagerViewModel parent, IDynamicSetSource<CacheSeriesInfoViewModel> storageCollection)
        {
            Parent = parent;
            _series = storageCollection.Where(i => i.Key.Symbol == Name);
            Series = _series.TransformToList().AsObservable();
        }

        protected SymbolManagerViewModel Parent { get; private set; }

        public abstract string Description { get; }
        public abstract string Security { get; }
        public abstract string Name { get; }
        public abstract string Category { get; }
        public abstract bool IsCustom { get; }
        public abstract bool IsOnline { get; }
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
            Series.Foreach(s => s.ResetSize());
        }
    }

    internal class ManagedOnlineSymbol : ManagedSymbol
    {
        private SymbolModel _model;

        public ManagedOnlineSymbol(SymbolModel symbolModel, SymbolManagerViewModel parent, IDynamicSetSource<CacheSeriesInfoViewModel> storageCollection)
        {
            _model = symbolModel;
            Init(parent, storageCollection);
        }

        public override string Description => _model.Description;
        public override string Security => _model.Descriptor.SecurityName;
        public override string Name => _model.Name;
        public override string Category => "Online Symbols";
        public override bool IsCustom => false;
        public override bool IsOnline => true;
        public SymbolModel Model => _model;

        public void Download()
        {
            Parent.Download(this);
        }
    }

    internal class ManagedCustomSymbol : ManagedSymbol
    {
        private CustomSymbol _model;
        private CustomFeedStorage _storage;

        public ManagedCustomSymbol(CustomSymbol symbolModel, SymbolManagerViewModel parent, CustomFeedStorage storage,
            IDynamicSetSource<CacheSeriesInfoViewModel> storageCollection)
        {
            _model = symbolModel;
            _storage = storage;
            Init(parent, storageCollection);
        }

        public override string Description => _model.Description;
        public override string Security => "";
        public override string Name => _model.Name;
        public override string Category => "Custom Symbols";
        public override bool IsCustom => true;
        public override bool IsOnline => false;

        public void Remove()
        {
            Parent.RemoveSymbol(this);
        }

        public void Import()
        {
            Parent.Import(this);
        }

        public void WriteSlice(TimeFrames frame, BarPriceType priceType, DateTime from, DateTime to, BarEntity[] values)
        {
            _storage.Put(Name, frame, priceType, from, to, values);
        }
    }

    internal class ManageSymbolGrouping
    {
        public ManageSymbolGrouping(string name, IDynamicListSource<ManagedSymbol> symbols,
            IDynamicListSource<ManageSymbolGrouping> childGroups = null)
        {
            GroupName = name;
            SymbolList = symbols;
            Symbols = symbols.AsObservable();
            GroupList = childGroups;
            Childs = childGroups?.AsObservable();
        }

        public string GroupName { get; }
        public IDynamicListSource<ManagedSymbol> SymbolList { get; }
        public IDynamicListSource<ManageSymbolGrouping> GroupList { get; }
        public IObservableListSource<ManageSymbolGrouping> Childs { get; }
        public IObservableListSource<ManagedSymbol> Symbols { get; }
    }
}
