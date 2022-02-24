using System;
using System.Threading.Tasks;
using TickTrader.Algo.Async;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Server
{
    public class ExecutorModel
    {
        //private readonly RuntimeControlModel _host;
        private readonly ChannelEventSource<PluginLogRecord> _logEventSrc = new ChannelEventSource<PluginLogRecord>();
        private readonly ChannelEventSource<PluginStatusUpdate> _statusEventSrc = new ChannelEventSource<PluginStatusUpdate>();
        private readonly ChannelEventSource<OutputSeriesUpdate> _outputEventSrc = new ChannelEventSource<OutputSeriesUpdate>();
        private readonly ChannelEventSource<ExecutorStateUpdate> _stateEventSrc = new ChannelEventSource<ExecutorStateUpdate>();


        public string Id { get; }

        public ExecutorConfig Config { get; } = new ExecutorConfig();

        public IEventSource<PluginLogRecord> LogUpdated => _logEventSrc;

        public IEventSource<PluginStatusUpdate> StatusUpdated => _statusEventSrc;

        public IEventSource<OutputSeriesUpdate> OutputUpdated => _outputEventSrc;

        public IEventSource<ExecutorStateUpdate> StateUpdated => _stateEventSrc;

        public Action<PluginExitedMsg> ExitHandler { get; set; }


        public ExecutorModel(string id, ExecutorConfig config)
        {
            //_host = host;
            Id = id;
            Config = config;
        }


        public void Dispose()
        {
            //_host.DisposeExecutor(Id);

            _logEventSrc.Dispose();
            _statusEventSrc.Dispose();
            _outputEventSrc.Dispose();
            _stateEventSrc.Dispose();
        }

        public Task Start()
        {
            return Task.CompletedTask;// _host.StartExecutor(new StartExecutorRequest { ExecutorId = Id });
        }

        public Task Stop()
        {
            return Task.CompletedTask;// _host.StopExecutor(new StopExecutorRequest { ExecutorId = Id });
        }


        internal void OnLogUpdated(PluginLogRecord record)
        {
            _logEventSrc.Writer.TryWrite(record);
        }

        internal void OnStatusUpdated(PluginStatusUpdate update)
        {
            _statusEventSrc.Writer.TryWrite(update);
        }

        internal void OnDataSeriesUpdate(OutputSeriesUpdate update)
        {
            _outputEventSrc.Writer.TryWrite(update);
        }

        internal void OnStateUpdated(ExecutorStateUpdate update)
        {
            _stateEventSrc.Writer.TryWrite(update);
        }

        internal void OnExit(PluginExitedMsg msg)
        {
            ExitHandler?.Invoke(msg);
        }
    }
}
