
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
        private FeedHistoryProviderModel.Handler _historyProvider;
        private WindowManager _wndManager;
        private CustomFeedStorage _customStorage;
        private VarContext _varContext = new VarContext();
        private VarDictionary<string, SymbolModel> _onlineSymbols = new VarDictionary<string, SymbolModel>();
        private IVarSet<string, ManagedCustomSymbol> _customManagedSymbols;
        private IVarSet<string, ManagedOnlineSymbol> _onlineManagedSymbols;

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

            _onlineManagedSymbols = _onlineSymbols.Select((k, v) => new ManagedOnlineSymbol((SymbolModel)v, this, onlineSeries));
            _customManagedSymbols = customStorage.GetSymbolsSyncCopy(new DispatcherSync()).Select((k, v) => new ManagedCustomSymbol(v, this, customStorage, customSeries));

            var symbolsBySecurity = _onlineManagedSymbols.GroupBy((k, v) => v.Security);
            var groupsBySecurity = symbolsBySecurity.TransformToList((k, v) => new ManageSymbolGrouping(v.GroupKey, v.TransformToList((k1, v1) => (ManagedSymbol)v1)));

            var onlineGroup = new ManageSymbolGrouping("Online", _onlineManagedSymbols.TransformToList((k,v) => (ManagedSymbol)v), groupsBySecurity);
            var customGroup = new ManageSymbolGrouping("Custom", _customManagedSymbols.TransformToList((k, v) => (ManagedSymbol)v));

            RootGroups = new ManageSymbolGrouping[] { customGroup, onlineGroup };
            SelectedGroup = _varContext.AddProperty<ManageSymbolGrouping>(customGroup);
            Symbols = _varContext.AddProperty<IObservableList<ManagedSymbol>>();
            SymbolFilter = _varContext.AddProperty<string>();
            SelectedSymbol = _varContext.AddProperty<ManagedSymbol>();
            CacheSeries = SelectedSymbol.Var.Ref(s => s.Series);

            _varContext.TriggerOnChange(SelectedGroup.Var, a => ApplySymbolFilter());
            _varContext.TriggerOnChange(SymbolFilter.Var, a => ApplySymbolFilter());

            _varContext.TriggerOn(clientModel.IsConnected, () =>
            {
                _onlineSymbols.Clear();
                foreach (var i in clientModel.Symbols.Snapshot)
                    _onlineSymbols.Add(i.Key, (SymbolModel)i.Value);
            });

            _onlineManagedSymbols.EnableAutodispose();
            _customManagedSymbols.EnableAutodispose();
        }

        public Property<ManageSymbolGrouping> SelectedGroup { get; }
        public ManageSymbolGrouping[] RootGroups { get; }

        public Property<IObservableList<ManagedSymbol>> Symbols { get; }
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
            _wndManager.ShowDialog(new FeedDownloadViewModel(_clientModel, symbol?.Model as SymbolModel), this);
        }

        public void Import()
        {
            Import(SelectedSymbol.Value as ManagedCustomSymbol);
        }

        public void Import(ManagedCustomSymbol symbol)
        {
            using (var symbolList = _customManagedSymbols.TransformToList())
                _wndManager.ShowDialog(new FeedImportViewModel(symbolList, symbol), this);
        }

        public void AddSymbol()
        {
            using (var currencies = _clientModel.Currencies.TransformToList((k, v) => k).Chain().AsObservable())
            {
                var model = new SymbolCfgEditorViewModel(null, currencies, _customManagedSymbols.Snapshot.ContainsKey);

                if (_wndManager.ShowDialog(model, this) == true)
                {
                    var actionModel = new ActionDialogViewModel("Adding symbol...", () => _customStorage.Add(model.GetResultingSymbol()));
                    _wndManager.ShowDialog(actionModel, this);
                }
            }
        }

        public void EditSymbol(ManagedCustomSymbol symbol)
        {
            using (var currencies = _clientModel.Currencies.TransformToList((k, v) => k).Chain().AsObservable())
            {
                var model = new SymbolCfgEditorViewModel(symbol.Model, currencies, _customManagedSymbols.Snapshot.ContainsKey);

                if (_wndManager.ShowDialog(model, this) == true)
                {
                    var actionModel = new ActionDialogViewModel("Saving symbol settings...", () => _customStorage.Update(model.GetResultingSymbol()));
                    _wndManager.ShowDialog(actionModel, this);
                }
            }
        }

        public void Export(CacheSeriesInfoViewModel series)
        {
            _wndManager.ShowDialog(new FeedExportViewModel(series.Key, series.Storage), this);
        }

        public void RemoveSymbol(ManagedCustomSymbol symbolModel)
        {
            var actionModel = new ActionDialogViewModel("Removing symbol...",
                    () => _customStorage.Remove(symbolModel.Name));
            _wndManager.ShowDialog(actionModel, this);
        }

        public void RemoveSeries(CacheSeriesInfoViewModel series)
        {
            var actionModel = new ActionDialogViewModel("Removing series...",
                   () => series.Storage.RemoveSeries(series.Key));
            _wndManager.ShowDialog(actionModel, this);
        }

        private async void DoUpdateSizes()
        {
            CanUpdateSizes = false;

            var toUpdate = new List<ManagedSymbol>();
            toUpdate.AddRange(_onlineManagedSymbols.Snapshot.Values);
            toUpdate.AddRange(_customManagedSymbols.Snapshot.Values);

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
        public FeedCache Storage => _cache;

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

        public void Remove()
        {
            _parent.RemoveSeries(this);
        }
    }

    internal abstract class ManagedSymbol : ObservableObject, IDisposable
    {
        private IVarSet<CacheSeriesInfoViewModel> _series;

        protected void Init(SymbolManagerViewModel parent, IVarSet<CacheSeriesInfoViewModel> storageCollection)
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

        public ManagedOnlineSymbol(SymbolModel symbolModel, SymbolManagerViewModel parent, IVarSet<CacheSeriesInfoViewModel> storageCollection)
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
            IVarSet<CacheSeriesInfoViewModel> storageCollection)
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
        public CustomSymbol Model => _model;

        public void Remove()
        {
            Parent.RemoveSymbol(this);
        }

        public void Import()
        {
            Parent.Import(this);
        }

        public void Edit()
        {
            Parent.EditSymbol(this);
        }

        public void WriteSlice(TimeFrames frame, BarPriceType priceType, DateTime from, DateTime to, BarEntity[] values)
        {
            _storage.Put(Name, frame, priceType, from, to, values);
        }

        public void WriteSlice(TimeFrames timeFrame, DateTime from, DateTime to, QuoteEntity[] values)
        {
            _storage.Put(Name, timeFrame, from, to, values);
        }
    }

    internal class ManageSymbolGrouping
    {
        public ManageSymbolGrouping(string name, IVarList<ManagedSymbol> symbols,
            IVarList<ManageSymbolGrouping> childGroups = null)
        {
            GroupName = name;
            SymbolList = symbols;
            Symbols = symbols.AsObservable();
            GroupList = childGroups;
            Childs = childGroups?.AsObservable();
        }

        public string GroupName { get; }
        public IVarList<ManagedSymbol> SymbolList { get; }
        public IVarList<ManageSymbolGrouping> GroupList { get; }
        public IObservableList<ManageSymbolGrouping> Childs { get; }
        public IObservableList<ManagedSymbol> Symbols { get; }
    }
}
