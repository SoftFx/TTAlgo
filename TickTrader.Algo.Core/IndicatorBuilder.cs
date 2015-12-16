using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Metadata;

namespace TickTrader.Algo.Core
{
    public class IndicatorBuilder<TRow>
    {
        private AlgoContext<TRow> context = new AlgoContext<TRow>();
        private Func<IAlgoContext, IndicatorProxy> factory;
        private IndicatorProxy insatnceProxy;

        public IndicatorBuilder(Type algoCustomType, IAlgoDataReader<TRow> reader, IAlgoDataWriter<TRow> writer)
            : this(c => new IndicatorProxy(AlgoDescriptor.Get(algoCustomType), c), reader, writer)
        {
        }

        public IndicatorBuilder(AlgoDescriptor descriptor, IAlgoDataReader<TRow> reader, IAlgoDataWriter<TRow> writer)
            : this(c => new IndicatorProxy(descriptor, c), reader, writer)
        {
        }

        public IndicatorBuilder(Func<IAlgoContext, IndicatorProxy> factory, IAlgoDataReader<TRow> reader, IAlgoDataWriter<TRow> writer)
        {
            if (factory == null)
                throw new ArgumentNullException("factory");

            if (reader == null)
                throw new ArgumentNullException("reader");

            if (writer == null)
                throw new ArgumentNullException("writer");

            this.factory = factory;

            context.Reader = reader;
            context.Writer = writer;

            reader.Appended += Reader_Appended;
            reader.AppendedRange += Reader_AppendedRange;
            reader.Initialized += Init;
        }

        private void Init()
        {
            if (insatnceProxy == null)
                insatnceProxy = factory(context);

            context.Init();
        }

        private void Reader_Appended(TRow row)
        {
            BuildOne(row);
        }

        private void Reader_AppendedRange(IEnumerable<TRow> rows)
        {
            foreach (var row in rows)
                BuildOne(row);
        }

        public void SetParameter(string id, object val)
        {
            context.SetParameter(id, val);
        }

        private void BuildOne(TRow row)
        {
            context.MoveNext(row);
            insatnceProxy.InvokeCalculate();
        }
    }
}
