using Caliburn.Micro;
using Machinarium.Qnil;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Common.Model;

namespace TickTrader.BotTerminal
{
    internal class FeedStorageViewModel : Screen, IWindowModel
    {
        private bool _canUpdateSizes = true;
        private TraderClientModel _clientModel;
        private FeedHistoryProviderModel _historyProvider;
        private WindowManager _wndManager;

        public FeedStorageViewModel(TraderClientModel clientModel, WindowManager wndManager)
        {
            _clientModel = clientModel;
            _historyProvider = clientModel.History;
            _wndManager = wndManager;

            ServerItems = new ObservableCollection<CacheSeriesInfoViewModel>();
            CustomItems = new ObservableCollection<CacheSeriesInfoViewModel>();

            lock (_historyProvider.OnlineCache.SyncObj)
            {
                foreach (var key in _historyProvider.OnlineCache.Keys)
                    ServerItems.Add(new CacheSeriesInfoViewModel(key, this));

                _historyProvider.OnlineCache.Added += k => Execute.OnUIThread(() => ServerItems.Add(new CacheSeriesInfoViewModel(k, this)));
                _historyProvider.OnlineCache.Removed += k => { };
            }
        }

        //private void OnlineKeys_Updated(SetUpdateArgs<FeedCacheKey> args)
        //{
        //    if (args.Action == DLinqAction.Insert)
        //        Execute.OnUIThread(() => ServerItems.Add(new CacheSeriesInfoViewModel(args.NewItem)));
        //}

        public ObservableCollection<CacheSeriesInfoViewModel> ServerItems { get; }
        public ObservableCollection<CacheSeriesInfoViewModel> CustomItems { get; }

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

        public void Export(CacheSeriesInfoViewModel series)
        {
            _wndManager.ShowDialog(new FeedExportViewModel(series.Key, _clientModel));
        }

        private async void DoUpdateSizes()
        {
            CanUpdateSizes = false;

            foreach (var item in ServerItems)
                item.UpdateSize(null);

            var onlineCache = _historyProvider.OnlineCache;

            List<FeedCacheKey> toUpdate;

            lock (onlineCache.SyncObj)
                toUpdate = onlineCache.Keys.ToList();

            var sizes = await Task.Factory.StartNew(() =>
            {
                return toUpdate.Select(k => new KeyValuePair<FeedCacheKey, double?>(k, onlineCache.GetCollectionSize(k)));
            });

            foreach (var pair in sizes)
                ServerItems.FirstOrDefault(i => i.Key == pair.Key)?.UpdateSize(pair.Value);

            CanUpdateSizes = true;
        }
    }

    internal class CacheSeriesInfoViewModel : ObservableObject
    {
        private FeedStorageViewModel _parent;

        public CacheSeriesInfoViewModel(FeedCacheKey key, FeedStorageViewModel parent)
        {
            _parent = parent;
            Key = key;
            Symbol = key.Symbol;
            Cfg = key.Frame + " " + key.PriceType;
        }

        public FeedCacheKey Key { get; }
        public string Symbol { get; }
        public string Cfg { get; }
        public double? Size { get; private set; }

        public void UpdateSize(double? newSize)
        {
            if (newSize == null)
                Size = null;
            else
                Size = Math.Round(newSize.Value / (1024 * 1024), 2);

            NotifyOfPropertyChange(nameof(Size));
        }

        public void Export()
        {
            _parent.Export(this);
        }
    }
}
