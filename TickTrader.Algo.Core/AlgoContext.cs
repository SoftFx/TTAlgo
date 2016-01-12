using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Core.Metadata;

namespace TickTrader.Algo.Core
{
    [Serializable]
    public class AlgoContext<TRow> : NoTimeoutByRefObject, IAlgoContext
    {
        private Dictionary<string, object> parameters = new Dictionary<string, object>();
        private List<IDataSeriesBuffer> inputs = new List<IDataSeriesBuffer>();
        private List<IDataSeriesBuffer> outputs = new List<IDataSeriesBuffer>();
        private int virtualPos;
        private int readPos;

        public AlgoContext()
        {
        }

        public IAlgoDataReader<TRow> Reader { get; set; }
        public IAlgoDataWriter<TRow> Writer { get; set; }

        internal int VirtualPos { get { return virtualPos; } }
        internal int ReadPos { get { return readPos; } }

        public void SetParameter(string paramId, object value)
        {
            parameters[paramId] = value;
        }

        public void Init()
        {
            //if (Writer != null)
                //Writer.Init(dataCache);

            if (Reader == null)
                throw new InvalidOperationException("Data Reader is not set!");
        }

        public void ReRead()
        {
            int lastIndex = ReadPos;
            var row = Reader.ReadAt(lastIndex);
            Writer.UpdateLast(row);
        }

        public int Read()
        {
            var buff = Reader.ReadAt(ReadPos, 100);

            Writer.Extend(buff);

            readPos += buff.Count;

            return buff.Count;
        }

        public void MoveNext()
        {
            if (virtualPos >= ReadPos)
                throw new InvalidOperationException("End of buffer!");

            foreach (var input in inputs)
                input.IncrementVirtualSize();

            foreach (var output in outputs)
                output.IncrementVirtualSize();

            virtualPos++;
        }

        public object GetParameter(string id)
        {
            object paramVal;
            if (!parameters.TryGetValue(id, out paramVal))
                throw new InvalidOperationException("Parameter '" + id + "' is not set!");
            return paramVal;
        }

        public void BindInput<T>(string id, InputDataSeries<T> buffer)
        {
            if (Reader == null)
                throw new InvalidOperationException("Data Reader is not set!");

            inputs.Add(buffer);
            Reader.BindInput<T>(id, buffer);
        }

        public void BindOutput<T>(string id, OutputDataSeries<T> buffer)
        {
            if (Writer == null)
                throw new InvalidOperationException("Data Writer is not set!");

            outputs.Add(buffer);
            Writer.BindOutput(id, buffer);
        }
    }
}
