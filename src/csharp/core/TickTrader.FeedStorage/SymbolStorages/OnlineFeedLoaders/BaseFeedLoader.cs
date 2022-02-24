using ActorSharp;
using System;
using System.Threading.Tasks;
using TickTrader.FeedStorage.Api;

namespace TickTrader.FeedStorage
{
    internal partial class OnlineFeedStorage
    {
        private interface IFeedLoader
        {
            Task<DateTime> DownloadData(ActorChannel<ISliceInfo> buffer, FeedCacheKey key, DateTime from, DateTime to);
        }


        private abstract class BaseFeedLoader<T> : IFeedLoader
        {
            protected const int SliceMaxSize = 4000;

            protected readonly OnlineFeedStorage _storage;


            protected BaseFeedLoader(OnlineFeedStorage storage)
            {
                _storage = storage;
            }


            protected abstract void DownloadData(BlockingChannel<T> channel, FeedCacheKey key, DateTime from, DateTime to);

            protected abstract TimeSlicer<T> GetTimeSlicer(DateTime from, DateTime to);

            protected abstract void WriteSlice(FeedCacheKey key, Slice<T> slice);


            public async Task<DateTime> DownloadData(ActorChannel<ISliceInfo> buffer, FeedCacheKey key, DateTime from, DateTime to)
            {
                var inputStream = ActorChannel.NewInput<T>();
                var barSlicer = GetTimeSlicer(from, to);

                var hasData = false;

                try
                {
                    DownloadData(_storage.CreateBlockingChannel(inputStream), key, from, to);

                    var i = from;
                    while (await inputStream.ReadNext())
                    {
                        if (barSlicer.Write(inputStream.Current))
                        {
                            var slice = barSlicer.CompleteSlice(false);

                            WriteSlice(key, slice);

                            hasData = true;

                            if (!await buffer.Write(slice))
                                throw new TaskCanceledException();

                            i = slice.To;
                        }
                    }

                    var lastSlice = barSlicer.CompleteSlice(true);

                    if (lastSlice != null)
                    {
                        WriteSlice(key, lastSlice);
                        hasData = true;

                        if (!await buffer.Write(lastSlice))
                            throw new TaskCanceledException();

                        i = lastSlice.To;
                    }

                    if (!hasData)
                    {
                        WriteSlice(key, new Slice<T>(from, to, new T[0]));
                        return to;
                    }

                    return i;
                }
                finally
                {
                    await inputStream.Close();
                }
            }
        }
    }
}
