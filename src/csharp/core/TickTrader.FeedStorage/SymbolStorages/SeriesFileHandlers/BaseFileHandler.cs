using ActorSharp;
using System;
using System.IO;
using System.Threading.Tasks;
using TickTrader.FeedStorage.Api;

namespace TickTrader.FeedStorage.StorageBase
{
    internal interface IFileHandler
    {
        Task ExportSeries(ActorChannel<ISliceInfo> buffer, FeedCacheKey key);
    }


    internal abstract class BaseFileHandler<T> : IFileHandler
    {
        private readonly FeedStorageBase _storage;
        private readonly BaseFileFormatter _formatter;
        private readonly IExportSeriesSettings _settings;

        protected readonly string _timeFormat;
        protected readonly char _separator;

        protected StreamWriter _writer;


        protected BaseFileHandler(FeedStorageBase storage, BaseFileFormatter formatter, IExportSeriesSettings settings)
        {
            _storage = storage;
            _formatter = formatter;
            _settings = settings;
            _separator = settings.Separator;
            _timeFormat = settings.TimeFormat;
        }


        protected abstract void WriteSlice(ArraySegment<T> values);


        public async Task ExportSeries(ActorChannel<ISliceInfo> buffer, FeedCacheKey key)
        {
            var from = _settings.From.ToUniversalTime();
            var to = _settings.To.ToUniversalTime();

            try
            {
                using (_writer = new StreamWriter(File.Open(_settings.FilePath, FileMode.Create)))
                {
                    _formatter.PreloadLogic(_writer);

                    foreach (var slice in _storage.GetSeries<T>(key)?.IterateSlices(from, to))
                    {
                        WriteSlice(slice.Content);

                        if (!await buffer.Write(new SliceInfo(slice.From, slice.To, slice.Content.Count)))
                            throw new TaskCanceledException();
                    }

                    _formatter.PostloadLogic(_writer);
                }
            }
            catch (Exception ex)
            {
                await buffer.Close(ex);
            }
            finally
            {
                await buffer.Close();
            }
        }
    }
}
