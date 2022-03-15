using ActorSharp;
using Google.Protobuf.WellKnownTypes;
using System;
using TickTrader.Algo.Domain;

namespace TickTrader.FeedStorage.StorageBase
{
    internal partial class OnlineFeedStorage
    {
        private sealed class BarFeedLoader : BaseFeedLoader<BarData>
        {
            public BarFeedLoader(OnlineFeedStorage storage) : base(storage)
            { }


            protected override void DownloadData(BlockingChannel<BarData> channel, FeedCacheKey key, DateTime from, DateTime to)
            {
                _storage._feedProvider.DownloadBars(channel, key.Symbol, from, to - TimeSpan.FromTicks(1), key.MarketSide.Value, key.TimeFrame);
            }

            protected override TimeSlicer<BarData> GetTimeSlicer(DateTime from, DateTime to)
            {
                return TimeSlicer.GetBarSlicer(SliceMaxSize, from, to);
            }

            protected override void WriteSlice(FeedCacheKey key, Slice<BarData> slice)
            {
                _storage.Put(key, slice.From, slice.To, slice.Items);
            }
        }
    }
}
