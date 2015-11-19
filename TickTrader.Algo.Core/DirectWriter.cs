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
            foreach(var writer in writers.Values)
                writer.Extend();
        }

        public void AddMapping<T>(string outputId, CollectionWriter<T, TRow> targetCollection)
        {
            writers.Add(outputId, new CollectionWriter<T>(targetCollection));
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
    }
}
