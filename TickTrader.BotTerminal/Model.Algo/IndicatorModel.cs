using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using TickTrader.Algo.Core;
using SciChart.Charting.Model.DataSeries;
using TickTrader.Algo.GuiModel;
using TickTrader.Algo.Api;

namespace TickTrader.BotTerminal
{
    internal class IndicatorModel : PluginModel
    {
        private Dictionary<string, IXyDataSeries> series = new Dictionary<string, IXyDataSeries>();

        public IndicatorModel(PluginSetup pSetup, IAlgoPluginHost host)
            : base(pSetup, host)
        {
            host.StartEvent += Host_StartEvent;
            host.StopEvent += Host_StopEvent;

            if (host.IsStarted)
                StartIndicator();
        }

        public bool IsOverlay { get { return Setup.Descriptor.IsOverlay; } }

        public IXyDataSeries GetOutputSeries(string id)
        {
            return series[id];
        }

        private void Host_StartEvent()
        {
            StartIndicator();
        }

        protected override PluginExecutor CreateExecutor()
        {
            var executor = base.CreateExecutor();

            foreach (var outputSetup in Setup.Outputs)
            {
                if (outputSetup is ColoredLineOutputSetup)
                {
                    var buffer = executor.GetOutput<double>(outputSetup.Id);
                    var adapter = new DoubleSeriesAdapter(buffer, (ColoredLineOutputSetup)outputSetup);
                    series.Add(outputSetup.Id, adapter.SeriesData);
                }
                else if (outputSetup is MarkerSeriesOutputSetup)
                {
                    var buffer = executor.GetOutput<Marker>(outputSetup.Id);
                    var adapter = new MarkerSeriesAdapter(buffer, (MarkerSeriesOutputSetup)outputSetup);
                    series.Add(outputSetup.Id, adapter.SeriesData);
                }
            }

            return executor;
        }

        private bool IsRunning { get; set; }
        private bool IsStopping { get; set; }

        private void StartIndicator()
        {
            if (!IsRunning)
            {
                IsRunning = StartExcecutor();
            }
        }

        private async Task StopIndicator()
        {
            if (IsRunning && !IsStopping)
            {
                IsStopping = true;
                await StopExecutor();
                IsRunning = false;
                IsStopping = false;
                foreach (var dataLine in this.series.Values)
                    dataLine.Clear();
            }
        }

        public void Dispose()
        {
            Host.StartEvent -= Host_StartEvent;
            Host.StopEvent -= Host_StopEvent;
            if (IsRunning)
                StopIndicator().ContinueWith(t => { /* TO DO: log errors */ });
        }

        private Task Host_StopEvent(object sender, CancellationToken cToken)
        {
            return StopIndicator();
        }
    }
}
