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
using TickTrader.Algo.Domain;

namespace TickTrader.BotTerminal
{
    internal class IndicatorModel : PluginModel, IIndicatorWriter
    {
        private IndicatorListenerProxy _indicatorListener;
        private EventJournal _journal;
        private IAlgoPluginHost _host;

        public IndicatorModel(PluginConfig config, LocalAlgoAgent agent, IAlgoPluginHost host, IAlgoSetupContext setupContext)
            : base(config, agent, host, setupContext)
        {
            _journal = agent.Shell.EventJournal;

            _host = host;
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

        protected override RuntimeModel CreateExecutor()
        {
            var executor = base.CreateExecutor();

            executor.Config.IsLoggingEnabled = true;
            _indicatorListener = new IndicatorListenerProxy(executor, this);

            return executor;
        }

        private void StartIndicator()
        {
            if (PluginStateHelper.CanStart(State))
            {
                _host.EnqueueStartAction(() => StartExcecutor());
                //if (StartExcecutor())
                //    ChangeState(PluginStates.Running);
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

        void IIndicatorWriter.LogMessage(UnitLogRecord record)
        {
            switch (record.Severity)
            {
                case UnitLogRecord.Types.LogSeverity.Info:
                case UnitLogRecord.Types.LogSeverity.Error:
                case UnitLogRecord.Types.LogSeverity.Custom:
                    _journal.Add(EventMessage.Create(record));
                    break;
                case UnitLogRecord.Types.LogSeverity.Alert:
                    AlertModel.AddAlert(InstanceId, record);
                    break;
                default:
                    break;
            }
        }
    }

    public interface IIndicatorWriter
    {
        void LogMessage(UnitLogRecord record);
    }
}
