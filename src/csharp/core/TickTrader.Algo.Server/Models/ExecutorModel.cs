using System.Threading.Tasks;
using TickTrader.Algo.Async;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Server
{
    public class ExecutorModel
    {
        private readonly PkgRuntimeModel _host;
        private readonly ChannelEventSource<PluginLogRecord> _logEventSrc = new ChannelEventSource<PluginLogRecord>();
        private readonly ChannelEventSource<DataSeriesUpdate> _outputEventSrc = new ChannelEventSource<DataSeriesUpdate>();
        private readonly ChannelEventSource<bool> _stoppedEventSrc = new ChannelEventSource<bool>();


        public string Id { get; }

        public ExecutorConfig Config { get; } = new ExecutorConfig();

        public IEventSource<PluginLogRecord> LogUpdated => _logEventSrc;

        public IEventSource<DataSeriesUpdate> OutputUpdated => _outputEventSrc;

        public IEventSource<bool> Stopped => _stoppedEventSrc;


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
            _outputEventSrc.Dispose();
            _stoppedEventSrc.Dispose();
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

        internal void OnDataSeriesUpdate(DataSeriesUpdate update)
        {
            _outputEventSrc.Writer.TryWrite(update);
        }

        internal void OnStopped()
        {
            _stoppedEventSrc.Writer.TryWrite(false);
        }
    }
}
