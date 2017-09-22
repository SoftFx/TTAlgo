using Caliburn.Micro;
using Machinarium.Qnil;
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
        private IDynamicListSource<ManagedSymbolViewModel> _allSymbols;
        private string _symbolFilter;

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

            CacheSeries = Dynamic.Union(onlineCacheViewModels, customCacheViewModels).AsObservable();

            var onlineSymbolList = clientModel.Symbols.TransformToList((k, v) => new ManagedSymbolViewModel(v, this));
            var customSymbolList = customStorage.GetSymbolsSyncCopy(new DispatcherSync()).TransformToList((k, v) => new ManagedSymbolViewModel(v, this));

            _allSymbols = Dynamic.Combine(onlineSymbolList, customSymbolList);
            ApplySymbolFilter();
        }

        public IObservableListSource<CacheSeriesInfoViewModel> CacheSeries { get; }
        public IObservableListSource<ManagedSymbolViewModel> Symbols { get; private set; }

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

        public string SymbolFilter
        {
            get => _symbolFilter;
            set
            {
                _symbolFilter = value;
                NotifyOfPropertyChange(nameof(SymbolFilter));
                ApplySymbolFilter();
            }
        }

        private void ApplySymbolFilter()
        {
            if (Symbols != null)
                Symbols.Dispose();

            if (string.IsNullOrWhiteSpace(_symbolFilter))
            {
                Symbols = _allSymbols.Chain().AsObservable();
            }
            else
            {
                var filterCopy = _symbolFilter;
                Symbols = _allSymbols.Where(s => CultureInfo.InvariantCulture.CompareInfo.IndexOf(s.Name, filterCopy, CompareOptions.IgnoreCase) >= 0).Chain().AsObservable();
            }

            NotifyOfPropertyChange(nameof(Symbols));
        }

        public void UpdateSizes()
        {
            DoUpdateSizes();
        }

        public void Download()
        {
            _wndManager.ShowDialog(new FeedDownloadViewModel(_clientModel));
        }

        public void Import()
        {
            _wndManager.ShowDialog(new FeedImportViewModel());
        }

        public void AddSymbol()
        {
            var model = new SymbolCfgEditorViewModel(null, _clientModel);

            if (_wndManager.ShowDialog(model) == true)
            {
                var actionModel = new ActionDialogViewModel("Adding symbol...",
                    () => _customStorage.Add(model.GetResultingSymbol()));
                _wndManager.ShowDialog(actionModel);
            }

            //var actionModel = new ActionDialogViewModel("Adding symbol...", () => Task.Delay(5000).Wait());
            //_wndManager.ShowDialog(actionModel);

            //actionModel = new ActionDialogViewModel("Adding symbol...", (c) => Task.Delay(5000, c).Wait());
            //_wndManager.ShowDialog(actionModel);
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

            var toUpdate = CacheSeries.ToArray();

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
        public string Name => _model.Name;
        public string Category { get; }

        public void Remove()
        {
            _parent.RemoveSymbol(this);
        }
    }
}
