using ActorSharp;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TickTrader.FeedStorage.Api;

namespace TickTrader.FeedStorage.StorageBase
{
    internal interface IFileHandler
    {
        Task ExportSeries(ActorChannel<ISliceInfo> buffer, IExportSeriesSettings settings);

        Task ImportSeries(ActorChannel<ISliceInfo> buffer, IImportSeriesSettings settings);
    }


    internal abstract class BaseFileHandler<T> : IFileHandler
    {
        private const int PageSize = 4000;

        protected readonly BaseFileFormatter _formatter;
        protected readonly FeedStorageBase _storage;
        protected readonly FeedCacheKey _key;

        protected readonly string _timeFormat;
        protected readonly char _separator;

        protected StreamWriter _writer;
        protected StreamReader _reader;


        protected abstract ICollection<T> Vector { get; }


        protected BaseFileHandler(FeedStorageBase storage, BaseFileFormatter formatter, FeedCacheKey key, IBaseFileSeriesSettings settings)
        {
            _key = key;
            _storage = storage;
            _formatter = formatter;
            _separator = settings.Separator;
            _timeFormat = settings.TimeFormat;

            _formatter.Separator = settings.Separator;
        }


        protected abstract T ReadSlice(string fileLine, int lineNumber);

        protected abstract Task WritePageToStorage(ActorChannel<ISliceInfo> buffer, T[] values);

        protected abstract void WriteSliceToStream(ArraySegment<T> values);


        protected abstract void PreloadLogic(StreamWriter writer);

        protected abstract void PostloadLogic(StreamWriter writer);


        public async Task ExportSeries(ActorChannel<ISliceInfo> buffer, IExportSeriesSettings settings)
        {
            var from = settings.From.ToUniversalTime();
            var to = settings.To.ToUniversalTime();
            var hasData = false;

            try
            {
                using (_writer = new StreamWriter(File.Open(settings.FilePath, FileMode.Create)))
                {
                    PreloadLogic(_writer);

                    foreach (var slice in _storage.GetSeries<T>(_key)?.IterateSlices(from, to))
                    {
                        hasData |= slice.Content.Count > 0;
                        WriteSliceToStream(slice.Content);

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
                    File.Delete(settings.FilePath);
            }
        }


        public async Task ImportSeries(ActorChannel<ISliceInfo> buffer, IImportSeriesSettings settings)
        {
            int lineCnt = 0;

            try
            {
                Vector.Clear();

                using (_reader = new StreamReader(settings.FilePath))
                {
                    while (!_reader.EndOfStream)
                    {
                        if (settings.SkipHeaderLine && ++lineCnt == 1)
                        {
                            _reader.ReadLine(); // skip header line
                            continue;
                        }

                        Vector.Add(ReadSlice(_reader.ReadLine(), lineCnt));

                        if (Vector.Count == PageSize)
                        {
                            await WritePageToStorage(buffer, Vector.ToArray());
                            Vector.Clear();
                        }
                    }

                    if (Vector.Count > 0)
                        await WritePageToStorage(buffer, Vector.ToArray());
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

        protected DateTime ParseDate(string str)
        {
            return DateTime.SpecifyKind(DateTime.ParseExact(str, _timeFormat, CultureInfo.InvariantCulture), DateTimeKind.Utc);
        }

        protected void ThrowFormatError(int lineNumber) => throw new Exception($"Invalid format at line {lineNumber}");
    }
}