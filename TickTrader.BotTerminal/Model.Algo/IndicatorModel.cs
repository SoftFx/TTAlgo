using System.Threading.Tasks;
using System.Threading;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Core.Setup;
using TickTrader.Algo.Core;

namespace TickTrader.BotTerminal
{
    internal class IndicatorModel : PluginModel, IIndicatorWriter
    {
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


        public void Dispose()
        {
            Host.StartEvent -= Host_StartEvent;
            Host.StopEvent -= Host_StopEvent;
            if (State == PluginModelInfo.Types.PluginState.Running)
                StopIndicator().ContinueWith(t => { /* TO DO: log errors */ });
        }


        protected override async void OnPluginUpdated()
        {
            if (State == PluginModelInfo.Types.PluginState.Running)
            {
                await StopIndicator();
                UpdateRefs();
                StartIndicator();
            }
        }

        private void StartIndicator()
        {
            if (State.CanStart())
            {
                _host.EnqueueStartAction(() => StartExcecutor().ContinueWith(t => { if (t.Result) ChangeState(PluginModelInfo.Types.PluginState.Running); }));
                //if (StartExcecutor())
                //    ChangeState(PluginModelInfo.Types.PluginState.Running);
            }
        }

        private async Task StopIndicator()
        {
            if (State.IsRunning())
            {
                if (await StopExecutor())
                    ChangeState(PluginModelInfo.Types.PluginState.Stopped);
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
                case PluginLogRecord.Types.LogSeverity.Info:
                case PluginLogRecord.Types.LogSeverity.Error:
                case PluginLogRecord.Types.LogSeverity.Custom:
                    _journal.Add(EventMessage.Create(record));
                    break;
                case PluginLogRecord.Types.LogSeverity.Alert:
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
