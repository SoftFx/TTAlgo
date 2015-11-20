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
        private AlgoContext<TRow> conext = new AlgoContext<TRow>();
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

            conext.Reader = reader;
            conext.Writer = writer;
        }

        public void SetParameter(string id, object val)
        {
            conext.SetParameter(id, val);
        }

        public void ReadAllAndBuild()
        {
            ReadAllAndBuild(CancellationToken.None);
        }

        public void ReadAllAndBuild(CancellationToken cToken)
        {
            if (insatnceProxy != null)
                throw new InvalidOperationException("Indicator has been already built!");

            insatnceProxy = factory(conext);

            int count;

            do
            {
                count = insatnceProxy.Context.Read();

                for (int i = 0; i < count; i++)
                {
                    if (cToken.IsCancellationRequested)
                        return;

                    insatnceProxy.Context.MoveNext();
                    insatnceProxy.InvokeCalculate();

                    if (cToken.IsCancellationRequested)
                        break;
                }
            }
            while (count != 0);
        }
    }
}
