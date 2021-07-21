using System.Threading.Tasks;
using TickTrader.Algo.Async;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Server
{
    public class ExecutorModel
    {
        private readonly PkgRuntimeModel _host;
        private readonly ChannelEventSource<PluginLogRecord> _logEventSrc = new ChannelEventSource<PluginLogRecord>();
        private readonly ChannelEventSource<PluginStatusUpdate> _statusEventSrc = new ChannelEventSource<PluginStatusUpdate>();
        private readonly ChannelEventSource<DataSeriesUpdate> _outputEventSrc = new ChannelEventSource<DataSeriesUpdate>();
        private readonly ChannelEventSource<ExecutorStateUpdate> _stateEventSrc = new ChannelEventSource<ExecutorStateUpdate>();


        public string Id { get; }

        public ExecutorConfig Config { get; } = new ExecutorConfig();

        public IEventSource<PluginLogRecord> LogUpdated => _logEventSrc;

        public IEventSource<PluginStatusUpdate> StatusUpdated => _statusEventSrc;

        public IEventSource<DataSeriesUpdate> OutputUpdated => _outputEventSrc;

        public IEventSource<ExecutorStateUpdate> StateUpdated => _stateEventSrc;


        public ExecutorModel(PkgRuntimeModel host, string id, ExecutorConfig config)
        {
            _host = host;
            Id = id;
            Config = config;
        }


        public void Dispose()
        {
            _host.DisposeExecutor(Id);

            _logEventSrc.Dispose();
            _statusEventSrc.Dispose();
            _outputEventSrc.Dispose();
            _stateEventSrc.Dispose();
        }

        public Task Start()
        {
            return _host.StartExecutor(new StartExecutorRequest { ExecutorId = Id });
        }

        public Task Stop()
        {
            return _host.StopExecutor(new StopExecutorRequest { ExecutorId = Id });
        }


        internal void OnLogUpdated(PluginLogRecord record)
        {
            _logEventSrc.Writer.TryWrite(record);
        }

        internal void OnStatusUpdated(PluginStatusUpdate update)
        {
            _statusEventSrc.Writer.TryWrite(update);
        }

        internal void OnDataSeriesUpdate(DataSeriesUpdate update)
        {
            _outputEventSrc.Writer.TryWrite(update);
        }

        internal void OnStateUpdated(ExecutorStateUpdate update)
        {
            _stateEventSrc.Writer.TryWrite(update);
        }
    }
}
