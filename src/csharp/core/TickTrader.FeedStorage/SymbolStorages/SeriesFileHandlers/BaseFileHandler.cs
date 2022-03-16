using ActorSharp;
using System;
using System.IO;
using System.Threading.Tasks;
using TickTrader.FeedStorage.Api;

namespace TickTrader.FeedStorage.StorageBase
{
    internal interface IFileHandler
    {
        Task ExportSeries(ActorChannel<ISliceInfo> buffer);
    }


    internal abstract class BaseFileHandler<T> : IFileHandler
    {
        private readonly FeedStorageBase _storage;
        private readonly IExportSeriesSettings _settings;

        protected readonly BaseFileFormatter _formatter;
        protected readonly FeedCacheKey _key;

        protected readonly string _timeFormat;
        protected readonly char _separator;

        protected StreamWriter _writer;


        protected BaseFileHandler(FeedStorageBase storage, BaseFileFormatter formatter, FeedCacheKey key, IExportSeriesSettings settings)
        {
            _key = key;
            _storage = storage;
            _formatter = formatter;
            _settings = settings;
            _separator = settings.Separator;
            _timeFormat = settings.TimeFormat;

            _formatter.Separator = settings.Separator;
        }


        protected abstract void WriteSlice(ArraySegment<T> values);

        protected abstract void PreloadLogic(StreamWriter writer);

        protected abstract void PostloadLogic(StreamWriter writer);


        public async Task ExportSeries(ActorChannel<ISliceInfo> buffer)
        {
            var from = _settings.From.ToUniversalTime();
            var to = _settings.To.ToUniversalTime();
            var hasData = false;

            try
            {
                using (_writer = new StreamWriter(File.Open(_settings.FilePath, FileMode.Create)))
                {
                    PreloadLogic(_writer);

                    foreach (var slice in _storage.GetSeries<T>(_key)?.IterateSlices(from, to))
                    {
                        hasData = true;
                        WriteSlice(slice.Content);

                        if (!await buffer.Write(new SliceInfo(slice.From, slice.To, slice.Content.Count)))
                            throw new TaskCanceledException();
                    }

                    PostloadLogic(_writer);
                }
            }
            catch (Exception ex)
            {
                await buffer.Close(ex);
            }
            finally
            {
                await buffer.Close();

                if (!hasData)
                    File.Delete(_settings.FilePath);
            }
        }
    }
}
