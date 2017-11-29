using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using TickTrader.Algo.Api;
using TickTrader.Algo.Common.Lib;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Core.Math;
using TickTrader.SeriesStorage;
using TickTrader.Server.QuoteHistory.Serialization;
using TT = TickTrader.BusinessObjects;

namespace TickTrader.Algo.Common.Model
{
    public class FeedHistoryProviderModel
    {
        private static readonly IAlgoCoreLogger logger = CoreLoggerFactory.GetLogger<FeedHistoryProviderModel>();

        private const int SliceMaxSize = 8000;

        private ConnectionModel connection;
        private string _dataFolder;
        private FeedHistoryFolderOptions _folderOptions;
        private FeedCache _diskCache = new FeedCache();

        public FeedHistoryProviderModel(ConnectionModel connection, string onlieDataFolder, FeedHistoryFolderOptions folderOptions)
        {
            _dataFolder = onlieDataFolder;
            this.connection = connection;
            _folderOptions = folderOptions;
        }

        private Task Connection_Initalizing(object sender, CancellationToken cancelToken)
        {
            return Init();
        }

        private Task Connection_Deinitalizing(object sender, CancellationToken cancelToken)
        {
            return Deinit();
        }

        #region Public Interface

        public FeedCache Cache => _diskCache;

        public async Task Init()
        {
            var onlineFolder = _dataFolder;
            if (_folderOptions == FeedHistoryFolderOptions.ServerHierarchy || _folderOptions == FeedHistoryFolderOptions.ServerClientHierarchy)
                onlineFolder = Path.Combine(onlineFolder, PathEscaper.Escape(connection.CurrentServer));
            if (_folderOptions == FeedHistoryFolderOptions.ServerClientHierarchy)
                onlineFolder = Path.Combine(onlineFolder, PathEscaper.Escape(connection.CurrentLogin));

            await Task.Factory.StartNew(() => _diskCache.Start(onlineFolder));
        }

        public async Task Deinit()
        {
            try
            {
                await Task.Factory.StartNew(() => _diskCache.Stop());
            }
            catch (Exception ex)
            {
                logger.Error("Init ERROR " + ex.ToString());
            }
        }

        public Task<Tuple<DateTime, DateTime>> GetAvailableRange(string symbol, BarPriceType priceType, TimeFrames timeFrame)
        {
            return connection.FeedProxy.GetAvailableRange(symbol, priceType, timeFrame);

            //if (timeFrame != TimeFrames.Ticks)
            //{
            //    var result = await connection.FeedProxy.GetHistoryBars(symbol, DateTime.Now, 1, priceType, timeFrame);
            //    return new Tuple<DateTime, DateTime>(result.FromAll, result.ToAll);
            //}
            //else
            //    throw new Exception("Ticks is not supported!");
        }

        public Task<BarEntity[]> GetBarPage(string symbol, BarPriceType priceType, TimeFrames timeFrame, DateTime startTime, int count)
        {
            return connection.FeedProxy.DownloadBarPage(symbol, startTime, count, priceType, timeFrame);
        }

        //public IAsyncEnumerator<BarStreamSlice> GetBars(string symbol, TimeFrames timeFrame, BarPriceType priceType, DateTime from, DateTime to)
        //{
        //    var buffer = new AsyncBuffer<BarStreamSlice>(2);
        //    EnumerateBarsInternal(buffer, symbol, timeFrame, priceType, from, to);
        //    return buffer;
        //}

        public IAsyncEnumerator<SliceInfo> DownloadBarSeriesToStorage(string symbol, TimeFrames timeFrame, BarPriceType priceType, DateTime from, DateTime to)
        {
            var buffer = new AsyncBuffer<SliceInfo>();
            PopulateBarCache(buffer, symbol, timeFrame, priceType, from, to);
            return buffer;

            //return PopulateBarCache(symbol, timeFrame, priceType, from, to).ToAsyncEnumerator();
        }

        public IAsyncEnumerator<SliceInfo> DownloadTickSeriesToStorage(string symbol, TimeFrames timeFrame, DateTime from, DateTime to)
        {
            throw new NotImplementedException();
            //return DownloadTicksInternal(symbol, timeFrame, from, to).ToAsyncEnumerator();
        }

        #endregion

        private async Task<DateTime> DownloadBarsInternal(Func<BarStreamSlice, Task<bool>> outputAction, FeedCacheKey key, DateTime from, DateTime to)
        {
            using (var barEnum = connection.FeedProxy.DownloadBars(key.Symbol, from, to, key.PriceType.Value, key.Frame))
            {
                var i = from;
                while (await barEnum.Next().ConfigureAwait(false))
                {
                    var bars = barEnum.Current;
                    var sliceTo = bars.Last().CloseTime;
                    System.Diagnostics.Debug.WriteLine("barEnum.Next() {0} - {1}", i, sliceTo);
                    var slice = new BarStreamSlice(i, sliceTo, bars);
                    Cache.Put(key, slice.From, slice.To, slice.Bars);
                    if (!await outputAction(slice).ConfigureAwait(false))
                        break;
                    i = sliceTo;
                }
                return i;
            }
        }

        private async void EnumerateBarSlices(AsyncBuffer<BarStreamSlice> buffer, string symbol, TimeFrames timeFrame, BarPriceType priceType, DateTime from, DateTime to)
        {
            var key = new FeedCacheKey(symbol, timeFrame, priceType);
            var i = from;
            foreach (var cacheItem in Cache.IterateBarCache(key, from, to))
            {
                if (cacheItem.From == i)
                {
                    var writeFlag = await buffer.WriteAsync(new BarStreamSlice(cacheItem.From, cacheItem.To, cacheItem.Content.ToArray()));
                    if (!writeFlag)
                        return;
                    i = cacheItem.To;
                }
                else
                    i = await DownloadBarsInternal(s => buffer.WriteAsync(s), key, i, cacheItem.From);
            }

            if (i < to)
                i =  await DownloadBarsInternal(s => buffer.WriteAsync(s), key, i, to);
        }

        private async void PopulateBarCache(AsyncBuffer<SliceInfo> buffer, string symbol, TimeFrames timeFrame, BarPriceType priceType, DateTime from, DateTime to)
        {
            try
            {
                var key = new FeedCacheKey(symbol, timeFrame, priceType);
                var i = from;
                foreach (var cacheItem in Cache.IterateCacheKeys(key, from, to))
                {
                    if (cacheItem.From == i)
                    {
                        var writeFlag = await buffer.WriteAsync(new SliceInfo(cacheItem.From, cacheItem.To, 0));
                        if (!writeFlag)
                            return;
                        i = cacheItem.To;
                    }
                    else
                        i = await DownloadBarsInternal(s => buffer.WriteAsync(ToInfo(s)), key, i, cacheItem.From);
                }

                if (i < to)
                    i = await DownloadBarsInternal(s => buffer.WriteAsync(ToInfo(s)), key, i, to);

                buffer.Dispose();
            }
            catch (Exception ex)
            {
                buffer.SetFailed(ex);
            }
        }

        private SliceInfo ToInfo(BarStreamSlice slice)
        {
            return new SliceInfo(slice.From, slice.To, slice.Bars.Length);
        }

        //private async IEnumerable<Task<BarStreamSlice>> EnumerateBarsInternal(string symbol, TimeFrames timeFrame, BarPriceType priceType, DateTime from, DateTime to)
        //{
        //    var buffer = new AsyncBuffer<BarStreamSlice>(2);

        //    await buffer.Write(null);


        //for (var i = from; i < to;)
        //{
        //    Func<Task< BarStreamSlice>> nextSliceFunc = async ()=>
        //    {
        //        var cachedSlice = Cache.GetFirstBarSlice(symbol, timeFrame, priceType, i, to);

        //        if (cachedSlice != null && cachedSlice.From == i)
        //        {
        //            i = cachedSlice.To;
        //            return new BarStreamSlice(cachedSlice.From, cachedSlice.To, cachedSlice.Content.ToList());
        //        }
        //        else
        //        {
        //            // download
        //            var slice = await DownloadBarSlice(symbol, timeFrame, priceType, i, SliceMaxSize);
        //            _diskCache.Put(symbol, timeFrame, priceType, i, slice.To, slice.Bars.ToArray());
        //            i = slice.To;
        //            return slice;
        //        }
        //    };

        //    yield return nextSliceFunc();
        //}
        //}

        //private IEnumerable<Task<SliceInfo>> DownloadBarsInternal(string symbol, TimeFrames timeFrame, BarPriceType priceType, DateTime from, DateTime to)
        //{
        //    const int chunkSize = 8000;

        //    for (var i = from; i < to;)
        //    {
        //        Func<Task<SliceInfo>> nextSliceFunc = async () =>
        //        {
        //            var cachedSlice = await Task.Factory.StartNew(() => Cache.GetFirstRange(symbol, timeFrame, priceType, i, to));

        //            if (cachedSlice != null && cachedSlice.From == i)
        //            {
        //                i = cachedSlice.To;
        //                return new SliceInfo(cachedSlice.From, cachedSlice.To, -1);
        //            }
        //            else
        //            {
        //                // download
        //                var slice = await DownloadBarSlice(symbol, timeFrame, priceType, i, chunkSize);
        //                _diskCache.Put(symbol, timeFrame, priceType, i, slice.To, slice.Bars.ToArray());
        //                i = slice.To;
        //                return new SliceInfo(slice.From, slice.To, slice.Bars.Count);
        //            }
        //        };

        //        yield return nextSliceFunc();
        //    }
        //}

        //private async Task<BarStreamSlice> DownloadBarSlice(string symbol, TimeFrames timeFrame, BarPriceType priceType, DateTime startTime, int count)
        //{
        //    var algoBars = await connection.FeedProxy.DownloadBars(symbol, startTime, count, priceType, timeFrame);
        //    if (count < 0)
        //        algoBars.Reverse();

        //    var from = algoBars.First().OpenTime;
        //    var to =  algoBars.Last().CloseTime;

        //    //var toCorrected = result.To.Value;

        //    //if (algoBars.Count > 0 && algoBars.Last().OpenTime == result.To)
        //    //{
        //    //    var sampler = BarSampler.Get(timeFrame);
        //    //    var barBoundaries = sampler.GetBar(result.To.Value);
        //    //    toCorrected = barBoundaries.Close;
        //    //}

        //    return new BarStreamSlice(from, to, algoBars);
        //}

        //private IEnumerable<Task<SliceInfo>> DownloadTicksInternal(string symbol, TimeFrames timeFrame, DateTime from, DateTime to)
        //{
        //    var includeLevel2 = timeFrame == TimeFrames.TicksLevel2;

        //    for (var i = to; i > from;)
        //    {
        //        Func<Task < SliceInfo>> nextTask = async ()=>
        //        {
        //            var cachedSlice = Cache.GetLastRange(symbol, timeFrame, null, from, i);

        //            if (cachedSlice != null && cachedSlice.To == i)
        //            {
        //                i = cachedSlice.From;
        //                return new SliceInfo(cachedSlice.From, cachedSlice.To, -1);
        //            }
        //            else
        //            {
        //                 download
        //                var slices = await DownloadTickFile(symbol, i, includeLevel2).ConfigureAwait(false);
        //                if (slices == null || slices.Count == 0)
        //                {
        //                     end of data
        //                    var emptyInfo = new SliceInfo(from, i, 0);
        //                    i = from;
        //                    return emptyInfo;
        //                }

        //                int count = 0;
        //                foreach (var slice in slices)
        //                {
        //                    _diskCache.Put(symbol, timeFrame, slice);
        //                    count += slice.Content.Count;
        //                }
        //                var slicesFrom = slices.First().From;
        //                var info = new SliceInfo(slicesFrom, i, count);
        //                i = slicesFrom;
        //                return info;
        //            }
        //        };

        //        yield return nextTask();
        //    }
        //}

        //private async Task<List<Slice<DateTime, QuoteEntity>>> DownloadTickFile(string symbol, DateTime refTimePoint, bool includeLevel2)
        //{
        //    var proxy = connection.FeedProxy;
        //    var result = await proxy.DownloadTickFiles(symbol, Decrease(refTimePoint), includeLevel2);

        //    if (result == null || result.Files.Count == 0)
        //        return null;

        //    var fileFrom = result.From.Value;
        //    var fileTo = result.To.Value;

        //    var slicer = new TimeBasedSlicer<QuoteEntity>(fileFrom, fileTo, SliceMaxSize, q => q.Time);

        //    foreach (var fileBytes in result.Files)
        //    {
        //        var ticks = DeserializeTicksFile(fileBytes.ToArray(), includeLevel2);
        //        slicer.Write(ticks.Select(t => Convert(t, symbol)).ToArray());
        //    }

        //    return slicer;
        //}

        //private List<TT.TickValue> DeserializeTicksFile(byte[] fileBytes, bool level2)
        //{
        //    var filename = level2 ? "ticks level2" : "ticks";
        //    IFormatter<TT.TickValue> formatter = level2 ? (IFormatter<TT.TickValue>)FeedTickLevel2Formatter.Instance : FeedTickFormatter.Instance;
        //    var serializer = new ItemsZipSerializer<TT.TickValue, List<TT.TickValue>>(formatter, filename);
        //    return serializer.Deserialize(fileBytes);
        //}

        //private static QuoteEntity Convert(TT.TickValue tick, string symbol)
        //{
        //    var bids = tick.Level2.Where(l => l.Type == TickTrader.Common.Business.FxPriceType.Bid)
        //        .Select(l => new BookEntryEntity((double)l.Price, l.Volume))
        //        .ToArray();
        //    var asks = tick.Level2.Where(l => l.Type == TickTrader.Common.Business.FxPriceType.Ask)
        //        .Select(l => new BookEntryEntity((double)l.Price, l.Volume))
        //        .ToArray();

        //    return new QuoteEntity(symbol, tick.Time, bids, asks);
        //}

        //private DateTime Decrease(DateTime val)
        //{
        //    return new DateTime(val.Ticks - 1);
        //}
    }

    internal class TimeBasedSlicer<T> : List<Slice<DateTime, T>>
    {
        private DateTime _from;
        private DateTime _to;
        private int _size;
        private Func<T, DateTime> _timeSelector;

        public TimeBasedSlicer(DateTime from, DateTime to, int maxSize, Func<T, DateTime> timeSelector)
        {
            _from = from;
            _to = to;
            _size = maxSize;
            _timeSelector = timeSelector;
        }

        public void Write(T[] data)
        {
            for (int i = 0; i < data.Length;)
                i += Write(new ArraySegment<T>(data, i, data.Length - i));
        }

        protected Slice<DateTime, T> LastSlice
        {
            get => this[Count - 1];
            set => this[Count - 1] = value;
        }

        private int Write(ArraySegment<T> data)
        {
            if (data.Count <= 0)
                return 0;

            if (Count > 0)
            {
                if (LastSlice.Content.Count < _size)
                    return FillSlice(LastSlice, data);

                AdjustLastSliceEnd(_timeSelector(data.First()));
            }

            return AddNewSlice(data);
        }

        private void AdjustLastSliceEnd(DateTime newTo)
        {
            var last = LastSlice;
            LastSlice = last.ChangeBounds(last.From, newTo);
        }

        private int AddNewSlice(ArraySegment<T> data)
        {
            int toCopy = Math.Min(_size, data.Count);
            var content = new ArraySegment<T>(data.Array, data.Offset, toCopy);
            //var lastItemTime = _timeSelector(content.Last());
            var slice = Slice<DateTime, T>.Create(new KeyRange<DateTime>(_from, _to), _timeSelector, content);
            Add(slice);
            return toCopy;
        }

        private int FillSlice(Slice<DateTime, T> sliceToFill, ArraySegment<T> data)
        {
            int space = _size - sliceToFill.Content.Count;
            int toCopy = Math.Min(space, data.Count);
            var content = CombineArrays(sliceToFill.Content, new ArraySegment<T>(data.Array, data.Offset, toCopy));
            //var lastItemTime = _timeSelector(content.Last());
            this[Count - 1] = Slice<DateTime, T>.Create(new KeyRange<DateTime>(sliceToFill.From, _to), _timeSelector, content);
            return toCopy;
        }

        private T[] CombineArrays(ArraySegment<T> a1, ArraySegment<T> a2)
        {
            var result = new T[a1.Count +  a2.Count];

            Array.Copy(a1.Array, a1.Offset, result, 0, a1.Count);
            Array.Copy(a2.Array, a2.Offset, result, a1.Count, a2.Count);

            return result;
        }
    }

    public class SliceInfo
    {
        public SliceInfo(DateTime from, DateTime to, int count)
        {
            From = from;
            To = to;
            Count = count;
        }

        public int Count { get; }
        public DateTime From { get; }
        public DateTime To { get; }
    }

    public class BarStreamSlice
    {
        public BarStreamSlice(DateTime from, DateTime to, BarEntity[] barList)
        {
            From = from;
            To = to;
            Bars = barList;
        }

        public DateTime From { get; }
        public DateTime To { get; }
        public BarEntity[] Bars { get; }
    }

    public class TickStreamSlice
    {
        public TickStreamSlice(DateTime from, DateTime to, List<QuoteEntity> ticksList)
        {
            From = from;
            To = to;
            Ticks = ticksList;
        }

        public DateTime From { get; }
        public DateTime To { get; }
        public List<QuoteEntity> Ticks { get; }
    }

    public enum FeedHistoryFolderOptions
    {
        NoHierarchy, // places history right into specified folder
        ServerHierarchy, // creates subfolder for server
        ServerClientHierarchy // creates subfolder for server and nested subfolder for client.
    }
}
