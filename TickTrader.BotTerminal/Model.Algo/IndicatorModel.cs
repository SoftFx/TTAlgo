using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using TickTrader.Algo.Core;
using SciChart.Charting.Model.DataSeries;
using TickTrader.Algo.Common.Model.Setup;
using TickTrader.Algo.Api;
using System.Linq;
using TickTrader.Algo.Common.Model.Config;
using TickTrader.Algo.Common.Info;

namespace TickTrader.BotTerminal
{
    internal class IndicatorModel : PluginModel
    {
        private Dictionary<string, IXyDataSeries> _series = new Dictionary<string, IXyDataSeries>();


        public bool HasOverlayOutputs => Setup.Outputs.Any(o => o.Metadata.Descriptor.Target == OutputTargets.Overlay);
        public bool HasPaneOutputs => Setup.Outputs.Any(o => o.Metadata.Descriptor.Target != OutputTargets.Overlay);


        public IndicatorModel(PluginConfig config, LocalAlgoAgent agent, IAlgoPluginHost host, IAlgoSetupContext setupContext)
            : base(config, agent, host, setupContext)
        {
            host.StartEvent += Host_StartEvent;
            host.StopEvent += Host_StopEvent;

            if (host.IsStarted)
                StartIndicator();
        }


        public IXyDataSeries GetOutputSeries(string id)
        {
            return _series[id];
        }

        public override void Dispose()
        {
            base.Dispose();

            Host.StartEvent -= Host_StartEvent;
            Host.StopEvent -= Host_StopEvent;
            if (State == PluginStates.Running)
                StopIndicator().ContinueWith(t => { /* TO DO: log errors */ });
        }


        protected override PluginExecutor CreateExecutor()
        {
            var executor = base.CreateExecutor();

            foreach (var outputSetup in Setup.Outputs)
            {
                if (outputSetup is ColoredLineOutputSetupModel)
                {
                    var model = new DoubleSeriesModel(executor, (ColoredLineOutputSetupModel)outputSetup);
                    _series.Add(outputSetup.Id, model.SeriesData);
                }
                else if (outputSetup is MarkerSeriesOutputSetupModel)
                {
                    var model = new MarkerSeriesModel(executor, (MarkerSeriesOutputSetupModel)outputSetup);
                    _series.Add(outputSetup.Id, model.SeriesData);
                }
            }

            return executor;
        }

        protected override async void OnPluginUpdated()
        {
            if (State == PluginStates.Running)
            {
                await StopIndicator();
                UpdateRefs();
                StartIndicator();
            }
        }


        private void StartIndicator()
        {
            if (PluginStateHelper.CanStart(State))
            {
                if (StartExcecutor())
                    ChangeState(PluginStates.Running);
            }
        }

        private async Task StopIndicator()
        {
            if (State == PluginStates.Running)
            {
                if (await StopExecutor())
                    ChangeState(PluginStates.Stopped);
                foreach (var dataLine in _series.Values)
                    dataLine.Clear();
            }
        }

        private void Host_StartEvent()
        {
            StartIndicator();
        }

        private Task Host_StopEvent(object sender, CancellationToken cToken)
        {
            return StopIndicator();
        }
    }
}
