using Machinarium.State;
using SoftFX.Extended;
using SoftFX.Extended.Storage;
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
            return Task.Factory.StartNew(() =>
            {
                if (timeFrame != TimeFrames.Ticks)
                {
                    var result = connection.FeedProxy.Server.GetHistoryBars(symbol, DateTime.Now, 1, FdkToAlgo.Convert(priceType), FdkToAlgo.ToBarPeriod(timeFrame));

                    return new Tuple<DateTime, DateTime>(result.FromAll, result.ToAll);
                }
                else
                    throw new Exception("Ticks is not supported!");
            });
        }

        public Task<List<BarEntity>> GetBarSlice(string symbol, BarPriceType priceType, TimeFrames timeFrame, DateTime startTime, int count)
        {
            return Task.Factory.StartNew(() =>
            {
                var slice = DownloadBarSlice(symbol, timeFrame, priceType, startTime, count);
                _diskCache.Put(symbol, timeFrame, priceType, slice.From, slice.To, slice.Bars.ToArray());
                return slice.Bars;
            });
        }

        public IAsyncEnumerator<BarStreamSlice> EnumerateBars(string symbol, TimeFrames timeFrame, BarPriceType priceType, DateTime from, DateTime to)
        {
            return EnumerateBarsInternal(symbol, timeFrame, priceType, from, to).ToAsyncEnumerator();
        }

        private IEnumerable<Task<BarStreamSlice>> EnumerateBarsInternal(string symbol, TimeFrames timeFrame, BarPriceType priceType, DateTime from, DateTime to)
        {
            for (var i = from; i < to;)
            {
                yield return Task.Factory.StartNew(() =>
                {
                    var cachedSlice = Cache.GetFirstBarSlice(symbol, timeFrame, priceType, i, to);

                    if (cachedSlice != null && cachedSlice.From == i)
                    {
                        i = cachedSlice.To;
                        return new BarStreamSlice(cachedSlice.From, cachedSlice.To, cachedSlice.Content.ToList());
                    }
                    else
                    {
                        // download
                        var slice = DownloadBarSlice(symbol, timeFrame, priceType, i, SliceMaxSize);
                        _diskCache.Put(symbol, timeFrame, priceType, i, slice.To, slice.Bars.ToArray());
                        i = slice.To;
                        return slice;
                    }
                });
            }
        }

        private IEnumerable<Task<SliceInfo>> DownloadBarsInternal(string symbol, TimeFrames timeFrame, BarPriceType priceType, DateTime from, DateTime to)
        {
            const int chunkSize = 8000;

            for (var i = from; i < to;)
            {
                yield return Task.Factory.StartNew(() =>
                {
                    var cachedSlice = Cache.GetFirstRange(symbol, timeFrame, priceType, i, to);

                    if (cachedSlice != null && cachedSlice.From == i)
                    {
                        i = cachedSlice.To;
                        return new SliceInfo(cachedSlice.From, cachedSlice.To, -1);
                    }
                    else
                    {
                        // download
                        var slice = DownloadBarSlice(symbol, timeFrame, priceType, i, chunkSize);
                        _diskCache.Put(symbol, timeFrame, priceType, i, slice.To, slice.Bars.ToArray());
                        i = slice.To;
                        return new SliceInfo(slice.From, slice.To, slice.Bars.Count);
                    }
                });
            }
        }

        public IAsyncEnumerator<SliceInfo> DownloadBarSeriesToStorage(string symbol, TimeFrames timeFrame, BarPriceType priceType, DateTime from, DateTime to)
        {
            return DownloadBarsInternal(symbol, timeFrame, priceType, from, to).ToAsyncEnumerator();
        }

        private BarStreamSlice DownloadBarSlice(string symbol, TimeFrames timeFrame, BarPriceType priceType, DateTime startTime, int count)
        {
            var result = connection.FeedProxy.Server.GetHistoryBars(symbol, startTime, count, FdkToAlgo.Convert(priceType), FdkToAlgo.ToBarPeriod(timeFrame));
            var algoBars = FdkToAlgo.Convert(result.Bars, count < 0);

            var toCorrected = result.To.Value;

            if (algoBars.Count > 0 && algoBars.Last().OpenTime == result.To)
            {
                var sampler = BarSampler.Get(timeFrame);
                var barBoundaries = sampler.GetBar(result.To.Value);
                toCorrected = barBoundaries.Close;
            }

            return new BarStreamSlice(result.From.Value, toCorrected, algoBars);
        }

        //private IEnumerable<Task<TickStreamSlice>> EnumerateTicksInternal()
        //{

        //}

        public IAsyncEnumerator<SliceInfo> DownloadTickSeriesToStorage(string symbol, TimeFrames timeFrame, DateTime from, DateTime to)
        {
            return DownloadTicksInternal(symbol, timeFrame, from, to).ToAsyncEnumerator();
        }

        private IEnumerable<Task<SliceInfo>> DownloadTicksInternal(string symbol, TimeFrames timeFrame, DateTime from, DateTime to)
        {
            var includeLevel2 = timeFrame == TimeFrames.TicksLevel2;

            for (var i = to; i > from;)
            {
                yield return Task.Factory.StartNew(() =>
                {
                    var cachedSlice = Cache.GetLastRange(symbol, timeFrame, null, from, i);

                    if (cachedSlice != null && cachedSlice.To == i)
                    {
                        i = cachedSlice.From;
                        return new SliceInfo(cachedSlice.From, cachedSlice.To, -1);
                    }
                    else
                    {
                        // download
                        var slices = DownloadTickFile(symbol, i, includeLevel2);
                        if (slices == null || slices.Count == 0)
                        {
                            // end of data
                            var emptyInfo = new SliceInfo(from, i, 0);
                            i = from;
                            return emptyInfo;
                        }

                        int count = 0;
                        foreach (var slice in slices)
                        {
                            _diskCache.Put(symbol, timeFrame, slice);
                            count += slice.Content.Count;
                        }
                        var slicesFrom = slices.First().From;
                        var info = new SliceInfo(slicesFrom, i, count);
                        i = slicesFrom;
                        return info;
                    }
                });
            }
        }

        private List<Slice<DateTime, QuoteEntity>> DownloadTickFile(string symbol, DateTime refTimePoint, bool includeLevel2)
        {
            var proxy = connection.FeedProxy;
            var result = proxy.Server.GetQuotesHistoryFiles(symbol, includeLevel2, Decrease(refTimePoint));

            if (result == null || result.Files.Length == 0)
                return null;

            var fileFrom = result.From.Value;
            var fileTo = result.To.Value;

            var slicer = new TimeBasedSlicer<QuoteEntity>(fileFrom, fileTo, SliceMaxSize, q => q.Time);

            foreach (var fileCode in result.Files)
            {
                var filename = includeLevel2 ? "ticks level2" : "ticks";
                IFormatter<TT.TickValue> formatter = includeLevel2 ? (IFormatter<TT.TickValue>)FeedTickLevel2Formatter.Instance : FeedTickFormatter.Instance;
                var serializer = new ItemsZipSerializer<TT.TickValue, List<TT.TickValue>>(formatter, filename);

                using (var downloadStream = new DataStream(proxy, fileCode))
                {
                    var bytes = downloadStream.ToArray();
                    var fdkTicks = serializer.Deserialize(bytes);
                    slicer.Write(fdkTicks.ToAlgo(symbol).ToArray());
                }
            }

            return slicer;
        }

        private DateTime Decrease(DateTime val)
        {
            return new DateTime(val.Ticks - 1);
        }
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
        public BarStreamSlice(DateTime from, DateTime to, List<BarEntity> barList)
        {
            From = from;
            To = to;
            Bars = barList;
        }

        public DateTime From { get; }
        public DateTime To { get; }
        public List<BarEntity> Bars { get; }
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
