using System;
using System.Threading.Tasks;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Server
{
    public class ExecutorModel
    {
        private readonly PkgRuntimeModel _host;


        public string Id { get; }

        public ExecutorConfig Config { get; } = new ExecutorConfig();


        public event Action<PluginLogRecord> LogUpdated;
        public event Action<DataSeriesUpdate> OutputUpdate;
        public event Action<Exception> ErrorOccurred;
        public event Action<ExecutorModel> Stopped;


        public ExecutorModel(PkgRuntimeModel host, string id, ExecutorConfig config)
        {
            _host = host;
            Id = id;
            Config = config;
        }


        public void Dispose()
        {
            _host.DisposeExecutor(Id);
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
            LogUpdated?.Invoke(record);
        }

        internal void OnErrorOccured(Exception ex)
        {
            ErrorOccurred?.Invoke(ex);
        }

        internal void OnStopped()
        {
            _host.OnExecutorStopped(Id);

            Stopped?.Invoke(this);
        }

        internal void OnDataSeriesUpdate(DataSeriesUpdate update)
        {
            //if (update.SeriesType == DataSeriesUpdate.Types.Type.SymbolRate)
            //{
            //    var bar = update.Value.Unpack<BarData>();
            //    ChartBarUpdated?.Invoke(bar, update.SeriesId, update.UpdateAction);
            //}
            //else if (update.SeriesType == DataSeriesUpdate.Types.Type.NamedStream)
            //{
            //    var bar = update.Value.Unpack<BarData>();
            //    if (update.SeriesId == BacktesterCollector.EquityStreamName)
            //        EquityUpdated?.Invoke(bar, update.UpdateAction);
            //    else if (update.SeriesId == BacktesterCollector.MarginStreamName)
            //        MarginUpdated?.Invoke(bar, update.UpdateAction);
            //}
            //else if (update.SeriesType == DataSeriesUpdate.Types.Type.Output)
            if (update.SeriesType == DataSeriesUpdate.Types.Type.Output)
                OutputUpdate?.Invoke(update);
        }
    }
}
