using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using TickTrader.Algo.Api;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Core.Repository;

namespace TickTrader.Algo.Core
{
    public class PluginExecutor : IPluginSetupTarget, IDisposable
    {
        private ISynchronizationContext _syncContext;
        
        private AlgoPluginRef _ref;
        private BunchingBlock<object> _channel;
        private Dictionary<string, object> _pluginParams = new Dictionary<string, object>();
        private FeedStrategy _fStrategy;
        private CommonCdProxy _bProxy;
        private FeedCdProxy _fProxy;

        public PluginExecutor(AlgoPluginRef pluginRef, ISynchronizationContext updatesSync)
        {
            _ref = pluginRef;
            _syncContext = updatesSync;
            Core = pluginRef.CreateExecutor();
            IsIsolated = pluginRef.IsIsolated;
        }

        private bool IsBunchingRequired => IsIsolated || _syncContext != null;

        // TO DO: Direct access to Core should be removed in future refactoring!
        internal PluginExecutorCore Core { get; }
        public bool IsIsolated { get; }
        public bool IsRunning { get; private set; }

        public event Action<BotLogRecord> LogUpdated;
        public event Action<TesterTradeTransaction> TradesUpdated;
        public event Action<TradeReportEntity> TradeHistoryUpdated;
        public event Action<RateUpdate> SymbolRateUpdated;
        public event Action<BarEntity, string, SeriesUpdateActions> ChartBarUpdated;
        public event Action<BarEntity, SeriesUpdateActions> EquityUpdated;
        public event Action<BarEntity, SeriesUpdateActions> MarginUpdated;
        public event Action<IDataSeriesUpdate> OutputUpdate;
        public event Action<Exception> ErrorOccurred;

        #region Excec control

        public void Start()
        {
            ConfigurateCore();
            Core.Start();
        }

        public void Stop()
        {
            Core.Stop();
        }

        public void Abort()
        {
            Core.Abort();
        }

        public void HandleDisconnect()
        {
            Core.HandleDisconnect();
        }

        public void HandleReconnect()
        {
            Core.HandleReconnect();
        }

        #endregion

        #region Setup

        public InvokeStartegy InvokeStrategy { get; set; }
        public IAccountInfoProvider AccInfoProvider { get; set; }
        public ITradeHistoryProvider TradeHistoryProvider { get; set; }
        public IPluginMetadata Metadata { get; set; }
        public IFeedProvider Feed { get; set; }
        public IFeedHistoryProvider FeedHistory { get; set; }
        public PluginExecutorConfig Config { get; } = new PluginExecutorConfig();

        public void SetParameter(string id, object value)
        {
            _pluginParams[id] = value;
        }

        public T GetFeedStrategy<T>()
           where T : FeedStrategy
        {
            return (T)_fStrategy;
        }

        public void MapInput(string inputName, string symbolCode, Mapping mapping)
        {
            // hook to appear in plugin domain
            mapping?.MapInput(this, inputName, symbolCode);
        }

        public BarStrategy InitBarStrategy(BarPriceType mainPirceTipe)
        {
            var strategy = new BarStrategy(mainPirceTipe);
            _fStrategy = strategy;
            return strategy;
        }

        public QuoteStrategy InitQuoteStrategy(IFeedProvider feed)
        {
            var strategy = new QuoteStrategy();
            _fStrategy = strategy;
            return strategy;
        }

        #endregion

        public void Dispose()
        {
        }

        private void ConfigurateCore()
        {
            Core.MainSymbolCode = Config.MainSymbolCode;
            Core.TimeFrame = Config.TimeFrame;

            Core.BotWorkingFolder = Core.BotWorkingFolder;
            Core.WorkingFolder = Core.WorkingFolder;

            if (IsIsolated)
            {
                _bProxy = new CommonCdProxy(AccInfoProvider, Metadata, TradeHistoryProvider);
                Core.AccInfoProvider = _bProxy;
                Core.TradeHistoryProvider = _bProxy;
                Core.Metadata = _bProxy;

                _fProxy = new FeedCdProxy(Feed, FeedHistory);
                Core.Feed = _fProxy;
                Core.FeedHistory = _fProxy;
            }
            else
            {
                Core.AccInfoProvider = AccInfoProvider;
                Core.TradeHistoryProvider = TradeHistoryProvider;
                Core.Metadata = Metadata;
                Core.Feed = Feed;
                Core.FeedHistory = FeedHistory;
            }
        }

        #region Update Marshalling 

        internal void StartUpdateMarshalling()
        {
            if (IsBunchingRequired)
            {
                _channel = new BunchingBlock<object>(MarshalUpdates, 30, 60);
                Core.OnUpdate = _channel.Enqueue;
            }
            else
            {
                Core.OnUpdate = MarshalUpdate;
            }
        }

        internal void StopUpdateMarshalling()
        {
            try
            {
                if (_channel != null)
                {
                    _channel.Complete();
                    _channel.Completion.Wait();
                }
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke(ex);
            }
            _channel = null;
        }

        private void MarshalUpdates(IReadOnlyList<object> updates)
        {
            _syncContext.Invoke(() =>
            {
                for (int i = 0; i < updates.Count; i++)
                {
                    try
                    {
                        MarshalUpdate(updates[i]);
                    }
                    catch (Exception ex)
                    {
                        ErrorOccurred?.Invoke(ex);
                    }
                }
            });
        }

        private void MarshalUpdate(object update)
        {
            if (update is BotLogRecord)
                LogUpdated?.Invoke((BotLogRecord)update);
            else if (update is TradeReportEntity)
                TradeHistoryUpdated?.Invoke((TradeReportEntity)update);
            else if (update is TesterTradeTransaction)
                TradesUpdated?.Invoke((TesterTradeTransaction)update);
            else if (update is RateUpdate)
                SymbolRateUpdated?.Invoke((RateUpdate)update);
            else if (update is IDataSeriesUpdate)
            {
                var seriesUpdate = (IDataSeriesUpdate)update;
                if (seriesUpdate.SeriesType == DataSeriesTypes.SymbolRate)
                {
                    var barUpdate = (DataSeriesUpdate<BarEntity>)update;
                    ChartBarUpdated?.Invoke(barUpdate.Value, barUpdate.SeriesId, barUpdate.Action);
                }
                else if (seriesUpdate.SeriesType == DataSeriesTypes.NamedStream)
                {
                    var barUpdate = (DataSeriesUpdate<BarEntity>)update;
                    if (barUpdate.SeriesId == BacktesterCollector.EquityStreamName)
                        EquityUpdated?.Invoke(barUpdate.Value, barUpdate.Action);
                    else if (barUpdate.SeriesId == BacktesterCollector.MarginStreamName)
                        MarginUpdated?.Invoke(barUpdate.Value, barUpdate.Action);
                }
                else if (seriesUpdate.SeriesType == DataSeriesTypes.Output)
                    OutputUpdate?.Invoke(seriesUpdate);
            }

            #endregion
        }
        
    }
}
