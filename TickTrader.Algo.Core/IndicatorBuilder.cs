using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Metadata;

namespace TickTrader.Algo.Core
{
    public class IndicatorBuilder<TRow> : IIndicatorBuilder
    {
        private IndicatorContext fixture;
        private int readPos;

        public IndicatorBuilder(Type algoLocalDomainType, IAlgoDataReader<TRow> reader, IAlgoDataWriter<TRow> writer)
            : this(AlgoPluginDescriptor.Get(algoLocalDomainType), reader, writer)
        {
        }

        public IndicatorBuilder(AlgoPluginDescriptor descriptor, IAlgoDataReader<TRow> reader, IAlgoDataWriter<TRow> writer)
        {
            if (descriptor == null)
                throw new ArgumentNullException("descriptor");

            if (descriptor.AlgoLogicType != AlgoTypes.Indicator)
                throw new InvalidPluginType("Supplied descriptor is not of Indicator type!");
           
            if (reader == null)
                throw new ArgumentNullException("reader");

            if (writer == null)
                throw new ArgumentNullException("writer");

            this.Reader = reader;
            this.Writer = writer;

            fixture = IndicatorContext.CreateIndicator(descriptor.Id);

            Init();
        }

        protected IAlgoDataReader<TRow> Reader { get; private set; }
        protected IAlgoDataWriter<TRow> Writer { get; private set; }

        private void Init()
        {
            if (Reader == null)
                throw new InvalidOperationException("Data Reader is not set!");

            foreach (var inputDescriptor in fixture.Descriptor.Inputs)
                Reader.BindInput(inputDescriptor.Id, fixture.GetInput(inputDescriptor.Id));

            foreach (var outputDescriptor in fixture.Descriptor.Outputs)
                Writer.BindOutput(outputDescriptor.Id, fixture.GetOutput(outputDescriptor.Id));

            fixture.InvokeInit();
        }

        public void Build()
        {
            Build(CancellationToken.None);
        }

        public void RebuildLast()
        {
            ReRead();
            fixture.InvokeCalculate();
        }

        public void Build(CancellationToken cToken)
        {
            int count;

            do
            {
                count = Read();

                for (int i = 0; i < count; i++)
                {
                    if (cToken.IsCancellationRequested)
                        return;

                    MoveNext();
                    fixture.InvokeCalculate();

                    if (cToken.IsCancellationRequested)
                        break;
                }
            }
            while (count != 0);
        }

        private int Read()
        {
            var buff = Reader.ReadAt(readPos, 100);

            Writer.Extend(buff);

            readPos += buff.Count;

            return buff.Count;
        }

        private void ReRead()
        {
            var row = Reader.ReadAt(readPos);
            Writer.UpdateLast(row);
        }

        private void MoveNext()
        {
            if (fixture.VirtualPos >= readPos)
                throw new InvalidOperationException("End of buffer!");

            fixture.MoveNext();
        }

        public void SetParameter(string id, object val)
        {
            fixture.SetParameter(id, val);
        }

        public void Reset()
        {
            fixture.Reset();

            Reader.Reset();
            Writer.Reset();

            readPos = 0;
        }
    }
}
