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
        private List<TRow> dataCache = new List<TRow>();
        private int virtualPos;

        public AlgoContext()
        {
        }

        public IAlgoDataReader<TRow> Reader { get; set; }
        public IAlgoDataWriter<TRow> Writer { get; set; }

        public void SetParameter(string paramId, object value)
        {
            parameters[paramId] = value;
        }

        public void Init()
        {
            if (Writer != null)
                Writer.Init(dataCache);

            if (Reader == null)
                throw new InvalidOperationException("Data Reader is not set!");
        }

        //public int Read()
        //{
        //    int oldSize = dataCache.Count;
        //    dataCache.AddRange(Reader.ReadNext());
        //    return dataCache.Count - oldSize;
        //}

        public void MoveNext(TRow row)
        {
            dataCache.Add(row);

            //if (virtualPos >= dataCache.Count)
            //throw new InvalidOperationException("Virtual position cannot be out of buffer boundaries.");

            Writer.Extend(row);

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

        public IDataSeriesBuffer BindInput(string id, InputFactory factory)
        {
            if (Reader == null)
                throw new InvalidOperationException("Data Reader is not set!");

            var input = Reader.BindInput(id, factory);
            inputs.Add(input);
            return input;
        }

        public IDataSeriesBuffer BindOutput(string id, OutputFactory factory)
        {
            if (Writer == null)
                throw new InvalidOperationException("Data Writer is not set!");

            var output = Writer.BindOutput(id, factory);
            outputs.Add(output);
            return output;
        }
    }
}
