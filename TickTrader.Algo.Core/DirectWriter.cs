using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Core
{
    public class DirectWriter<TRow> : IAlgoDataWriter<TRow>
    {
        private Dictionary<string, ICollectionWriter> writers = new Dictionary<string, ICollectionWriter>();

        public DirectWriter()
        {
        }

        public void Extend(TRow row)
        {
            foreach (var writer in writers.Values)
                writer.Extend();
        }

        public void AddMapping<T>(string outputId, CollectionWriter<T, TRow> targetCollection)
        {
            writers.Add(outputId, new CollectionWriter<T>(targetCollection));
        }

        public void AddMapping<T>(string outputId, List<T> targetCollection)
        {
            writers.Add(outputId, new CollectionWriter<T>(new ListWriter<T>(targetCollection)));
        }

        public void AddMapping<T, TList>(string outputId, List<TList> targetCollection, Func<TRow, T, TList> selector)
        {
            writers.Add(outputId, new CollectionWriter<T>(new ListWriter<T, TList>(targetCollection, selector)));
        }

        public IDataSeriesBuffer BindOutput(string id, OutputFactory factory)
        {
            ICollectionWriter writer;
            if (!writers.TryGetValue(id, out writer))
                throw new Exception("Output '" + id + "' is not mapped.");
            return writer.CreateProxy(factory);
        }


        public void Init(IList<TRow> inputCache)
        {
            foreach (var writer in writers.Values)
                writer.InputData = inputCache;
        }

        private interface ICollectionWriter
        {
            void Extend();
            IList<TRow> InputData { get; set; }
            IDataSeriesBuffer CreateProxy(OutputFactory factory);
        }

        private class CollectionWriter<T> : MarshalByRefObject, ICollectionWriter
        {
            private CollectionWriter<T, TRow> target;
            private OutputDataSeries<T> outputProxy;

            public CollectionWriter(CollectionWriter<T, TRow> targetCollection)
            {
                this.target = targetCollection;
            }

            public void Extend()
            {
                outputProxy.AppendEmpty();
            }

            public IList<TRow> InputData { get; set; }

            public IDataSeriesBuffer CreateProxy(OutputFactory factory)
            {
                outputProxy = factory.CreateOutput<T>();
                outputProxy.Updated += (d, i) => target.WriteAt(i, d, InputData[i]);
                outputProxy.Appended += (d, i) => target.Append(InputData[i], d);
                return outputProxy;
            }
        }

        private class ListWriter<T, TList> : CollectionWriter<T, TRow>
        {
            private IList<TList> list;
            private Func<TRow, T, TList> selector;

            public ListWriter(IList<TList> list, Func<TRow, T, TList> selector)
            {
                this.list = list;
                this.selector = selector;
            }

            public void Append(TRow row, T data)
            {
                list.Add(selector(row, data));
            }

            public void WriteAt(int index, T data, TRow row)
            {
                list[index] = selector(row, data);
            }
        }

        private class ListWriter<T> : CollectionWriter<T, TRow>
        {
            private IList<T> list;

            public ListWriter(IList<T> list)
            {
                this.list = list;
            }

            public void Append(TRow row, T data)
            {
                list.Add(data);
            }

            public void WriteAt(int index, T data, TRow row)
            {
                list[index] = data;
            }
        }
    }
}
