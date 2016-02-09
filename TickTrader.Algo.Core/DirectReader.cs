using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.Core
{
    public class DirectReader<TRow> : ReaderBase<TRow>, IAlgoDataReader<TRow>
    {
        private IList<TRow> srcCollection;

        public DirectReader(IList<TRow> srcCollection)
        {
            this.srcCollection = srcCollection;
        }

        public void AddMapping<TInput>(string inputId, Func<TRow, TInput> selector)
        {
            AddMapping(inputId, new Mapping<TInput>(selector));
        }

        public void AddMapping(string inputId, Func<TRow, double> selector)
        {
            AddMapping(inputId, new Mapping<double>(selector));
        }

        public TRow ReadAt(int index)
        {
            throw new NotImplementedException();
        }

        public List<TRow> ReadAt(int index, int pageSize)
        {
            var page = TakeAt(index, pageSize).ToList();

            foreach (TRow row in page)
            {
                foreach (var mapping in Mappings)
                    mapping.Append(row);
            }

            foreach (var mapping in Mappings)
                mapping.Flush();

            return page;
        }

        public void Reset()
        {
        }

        private IEnumerable<TRow> TakeAt(int index, int pageSize)
        {
            for (int size = 0; size < pageSize; size++)
            {
                if (index >= srcCollection.Count)
                    break;
                yield return srcCollection[index++];
            }
        }

        public void BindInput<T>(string id, InputDataSeries<T> buffer)
        {
            ((Mapping<T>)GetMappingOrThrow(id)).SetProxy(buffer);
        }
    }

    //public class StreamReader<TRow> : IAlgoDataReader<TRow>
    //{
    //    private StreamReader main;
    //    private List<StreamReader> additionalStreams = new List<StreamReader>();
        
    //    private int chunkSize;

    //    public MultiStreamReader(string mainId, InputStream<T> mainStream, int chunkSize = 100)
    //    {
    //        this.main = new StreamReader(mainId, mainStream);
    //        this.chunkSize = chunkSize;
    //    }

    //    public void AddMapping<TInput>(string inputId, string streamId, Func<T, TInput> selector)
    //    {
    //        GetStreamReaderOrThow(streamId).AddMapping(new Mapping<TInput>(selector, new InputDataSeries<TInput>()));
    //    }

    //    public void AddMapping(string inputId, string streamId, Func<T, double> selector)
    //    {
    //        GetStreamReaderOrThow(streamId).AddMapping(new Mapping<double>(selector, new InputDataSeries()));
    //    }

    //    public void AddStream(string id, InputStream<T> stream)
    //    {
    //        if (GetStreamReader(id) != null)
    //            throw new Exception("Stream with id=" + id + " has been already added.");

    //        additionalStreams.Add(new StreamReader(id, stream));
    //    }

    //    public int ReadNext()
    //    {
    //        List<Bar> buffer = new List<Bar>();
    //        while (buffer.Count < chunkSize)
    //        {
    //            T refRec;
    //            if (!main.ReadNext(out refRec))
    //                break;

    //            foreach (var stream in additionalStreams)
    //                stream.ReadNext(refRec);
    //        }

    //        return buffer.Count;
    //    }

    //    public void Update()
    //    {
    //    }

    //    public IDataSeriesBuffer GetInputBuffer(string id)
    //    {
    //        IDataSeriesBuffer buff;
    //        if (!inputBuffers.TryGetValue(id, out buff))
    //            throw new Exception("Input '" + id + "' is not mapped.");
    //        return buff;
    //    }

    //    private StreamReader GetStreamReaderOrThow(string id)
    //    {
    //        StreamReader reader = GetStreamReader(id);
    //        if (reader == null)
    //            throw new Exception("Cannot find stream: " + id);
    //        return reader;
    //    }

    //    private StreamReader GetStreamReader(string id)
    //    {
    //        if (main.StreamId == id)
    //            return main;
    //        return additionalStreams.FirstOrDefault(s => s.StreamId == id);
    //    }

    //    private class StreamReader
    //    {
    //        private InputStream<T> stream;
    //        private List<IMapping> mappings = new List<IMapping>();

    //        public StreamReader(string Id, InputStream<T> stream)
    //        {
    //            this.StreamId = Id;
    //            this.stream = stream;
    //        }

    //        public string StreamId { get; private set; }

    //        public void AddMapping(IMapping mp)
    //        {
    //            this.mappings.Add(mp);
    //        }

    //        public bool ReadNext(out T record)
    //        {
    //            T lolRecCopy;

    //            if (!stream.ReadNext(out lolRecCopy))
    //            {
    //                record = default(T);
    //                return false;
    //            }

    //            record = lolRecCopy;
    //            mappings.ForEach(m => m.Append(lolRecCopy));
    //            return true;
    //        }

    //        public void ReadNext(T refRecord)
    //        {
    //            T rec;

    //            if (!stream.ReadNext(refRecord, out rec))
    //                mappings.ForEach(m => m.AppendNan());
    //            else
    //                mappings.ForEach(m => m.Append(rec));
    //        }
    //    }

    //    private interface IMapping
    //    {
    //        void AppendNan();
    //        void Append(T rec);
    //        void Flush();
    //        IDataSeriesBuffer InputProxy { get; }
    //    }

    //    private class Mapping<TIn> : IMapping
    //    {
    //        private List<TIn> cacheBuff = new List<TIn>();
    //        private Func<T, TIn> selector;
    //        private TIn nanValue;
    //        private InputDataSeries<TIn> inputProxy;

    //        public Mapping(Func<T, TIn> selector, InputDataSeries<TIn> inputProxy, TIn nanValue = default(TIn))
    //        {
    //            this.selector = selector;
    //            this.inputProxy = inputProxy;
    //            this.nanValue = nanValue;
    //        }

    //        public void Append(T record)
    //        {
    //            cacheBuff.Add(selector(record));
    //        }

    //        public void AppendNan()
    //        {
    //            cacheBuff.Add(nanValue);
    //        }

    //        public void Flush()
    //        {
    //            inputProxy.Append(cacheBuff);
    //            cacheBuff.Clear();
    //        }

    //        public IDataSeriesBuffer InputProxy { get { return inputProxy; } }
    //    }
    //}
}
