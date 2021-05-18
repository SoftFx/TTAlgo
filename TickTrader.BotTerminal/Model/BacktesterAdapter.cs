using System;
using System.Collections.Generic;
using TickTrader.Algo.Api;
using TickTrader.Algo.Core;
using TickTrader.Algo.Domain;

namespace TickTrader.BotTerminal
{
    internal class BacktesterAdapter : IPluginModel
    {
        private Dictionary<string, IOutputCollector> _outputs = new Dictionary<string, IOutputCollector>();

        public BacktesterAdapter(PluginConfig config, Backtester backtester)
        {
            InitOutputs(config, backtester);
        }

        private void InitOutputs(PluginConfig config, Backtester backtester)
        {
            foreach (var property in config.UnpackProperties())
            {
                if (property is IOutputConfig output)
                {
                    if (output is ColoredLineOutputConfig)
                        CreateOuput<double>(backtester, output);
                    else if (output is MarkerSeriesOutputConfig)
                        CreateOuput<Marker>(backtester, output);
                }
            }
        }

        private void CreateOuput<T>(Backtester backtester, IOutputConfig output)
        {
            var id = output.PropertyId;
            var adapter = new TesterOutputCollector<T>(id, backtester.Executor);
            _outputs.Add(id, adapter);
        }

        public string InstanceId => "Backtester";

        IDictionary<string, IOutputCollector> IPluginModel.Outputs => _outputs;

        event Action IPluginModel.OutputsChanged { add { } remove { } }
    }
}
