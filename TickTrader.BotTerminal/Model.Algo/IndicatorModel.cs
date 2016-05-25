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

namespace TickTrader.BotTerminal
{
    internal class IndicatorModel : IIndicatorAdapterContext
    {
        private enum States { Idle, Building, Stopping }
        private enum Events { Start, StopRequest, DoneBuildig }

        private StateMachine<States> stateController = new StateMachine<States>();
        private IIndicatorSetup setup;
        private List<IRenderableSeries> seriesList = new List<IRenderableSeries>();
        private List<IDynamicListSource<IAnnotation>> annotations = new List<IDynamicListSource<IAnnotation>>();
        private CancellationTokenSource stopSrc;
        private IndicatorBuilder builder;
        private Func<int, DateTime> indexToTimeFunc;

        public IndicatorModel(IIndicatorSetup setup, IndicatorBuilder builder, Func<int, DateTime> indexToTimeFunc)
        {
            if (setup == null)
                throw new ArgumentNullException("config");

            if (builder == null)
                throw new ArgumentNullException("builder");

            this.setup = setup;
            this.builder = builder;
            this.indexToTimeFunc = indexToTimeFunc;

            stateController.AddTransition(States.Idle, Events.Start, States.Building);
            stateController.AddTransition(States.Building, Events.StopRequest, States.Stopping);
            stateController.AddTransition(States.Building, Events.DoneBuildig, States.Idle);
            stateController.AddTransition(States.Stopping, Events.DoneBuildig, States.Idle);

            stateController.OnEnter(States.Building, () => BuildIndicator(stopSrc.Token, setup.DataLen));
            stateController.OnEnter(States.Stopping, () => stopSrc.Cancel());

            stateController.StateChanged += (o, n) => System.Diagnostics.Debug.WriteLine("Indicator [" + Id + "] " + o + " => " + n);
        }

        public long Id { get { return setup.InstanceId; } }
        public string DisplayName { get { return setup.Descriptor.DisplayName; } }
        public IReadOnlyList<IRenderableSeries> SeriesCollection { get { return seriesList; } }
        public IReadOnlyList<IDynamicListSource<IAnnotation>> Annotations { get { return annotations; } }
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

        OutputBuffer<T> IIndicatorAdapterContext.GetOutput<T>(string name)
        {
            return builder.GetOutput<T>(name);
        }

        DateTime IIndicatorAdapterContext.GetTimeCoordinate(int index)
        {
            return indexToTimeFunc(index);
        }

        void IIndicatorAdapterContext.AddSeries(IRenderableSeries series)
        {
            this.seriesList.Add(series);
        }

        void IIndicatorAdapterContext.AddSeries(DynamicList<MarkerAnnotation> series)
        {
            this.annotations.Add(series.Select<MarkerAnnotation, IAnnotation>(m => m));
        }
    }
}
