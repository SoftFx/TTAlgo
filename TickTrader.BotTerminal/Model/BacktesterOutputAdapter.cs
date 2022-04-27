using System;
using System.Collections.Generic;
using System.Linq;
using TickTrader.Algo.BacktesterApi;
using TickTrader.Algo.Domain;

namespace TickTrader.BotTerminal
{
    internal class BacktesterOutputAdapter : IPluginModel
    {
        private Dictionary<string, IOutputCollector> _outputs = new Dictionary<string, IOutputCollector>();


        public BacktesterOutputAdapter(PluginConfig config, PluginDescriptor descriptor)
        {
            InitOutputs(config, descriptor);
        }


        public void SendSnapshots(BacktesterResults results)
        {
            foreach (var pair in _outputs)
            {
                var id = pair.Key;
                var output = pair.Value;

                if (results.Outputs.TryGetValue(id, out var points))
                    output.SendSnapshot(points);
            }
        }

        //public void Subscribe(BacktesterController backtester) { } // visualization hook


        private void InitOutputs(PluginConfig config, PluginDescriptor descriptor)
        {
            var outputMap = descriptor.Outputs.ToDictionary(o => o.Id);

            foreach (var property in config.UnpackProperties())
            {
                if (property is IOutputConfig output)
                {
                    var id = output.PropertyId;

                    if (output.IsEnabled && outputMap.ContainsKey(id))
                        CreateOutput(output, outputMap[id]);
                }
            }
        }

        private void CreateOutput(IOutputConfig config, OutputDescriptor descriptor)
        {
            IOutputCollector output = null;

            if (config is ColoredLineOutputConfig)
                output = new OutputCollector<double>(config, descriptor);
            else if (config is MarkerSeriesOutputConfig)
                output = new OutputCollector<MarkerInfo>(config, descriptor);

            if (output == null)
                return;

            _outputs.Add(config.PropertyId, output);
        }


        #region IPluginModel implementation

        string IPluginModel.InstanceId => "Backtester";

        IDictionary<string, IOutputCollector> IPluginModel.Outputs => _outputs;

        event Action IPluginModel.OutputsChanged { add { } remove { } }

        #endregion
    }
}
