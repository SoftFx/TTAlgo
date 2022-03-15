using ActorSharp;
using Google.Protobuf.WellKnownTypes;
using System;
using TickTrader.Algo.Domain;

namespace TickTrader.FeedStorage.StorageBase
{
    internal partial class OnlineFeedStorage
    {
        private sealed class TickFeedLoader : BaseFeedLoader<QuoteInfo>
        {
            public TickFeedLoader(OnlineFeedStorage storage) : base(storage)
            { }


            protected override void DownloadData(BlockingChannel<QuoteInfo> channel, FeedCacheKey key, DateTime from, DateTime to)
            {
                _storage._feedProvider.DownloadQuotes(channel, key.Symbol, from, to, key.TimeFrame == Feed.Types.Timeframe.TicksLevel2);
            }

            protected override TimeSlicer<QuoteInfo> GetTimeSlicer(DateTime from, DateTime to)
            {
                return TimeSlicer.GetQuoteSlicer(SliceMaxSize, from, to);
            }

            protected override void WriteSlice(FeedCacheKey key, Slice<QuoteInfo> slice)
            {
                _storage.Put(key, slice.From, slice.To, slice.Items);
            }
        }
    }
}
