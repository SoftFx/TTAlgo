using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;
using TickTrader.Algo.Common.Model.Setup;
using TickTrader.Algo.Core;

namespace TickTrader.BotTerminal
{
    internal class BacktesterAdapter : IPluginModel
    {
        private Dictionary<string, IOutputCollector> _outputs = new Dictionary<string, IOutputCollector>();

        public BacktesterAdapter(PluginSetupModel setup, Backtester backtester)
        {
            InitOutputs(setup, backtester);
        }

        private void InitOutputs(PluginSetupModel setup, Backtester backtester)
        {
            foreach (var outputSetup in setup.Outputs)
            {
                if (outputSetup is ColoredLineOutputSetupModel)
                    CreateOuput<double>(backtester, outputSetup);
                else if (outputSetup is MarkerSeriesOutputSetupModel)
                    CreateOuput<Marker>(backtester, outputSetup);
            }
        }

        private void CreateOuput<T>(Backtester backtester, OutputSetupModel setup)
        {
            var id = setup.Id;
            var adapter = new TesterOutputCollector<T>(setup, backtester.Executor);
            _outputs.Add(id, adapter);
        }

        public string InstanceId => "Backtester";

        IDictionary<string, IOutputCollector> IPluginModel.Outputs => _outputs;

        event Action IPluginModel.OutputsChanged { add { } remove { } }
    }
}
