using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Metadata;

namespace TickTrader.Algo.Core
{
    public class AlgoHost : IAlgoContext
    {
        private IndicatorProxy proxy;
        private Dictionary<string, object> parameters = new Dictionary<string, object>();
        private Dictionary<string, IManagedDataSeries> inputs = new Dictionary<string, IManagedDataSeries>();
        private Dictionary<string, IManagedDataSeries> outputs = new Dictionary<string, IManagedDataSeries>();

        public AlgoHost(IndicatorProxy proxy)
        {
            this.proxy = proxy;
        }

        public int Count { get; private set; }

        public void Extend()
        {
            Count++;

            foreach (var input in inputs.Values)
                input.Extend();

            foreach (var output in outputs.Values)
                output.Extend();
        }

        public void Calculate()
        {
            proxy.InvokeCalculate();
        }

        #region Algo init methods

        public void AddInput<T>(string inputId, IList<T> data)
        {
            AddInput<T>(inputId, new SeriesListAdapter<T>(data));
        }

        public void AddInput<T>(string inputId, ISeriesAccessor<T> data)
        {
            inputs.Add(inputId, new ListBasedReadonlyDataSeries<T>(data));
        }

        public void AddOutput<T>(string outputId, IList<T> data)
        {
            AddOutput<T>(outputId, new SeriesListAdapter<T>(data));
        }

        public void AddOutput<T>(string outputId, ISeriesAccessor<T> data)
        {
            outputs.Add(outputId, new ListBasedDataSeries<T>(data));
        }

        public void Setparameter(string paramId, object value)
        {
            parameters[paramId] = value;
        }

        #endregion

        #region IAlgoContext implementation

        bool IAlgoContext.GetParameter(string paramId, out object paramValue)
        {
            return parameters.TryGetValue(paramId, out paramValue);
        }

        object IAlgoContext.GetInputSeries(string inputId)
        {
            IManagedDataSeries input;
            if (!inputs.TryGetValue(inputId, out input))
                throw new Exception("Input " + inputId + " cannot be found.");
            return input;
        }

        object IAlgoContext.GetOutputSeries(string inputId)
        {
            IManagedDataSeries input;
            if (!inputs.TryGetValue(inputId, out input))
                throw new Exception("Input " + inputId + " cannot be found.");
            return input;
        }

        #endregion
    }
}
