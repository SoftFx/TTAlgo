using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Core
{
    public class DirectWriter<TRow> : IAlgoDataWriter<TRow>
    {
        private Dictionary<string, IMapping> mappings = new Dictionary<string, IMapping>();
        private List<TRow> inputData = new List<TRow>();

        public DirectWriter()
        {
        }

        public void Extend(List<TRow> rows)
        {
            inputData.AddRange(rows);
        }

        public void Reset()
        {
            inputData.Clear();
            foreach (var mapping in mappings.Values)
                mapping.Reset();
        }

        public void UpdateLast(TRow row)
        {
            inputData[inputData.Count - 1] = row;
        }

        public void AddMapping<T>(string outputId, CollectionWriter<T, TRow> targetCollection)
        {
            mappings.Add(outputId, new Mapping<T>(targetCollection));
        }

        public void AddMapping<T>(string outputId, List<T> targetCollection)
        {
            mappings.Add(outputId, new Mapping<T>(new ListWriter<T>(targetCollection)));
        }

        public void AddMapping<T, TList>(string outputId, List<TList> targetCollection, Func<TRow, T, TList> selector)
        {
            mappings.Add(outputId, new Mapping<T>(new ListWriter<T, TList>(targetCollection, selector)));
        }

        public void BindOutput<T>(string id, OutputDataSeries<T> buffer)
        {
            IMapping mapping;
            if (!mappings.TryGetValue(id, out mapping))
                throw new Exception("Output '" + id + "' is not mapped.");
            mapping.InputData = inputData;
            ((Mapping<T>)mapping).SetBuffer(buffer);
        }

        private interface IMapping
        {
            IList<TRow> InputData { get; set; }
            void Reset();
        }

        private class Mapping<T> : MarshalByRefObject, IMapping
        {
            private CollectionWriter<T, TRow> target;
            private OutputDataSeries<T> outputProxy;

            public Mapping(CollectionWriter<T, TRow> targetCollection)
            {
                this.target = targetCollection;
            }

            public void SetBuffer(OutputDataSeries<T> buffer)
            {
                outputProxy = buffer;
                outputProxy.Updated += (d, i) => target.WriteAt(i, d, InputData[i]);
                outputProxy.Appended += (d, i) => target.Append(InputData[i], d);
            }

            public IList<TRow> InputData { get; set; }

            public void Reset()
            {
                target.Reset();
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

            public void Reset()
            {
                list.Clear();
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

            public void Reset()
            {
                list.Clear();
            }
        }
    }
}
