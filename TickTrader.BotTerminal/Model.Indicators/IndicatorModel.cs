using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using TickTrader.Algo.Core;
using StateMachinarium;
using SciChart.Charting.Model.DataSeries;
using TickTrader.Algo.GuiModel;
using SciChart.Charting.Visuals.RenderableSeries;

namespace TickTrader.BotTerminal
{
    internal class IndicatorModel
    {
        private enum States { Idle, Building, Stopping }
        private enum Events { Start, StopRequest, DoneBuildig }

        private StateMachine<States> stateController = new StateMachine<States>();
        private IIndicatorSetup setup;
        private List<IRenderableSeries> seriesList = new List<IRenderableSeries>();
        private CancellationTokenSource stopSrc;
        private IndicatorBuilder builder;

        public IndicatorModel(IIndicatorSetup setup, IndicatorBuilder builder)
        {
            if (setup == null)
                throw new ArgumentNullException("config");

            if (builder == null)
                throw new ArgumentNullException("builder");

            this.setup = setup;
            this.builder = builder;

            stateController.AddTransition(States.Idle, Events.Start, States.Building);
            stateController.AddTransition(States.Building, Events.StopRequest, States.Stopping);
            stateController.AddTransition(States.Building, Events.DoneBuildig, States.Idle);
            stateController.AddTransition(States.Stopping, Events.DoneBuildig, States.Idle);

            stateController.OnEnter(States.Building, () => BuildIndicator(stopSrc.Token, setup.DataLen));
            stateController.OnEnter(States.Stopping, () => stopSrc.Cancel());

            stateController.StateChanged += (o, n) => System.Diagnostics.Debug.WriteLine("Indicator [" + Id + "] " + o + " => " + n);
        }

        public long Id { get { return setup.InstanceId; } }
        public string DisplayName { get { return "[" + Id + "] " + setup.Descriptor.DisplayName; } }
        public IEnumerable<IRenderableSeries> SeriesCollection { get { return seriesList; } }
        public bool IsOverlay { get { return setup.Descriptor.IsOverlay; } }
        public IndicatorSetupBase Setup { get { return setup.UiModel; } }

        public event Action<IndicatorModel> Closed;

        public void Start()
        {
            stopSrc = new CancellationTokenSource();
            stateController.PushEvent(Events.Start);
        }

        public Task Stop()
        {
            return stateController.PushEventAndWait(Events.StopRequest, States.Idle);
        }

        public IIndicatorSetup CreateSetupClone()
        {
            return setup.CreateCopy();
        }

        public void Close()
        {
            Stop();

            if (Closed != null)
                Closed(this);
        }

        private async void BuildIndicator(CancellationToken cToken, int size)
        {
            try
            {
                await Task.Factory.StartNew(() => builder.BuildNext(size, cToken));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
            }

            stateController.PushEvent(Events.DoneBuildig);
        }

        public void AddSeries(ColoredLineOutputSetup setup, IDataSeries data)
        {
            if (setup.IsEnabled)
            {
                data.SeriesName = setup.Descriptor.Id;
                FastLineRenderableSeries chartSeries = new FastLineRenderableSeries();
                chartSeries.DataSeries = data;
                chartSeries.Stroke = setup.LineColor;
                chartSeries.StrokeThickness = setup.LineThickness;
                seriesList.Add(chartSeries);
            }
        }
    }
}
