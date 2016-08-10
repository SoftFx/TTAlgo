using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using TickTrader.Algo.Core;
using SciChart.Charting.Model.DataSeries;
using TickTrader.Algo.GuiModel;
using SciChart.Charting.Visuals.RenderableSeries;
using Machinarium.State;
using TickTrader.Algo.Api;
using SciChart.Charting.Visuals.Annotations;
using NLog;
using Machinarium.Qnil;

namespace TickTrader.BotTerminal
{
    internal class IndicatorModel2 : PluginModel
    {
        private Task startTask;
        private Dictionary<string, IXyDataSeries> series = new Dictionary<string, IXyDataSeries>();

        public IndicatorModel2(PluginSetup pSetup, IAlgoPluginHost host)
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
                //else if (outputSetup is MarkerSeriesOutputSetup)
                //    new MarkerSeriesAdapter(model, (MarkerSeriesOutputSetup)outputSetup);
            }

            return executor;
        }

        private bool IsRunning { get { return startTask != null; } }

        private void StartIndicator()
        {
            if (!IsRunning)
                startTask = StartExcecutor();
        }

        private async Task StopIndicator()
        {
            if (IsRunning)
            {
                await startTask;
                await StopExecutor();
                startTask = null;
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
