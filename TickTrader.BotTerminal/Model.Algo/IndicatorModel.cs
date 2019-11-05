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
    internal class IndicatorModel : PluginModel, IIndicatorWriter
    {
        private IndicatorListenerProxy _indicatorListener;

        public IndicatorModel(PluginConfig config, LocalAlgoAgent agent, IAlgoPluginHost host, IAlgoSetupContext setupContext)
            : base(config, agent, host, setupContext)
        {
            host.StartEvent += Host_StartEvent;
            host.StopEvent += Host_StopEvent;

            if (host.IsStarted)
                StartIndicator();
        }


        public override void Dispose()
        {
            base.Dispose();

            Host.StartEvent -= Host_StartEvent;
            Host.StopEvent -= Host_StopEvent;
            if (State == PluginStates.Running)
                StopIndicator().ContinueWith(t => { /* TO DO: log errors */ });
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

        protected override PluginExecutor CreateExecutor()
        {
            var executor = base.CreateExecutor();

            _indicatorListener = new IndicatorListenerProxy(executor, this);

            return executor;
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

        void IIndicatorWriter.LogMessage(PluginLogRecord record)
        {
            switch (record.Severity)
            {
                case LogSeverities.Alert:
                case LogSeverities.AlertClear:
                    AlertModel.AddAlert(InstanceId, record);
                    break;
                default:
                    break;
            }
        }
    }

    public interface IIndicatorWriter
    {
        void LogMessage(PluginLogRecord record);
    }
}
