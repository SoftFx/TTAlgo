using Caliburn.Micro;
using Machinarium.State;
using NLog;
using SoftFX.Extended;
using SoftFX.Extended.Reports;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using TickTrader.BotTerminal.Lib;

namespace TickTrader.BotTerminal
{
    internal class AccountModel
    {
        private Logger logger;
        public enum States { Offline, WaitingData, Canceled, Online, Deinitializing }
        public enum Events { Connected, ConnectionCanceled, CacheInitialized, Diconnected, DoneInit, DoneDeinit }

        private StateMachine<States> stateControl = new StateMachine<States>(new DispatcherStateMachineSync());
        private ObservableDictionary<string, PositionModel> positions = new ObservableDictionary<string, PositionModel>();
        private ObservableDictionary<string, AssetModel> assets = new ObservableDictionary<string, AssetModel>();
        private ObservableDictionary<string, OrderModel> orders = new ObservableDictionary<string, OrderModel>();
        private ConnectionModel connection;
        private ActionBlock<System.Action> uiUpdater;
        private AccountType? accType;
        //private RepeatableActivity initActivity;

        public AccountModel(ConnectionModel connection)
        {
            logger = NLog.LogManager.GetCurrentClassLogger();

            this.connection = connection;
            this.Positions = positions.AsReadonly();
            this.Orders = orders.AsReadonly();
            this.Assets = assets.AsReadonly();

            //this.initActivity = new RepeatableActivity(Init);
            TradeHistory = new TradeHistoryProvider(connection);

            stateControl.AddTransition(States.Offline, Events.Connected, States.WaitingData);
            stateControl.AddTransition(States.WaitingData, Events.CacheInitialized, States.Online);
            stateControl.AddTransition(States.WaitingData, Events.Diconnected, States.Offline);
            stateControl.AddTransition(States.Online, Events.Diconnected, States.Deinitializing);
            stateControl.AddTransition(States.Deinitializing, Events.DoneDeinit, States.Offline);

            stateControl.OnEnter(States.WaitingData, Init);
            stateControl.OnEnter(States.Online, UpdateSnapshots);
            stateControl.OnEnter(States.Deinitializing, Deinit);

            connection.Connecting += () =>
                {
                    connection.TradeProxy.CacheInitialized += TradeProxy_CacheInitialized;
                    connection.TradeProxy.AccountInfo += AccountInfoChanged;
                    connection.TradeProxy.ExecutionReport += TradeProxy_ExecutionReport;
                    connection.TradeProxy.PositionReport += TradeProxy_PositionReport;
                };

            connection.Disconnecting += () =>
            {
                connection.TradeProxy.CacheInitialized -= TradeProxy_CacheInitialized;
                connection.TradeProxy.AccountInfo -= AccountInfoChanged;
                connection.TradeProxy.ExecutionReport -= TradeProxy_ExecutionReport;
                connection.TradeProxy.PositionReport -= TradeProxy_PositionReport;
            };

            connection.Connected += () => stateControl.PushEvent(Events.Connected);

            //connection.Initalizing += (s, c) =>
            //{
            //    c.Register(() => stateControl.PushEvent(Events.ConnectionCanceled));
            //    return stateControl.PushEventAndWait(Events.Connected, state => state == States.Online || state == States.Canceled);
            //};

            connection.Deinitalizing += (s, c) => stateControl.PushEventAndWait(Events.Diconnected, States.Offline);

            stateControl.StateChanged += (from, to) => logger.Debug("STATE " + from + " => " + to);
            stateControl.EventFired += e => logger.Debug("EVENT " + e);
        }

        public event System.Action AccountTypeChanged = delegate { };
        public ReadonlyDictionaryObserver<string, PositionModel> Positions { get; private set; }
        public ReadonlyDictionaryObserver<string, OrderModel> Orders { get; private set; }
        public ReadonlyDictionaryObserver<string, AssetModel> Assets { get; private set; }
        public TradeHistoryProvider TradeHistory { get; private set; }
        public AccountType? Type
        {
            get { return accType; }
            private set
            {
                if (accType != value)
                {
                    accType = value;
                    AccountTypeChanged();
                }
            }
        }
        public IStateProvider<States> State { get { return stateControl; } }

        public void Init()
        {
            this.uiUpdater = DataflowHelper.CreateUiActionBlock<System.Action>(a => a(), 100, 100, CancellationToken.None);
            positions.Clear();
            orders.Clear();
            assets.Clear();
        }

        public void UpdateSnapshots()
        {
            Type = connection.TradeProxy.Cache.AccountInfo.Type;

            var fdkPositionsArray = connection.TradeProxy.Cache.Positions;
            foreach (var fdkPosition in fdkPositionsArray)
                positions.Add(fdkPosition.Symbol, new PositionModel(fdkPosition));

            var fdkOrdersArray = connection.TradeProxy.Cache.TradeRecords;
            foreach (var fdkOrder in fdkOrdersArray)
                orders.Add(fdkOrder.OrderId, new OrderModel(fdkOrder));

            var fdkAssetsArray = connection.TradeProxy.Cache.AccountInfo.Assets;
            foreach (var fdkAsset in fdkAssetsArray)
                assets.Add(fdkAsset.Currency, new AssetModel(fdkAsset));

            stateControl.PushEvent(Events.DoneInit);
        }

        public async void Deinit()
        {
            await Task.Factory.StartNew(() =>
                {
                    uiUpdater.Complete();
                    uiUpdater.Completion.Wait();
                });

            stateControl.PushEvent(Events.DoneDeinit);
        }

        private void UpsertOrder(ExecutionReport report)
        {
            OrderModel order;
            if (orders.TryGetValue(report.OrderId, out order))
                order.Update(report);
            else
                orders.Add(report.OrderId, new OrderModel(report));
        }

        private void UpsertPosition(Position report)
        {
            PositionModel position;
            if (positions.TryGetValue(report.Symbol, out position))
                position.Update(report);
            else
                positions.Add(report.Symbol, new PositionModel(report));
        }

        void TradeProxy_PositionReport(object sender, SoftFX.Extended.Events.PositionReportEventArgs e)
        {
            if (IsEmpty(e.Report))
                uiUpdater.SendAsync(() => positions.Remove(e.Report.Symbol));
            else
                uiUpdater.SendAsync(() => UpsertPosition(e.Report));
        }

        void TradeProxy_ExecutionReport(object sender, SoftFX.Extended.Events.ExecutionReportEventArgs e)
        {
            var execType = e.Report.ExecutionType;

            if (execType == ExecutionType.Canceled
                || execType == ExecutionType.Expired
                || e.Report.LeavesVolume == 0)
            {
                uiUpdater.SendAsync(() => orders.Remove(e.Report.OrderId));
            }
            else if (execType == ExecutionType.Calculated
                || execType == ExecutionType.Replace
                || execType == ExecutionType.Trade
                || execType == ExecutionType.PendingCancel
                || execType == ExecutionType.PendingReplace)
            {
                uiUpdater.SendAsync(() => UpsertOrder(e.Report));
            }

            if (Type == AccountType.Cash)
                uiUpdater.SendAsync(() =>
                {
                    foreach (var asset in e.Report.Assets)
                        UpdateAsset(asset);
                });
        }

        private void UpdateAsset(AssetInfo assetInfo)
        {
            AssetModel asset;
            if (assets.TryGetValue(assetInfo.Currency, out asset))
            {
                if (IsEmpty(assetInfo))
                    assets.Remove(asset.Currency);
                else
                    asset.Update(assetInfo);
            }
            else
                assets.Add(assetInfo.Currency, new AssetModel(assetInfo));
        }

        void AccountInfoChanged(object sender, SoftFX.Extended.Events.AccountInfoEventArgs e)
        {
            Type = e.Information.Type;
        }

        void TradeProxy_CacheInitialized(object sender, SoftFX.Extended.Events.CacheEventArgs e)
        {
            stateControl.PushEvent(Events.CacheInitialized);
        }

        private bool IsEmpty(Position position)
        {
            return position.BuyAmount == 0
               && position.SellAmount == 0;
        }
        private bool IsEmpty(AssetInfo assetInfo)
        {
            return assetInfo.Balance == 0;
        }
    }

    internal class TradeHistoryProvider
    {
        private Logger _logger;
        private ConnectionModel _connectionModel;

        public event Action<TradeTransactionModel> OnTradeReport = delegate { };

        public TradeHistoryProvider(ConnectionModel connectionModel)
        {
            _logger = NLog.LogManager.GetCurrentClassLogger();

            _connectionModel = connectionModel;

            _connectionModel.Connecting += () => { _connectionModel.TradeProxy.TradeTransactionReport += TradeTransactionReport; };
            _connectionModel.Disconnecting += () => { _connectionModel.TradeProxy.TradeTransactionReport -= TradeTransactionReport; };
        }

        public Task<TradeTransactionModel[]> DownloadHistoryAsync(DateTime from, DateTime to)
        {
            return DownloadHistoryAsync(from, to, CancellationToken.None);
        }
        public Task<TradeTransactionModel[]> DownloadHistoryAsync(DateTime from, DateTime to, CancellationToken token)
        {
            return DownloadHistoryAsync(from, to, CancellationToken.None, null);
        }
        public Task<TradeTransactionModel[]> DownloadHistoryAsync(DateTime from, DateTime to, CancellationToken token, IProgress<TradeTransactionModel> progress)
        {
            return StartDownloadingHistory(from, to, token, progress);
        }

        private Task<TradeTransactionModel[]> StartDownloadingHistory(DateTime from, DateTime to, CancellationToken token, IProgress<TradeTransactionModel> progress)
        {
            return Task.Run(() =>
            {
                try
                {
                    var tradesList = new List<TradeTransactionModel>();
                    token.ThrowIfCancellationRequested();

                    var historyStream = _connectionModel.TradeProxy.Server.GetTradeTransactionReports(TimeDirection.Forward, true, from, to);

                    while (!historyStream.EndOfStream)
                    {
                        token.ThrowIfCancellationRequested();

                        var historyItem = new TradeTransactionModel(historyStream.Item);
                        tradesList.Add(historyItem);
                        progress?.Report(historyItem);

                        historyStream.Next();
                    }

                    return tradesList.ToArray();
                }
                catch(OperationCanceledException) { throw; }
                catch(Exception ex) { _logger.Error(ex, "DownloadHistoryAsync FAILED"); throw; }
            }, token);

        }
        private void TradeTransactionReport(object sender, SoftFX.Extended.Events.TradeTransactionReportEventArgs e)
        {
            OnTradeReport(new TradeTransactionModel(e.Report));
        }
    }


}
