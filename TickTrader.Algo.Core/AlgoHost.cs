using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Core.Metadata;

namespace TickTrader.Algo.Core
{
    public class AlgoHost : NoTimeoutByRefObject, IAlgoContext
    {
        private AlgoDescriptor descriptor;
        private Dictionary<string, object> parameters = new Dictionary<string, object>();
        private Dictionary<string, IInputDataSeries> inputs = new Dictionary<string, IInputDataSeries>();
        private Dictionary<string, IOutputDataSeries> outputs = new Dictionary<string, IOutputDataSeries>();

        internal AlgoHost(AlgoDescriptor descriptor)
        {
            this.descriptor = descriptor;
        }

        public int Count { get; private set; }

        public void ReadNext()
        {
            foreach (var input in inputs.Values)
                input.ReadNext();

            foreach (var output in outputs.Values)
                output.ExtendBuffer();
        }

        public void UpdateCurrent()
        {
            foreach (var input in inputs.Values)
                input.ReRead();
        }

        public void Reset()
        {
            foreach (var input in inputs.Values)
                input.Reset();

            foreach (var output in outputs.Values)
                output.Reset();
        }

        #region Algo init methods

        public void AddInput<T>(string inputId, DataSeriesReader<T> reader)
        {
            InputDescriptor inputInfo = descriptor.Inputs.FirstOrDefault(i => i.Id == inputId);
            if (inputInfo != null)
            {
                IInputDataSeries seriesPropertyObj;
                if (inputInfo.IsShortDefinition)
                    seriesPropertyObj = new InputDataSeries((DataSeriesReader<double>)reader);
                else
                    seriesPropertyObj = new InputDataSeries<T>(reader);
                inputs.Add(inputId, seriesPropertyObj);
            }
        }

        public void AddOutput<T>(string outputId, DataSeriesWriter<T> writer)
        {
            OutputDescriptor outputInfo = descriptor.Outputs.FirstOrDefault(i => i.Id == outputId);
            if (outputInfo != null)
            {
                IOutputDataSeries seriesPropertyObj;
                if (outputInfo.IsShortDefinition)
                    seriesPropertyObj = new OutputDataSeries((DataSeriesWriter<double>)writer);
                else
                    seriesPropertyObj = new OutputDataSeries<T>(writer);
                outputs.Add(outputId, seriesPropertyObj);
            }
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
            IInputDataSeries input;
            if (!inputs.TryGetValue(inputId, out input))
                throw new Exception("Input " + inputId + " cannot be found.");
            return input;
        }

        object IAlgoContext.GetOutputSeries(string outputId)
        {
            IOutputDataSeries output;
            if (!outputs.TryGetValue(outputId, out output))
                throw new Exception("Output " + outputId + " cannot be found.");
            return output;
        }

        #endregion
    }

    public interface DataSeriesReader<T>
    {
        T ReadNext();
        T ReRead();
        void Reset();
    }

    public interface DataSeriesWriter<T>
    {
        void WriteAt(int index, T val);
        void Reset();
    }
}
