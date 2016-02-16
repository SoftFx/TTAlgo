using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using TickTrader.Algo.Core;
using StateMachinarium;
using SciChart.Charting.Model.DataSeries;

namespace TickTrader.BotTerminal
{
    internal class IndicatorModel : ISeriesContainer
    {
        private enum States { Idle, Building, Stopping }
        private enum Events { Start, StopRequest, DoneBuildig }

        private StateMachine<States> stateController = new StateMachine<States>();
        private IIndicatorConfig config;
        private List<IDataSeries> seriesList = new List<IDataSeries>();
        private CancellationTokenSource stopSrc;
        private IIndicatorBuilder builder;

        public IndicatorModel(IIndicatorConfig config)
        {
            if (config == null)
                throw new ArgumentNullException("config");

            this.config = config;

            this.builder = config.CreateBuilder(this);

            stateController.AddTransition(States.Idle, Events.Start, States.Building);
            stateController.AddTransition(States.Building, Events.StopRequest, States.Stopping);
            stateController.AddTransition(States.Building, Events.DoneBuildig, States.Idle);
            stateController.AddTransition(States.Stopping, Events.DoneBuildig, States.Idle);

            stateController.OnEnter(States.Building, () => BuildIndicator(stopSrc.Token));
            stateController.OnEnter(States.Stopping, () => stopSrc.Cancel());

            stateController.StateChanged += (o, n) => System.Diagnostics.Debug.WriteLine("Indicator [" + Id + "] " + o + " => " + n);
        }

        public long Id { get { return config.InstanceId; } }
        public string DisplayName { get { return "[" + Id + "] " + config.Descriptor.DisplayName; } }
        public IEnumerable<IDataSeries> SeriesCollection { get { return seriesList; } }

        public void Start()
        {
            stopSrc = new CancellationTokenSource();
            stateController.PushEvent(Events.Start);
        }

        public Task Stop()
        {
            return stateController.PushEventAndAsyncWait(Events.StopRequest, States.Idle);
        }

        public IIndicatorConfig GetFactoryClone()
        {
            throw new NotImplementedException();
        }

        private async void BuildIndicator(CancellationToken cToken)
        {
            try
            {
                builder.Reset();

                await Task.Factory.StartNew(() => builder.Build(cToken));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
            }

            stateController.PushEvent(Events.DoneBuildig);
        }

        public void AddSeries(IDataSeries series)
        {
            seriesList.Add(series);
        }
    }
}
