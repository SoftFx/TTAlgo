﻿using System;
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
    public class PluginExecutor : CrossDomainObject, IDisposable
    {
        private readonly string _id;
        private ISynchronizationContext _syncContext;

        private AlgoPluginRef _pluginRef;
        private PluginContainer _container;
        private CommonCdProxy _cProxy;
        private FeedCdProxy _fProxy;
        private TradeApiProxy _tProxy;

        internal PluginExecutor(string id, AlgoPluginRef pluginRef, ISynchronizationContext updatesSync)
        {
            _id = id;
            _pluginRef = pluginRef;
            _syncContext = updatesSync;
            IsIsolated = pluginRef.IsIsolated;

            _container = PluginContainer.Load(_pluginRef.PackagePath, _pluginRef.IsIsolated);
            Launcher = _container.CreateObject<PluginLauncher>();
            Core = Launcher.CreateExecutor(_pluginRef.Id);

            Core.IsGlobalMarshalingEnabled = true;
            Core.IsBunchingRequired = IsIsolated || _syncContext != null;

            Core.MarshalUpdate = MarshalUpdate;
            Core.MarshalUpdates = MarshalUpdatesToContext;
            Core.Stopped += () =>
            {
                if (_syncContext != null)
                    _syncContext.Invoke(() => Stopped?.Invoke(this));
                else
                    Stopped?.Invoke(this);
            };
        }

        internal PluginExecutorCore Core { get; private set; }
        internal PluginLauncher Launcher { get; private set; }
        public bool IsIsolated { get; }
        //public bool IsRunning { get; private set; }

        public IAccountInfoProvider AccInfoProvider { get; set; }
        public ITradeHistoryProvider TradeHistoryProvider { get; set; }
        public IPluginMetadata Metadata { get; set; }
        public IFeedProvider Feed { get; set; }
        public IFeedHistoryProvider FeedHistory { get; set; }
        public ITradeExecutor TradeExecutor { get; set; }
        public PluginExecutorConfig Config { get; } = new PluginExecutorConfig();

        public event Action<PluginLogRecord> LogUpdated;
        public event Action<TesterTradeTransaction> TradesUpdated;
        public event Action<TradeReportEntity> TradeHistoryUpdated;
        public event Action<RateUpdate> SymbolRateUpdated;
        public event Action<BarEntity, string, SeriesUpdateActions> ChartBarUpdated;
        public event Action<BarEntity, SeriesUpdateActions> EquityUpdated;
        public event Action<BarEntity, SeriesUpdateActions> MarginUpdated;
        public event Action<IDataSeriesUpdate> OutputUpdate;
        public event Action<Exception> ErrorOccurred;

        public event Action<PluginExecutor> Stopped;

        #region Excec control

        public void Launch(string address, int port)
        {
            Launcher.Launch(address, port, _id);
        }

        internal void Start()
        {
            ConfigurateCore();
            Launcher.ConfigureCore();
            Core.Start();
        }

        public void Stop()
        {
            Core.Stop();
        }

        public Task StopAsync()
        {
            var taskSrc = new CrossDomainTaskProxy();
            Core.StopAsync(taskSrc);
            return taskSrc.Task;
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

        public void WriteConnectionInfo(string connectionInfo)
        {
            Core.WriteConnectionInfo(connectionInfo);
        }

        #endregion

        public override void Dispose()
        {
            DisposeProxies();
            _container.Dispose();
            base.Dispose();
        }

        private void DisposeProxies()
        {
            _cProxy?.Dispose();
            _fProxy?.Dispose();
            _tProxy?.Dispose();
            _cProxy = null;
            _fProxy = null;
            _tProxy = null;
        }

        //public override void Dispose()
        //{
        //}

        private void ConfigurateCore()
        {
            DisposeProxies();

            if (AccInfoProvider == null)
                throw new ExecutorException("AccInfoProvider is not set!");

            if (Metadata == null)
                throw new ExecutorException("Metadata is not set!");

            if (TradeHistoryProvider == null)
                throw new ExecutorException("TradeHistoryProvider is not set!");

            if (Feed == null)
                throw new ExecutorException("Feed is not set!");

            if (FeedHistory == null)
                throw new ExecutorException("FeedHistory is not set!");

            if (IsIsolated)
            {
                _cProxy = new CommonCdProxy(AccInfoProvider, Metadata, TradeHistoryProvider);
                _fProxy = new FeedCdProxy(Feed, FeedHistory);

                if (TradeExecutor != null)
                    _tProxy = new TradeApiProxy(TradeExecutor);

                Core.ApplyConfig(Config, _cProxy, _cProxy, _cProxy, _fProxy, _fProxy, _tProxy);
            }
            else
                Core.ApplyConfig(Config, AccInfoProvider, Metadata, TradeHistoryProvider, Feed, FeedHistory, TradeExecutor);
        }

        #region Update Marshalling 

        private void MarshalUpdatesToContext(IReadOnlyList<object> updates)
        {
            if (_syncContext != null)
                _syncContext.Invoke(MarshalUpdates, updates);
            else
                MarshalUpdates(updates);
        }

        private void MarshalUpdates(IReadOnlyList<object> updates)
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
        }

        private void MarshalUpdate(object update)
        {
            if (update is PluginLogRecord)
                LogUpdated?.Invoke((PluginLogRecord)update);
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
            else if (update is Exception)
                ErrorOccurred?.Invoke((Exception)update);

            #endregion
        }
    }
}
