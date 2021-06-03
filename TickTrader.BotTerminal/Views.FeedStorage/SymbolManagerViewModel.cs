using Caliburn.Micro;
using Machinarium.Qnil;
using Machinarium.Var;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using TickTrader.Algo.Account;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Core.Setup;
using TickTrader.Algo.Domain;
using TickTrader.FeedStorage;

namespace TickTrader.BotTerminal
{
    internal class SymbolManagerViewModel : Screen, IWindowModel
    {
        private bool _canUpdateSizes = true;
        private TraderClientModel _clientModel;
        private FeedHistoryProviderModel.Handler _historyProvider;
        private WindowManager _wndManager;
        private SymbolCatalog _catalog;
        private VarContext _varContext = new VarContext();
        private VarDictionary<string, SymbolInfo> _onlineSymbols = new VarDictionary<string, SymbolInfo>();
        private IVarSet<SymbolKey, ManagedSymbolViewModel> _customManagedSymbols;
        private IVarSet<SymbolKey, ManagedSymbolViewModel> _onlineManagedSymbols;

        public SymbolManagerViewModel(TraderClientModel clientModel, SymbolCatalog catalog, WindowManager wndManager)
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

            _onlineManagedSymbols = catalog.OnlineSymbols.Select((k, v) => new ManagedSymbolViewModel(this, v, "Online Symbols"));
            _customManagedSymbols = catalog.CustomSymbols.Select((k, v) => new ManagedSymbolViewModel(this, v, "Custom Symbols"));

            var symbolsBySecurity = _onlineManagedSymbols.GroupBy((k, v) => v.Security);
            var groupsBySecurity = symbolsBySecurity.TransformToList((k, v) => new ManageSymbolGrouping(v.GroupKey, v.TransformToList((k1, v1) => v1)));

            var onlineGroup = new ManageSymbolGrouping("Online", _onlineManagedSymbols.TransformToList((k, v) => v), groupsBySecurity);
            var customGroup = new ManageSymbolGrouping("Custom", _customManagedSymbols.TransformToList((k, v) => v));

            RootGroups = new ManageSymbolGrouping[] { customGroup, onlineGroup };
            SelectedGroup = _varContext.AddProperty<ManageSymbolGrouping>(customGroup);
            Symbols = _varContext.AddProperty<IObservableList<ManagedSymbolViewModel>>();
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

            _onlineManagedSymbols.EnableAutodispose();
            _customManagedSymbols.EnableAutodispose();
        }

        public Property<ManageSymbolGrouping> SelectedGroup { get; }
        public ManageSymbolGrouping[] RootGroups { get; }

        public Property<IObservableList<ManagedSymbolViewModel>> Symbols { get; }
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
            Download(SelectedSymbol.Value?.Model);
        }

        public void Download(SymbolData symbol)
        {
            _wndManager.ShowDialog(new FeedDownloadViewModel(_clientModel, _catalog, symbol), this);
        }

        public void Import()
        {
            Import(SelectedSymbol.Value.Model);
        }

        public void Import(SymbolData symbol)
        {
            using (var symbolList = _customManagedSymbols.TransformToList())
                _wndManager.ShowDialog(new FeedImportViewModel(_catalog, symbol), this);
        }

        public void AddSymbol()
        {
            var model = new SymbolCfgEditorViewModel(null, _clientModel.SortedCurrenciesNames, HasSymbol);

            if (_wndManager.ShowDialog(model, this).Result == true)
            {
                var actionModel = new ActionDialogViewModel("Adding symbol...", () => _catalog.AddCustomSymbol(model.GetResultingSymbol()));
                _wndManager.ShowDialog(actionModel, this);
            }
        }

        public void EditSymbol(SymbolData symbol)
        {
            var model = new SymbolCfgEditorViewModel(((CustomSymbolData)symbol).Entity, _clientModel.SortedCurrenciesNames, HasSymbol);

            if (_wndManager.ShowDialog(model, this).Result == true)
            {
                var actionModel = new ActionDialogViewModel("Saving symbol settings...", () => _catalog.Update(model.GetResultingSymbol()));
                _wndManager.ShowDialog(actionModel, this);
            }
        }

        public void CopySymbol(SymbolData symbol)
        {
            CustomSymbol smb = null;
            if (symbol is CustomSymbolData)
            {
                smb = ((CustomSymbolData)symbol).Entity;
            }
            else
            {
                smb = CustomSymbol.FromAlgo(symbol.InfoEntity);
            }

            var model = new SymbolCfgEditorViewModel(smb, _clientModel.SortedCurrenciesNames, HasSymbol, true);

            if (_wndManager.ShowDialog(model, this).Result == true)
            {
                var actionModel = new ActionDialogViewModel("Saving symbol settings...", () => _catalog.AddCustomSymbol(model.GetResultingSymbol()));
                _wndManager.ShowDialog(actionModel, this);
            }
        }

        public void Export(CacheSeriesInfoViewModel series)
        {
            _wndManager.ShowDialog(new FeedExportViewModel(series.Model), this);
        }

        public void RemoveSymbol(SymbolData symbolModel)
        {
            var actionModel = new ActionDialogViewModel("Removing symbol...", () => symbolModel.Remove());
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
            toUpdate.AddRange(_onlineManagedSymbols.Snapshot.Values);
            toUpdate.AddRange(_customManagedSymbols.Snapshot.Values);

            foreach (var item in toUpdate)
                item.ResetSize();

            foreach (var item in toUpdate)
                await item.UpdateSize();

            CanUpdateSizes = true;
        }

        private bool HasSymbol(string smbName)
        {
            smbName = smbName.Trim();
            return _onlineManagedSymbols.Snapshot.ContainsKey(new SymbolKey(smbName, SymbolConfig.Types.SymbolOrigin.Online))
                || _customManagedSymbols.Snapshot.ContainsKey(new SymbolKey(smbName, SymbolConfig.Types.SymbolOrigin.Custom));
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
            Cfg = Key.Frame + " " + Key.MarketSide;
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

        public ManagedSymbolViewModel(SymbolManagerViewModel parent, SymbolData model, string category)
        {
            Model = model;
            Parent = parent;
            Category = category;

            _series = model.SeriesCollection.Select(s => new CacheSeriesInfoViewModel(s, parent));
            Series = _series.TransformToList().AsObservable();
        }

        protected SymbolManagerViewModel Parent { get; private set; }

        public SymbolData Model { get; }
        public string Description => Model.Description;
        public string Security => Model.Security;
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
        public ManageSymbolGrouping(string name, IVarList<ManagedSymbolViewModel> symbols,
            IVarList<ManageSymbolGrouping> childGroups = null)
        {
            GroupName = name;
            SymbolList = symbols;
            Symbols = symbols.AsObservable();
            GroupList = childGroups;
            Childs = childGroups?.AsObservable();
        }

        public string GroupName { get; }
        public IVarList<ManagedSymbolViewModel> SymbolList { get; }
        public IVarList<ManageSymbolGrouping> GroupList { get; }
        public IObservableList<ManageSymbolGrouping> Childs { get; }
        public IObservableList<ManagedSymbolViewModel> Symbols { get; }
    }
}
