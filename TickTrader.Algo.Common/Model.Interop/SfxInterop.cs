using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TickTrader.Algo.Api;
using TickTrader.Algo.Common.Lib;
using TickTrader.Algo.Common.Model.Interop;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Lib;
using TickTrader.FDK.Common;
using SFX = TickTrader.FDK.Common;
using API = TickTrader.Algo.Api;
using BO = TickTrader.BusinessObjects;
using TickTrader.FDK.QuoteStore;
using TickTrader.FDK.TradeCapture;
using ActorSharp;

namespace TickTrader.Algo.Common.Model
{
    internal class SfxInterop : IServerInterop, IFeedServerApi, ITradeServerApi
    {
        private const int ConnectTimeoutMs = 60 * 1000;
        private const int LogoutTimeoutMs = 60 * 1000;
        private const int DisconnectTimeoutMs = 60 * 1000;
        private const int DownloadTimeoutMs = 120 * 1000;

        private static IAlgoCoreLogger logger = CoreLoggerFactory.GetLogger<SfxInterop>();

        public IFeedServerApi FeedApi => this;
        public ITradeServerApi TradeApi => this;

        public bool AutoAccountInfo => false;
        public bool AutoSymbols => false;

        private FDK.QuoteFeed.Client _feedProxy;
        private FDK.QuoteStore.Client _feedHistoryProxy;
        private FDK.OrderEntry.Client _tradeProxy;
        private FDK.TradeCapture.Client _tradeHistoryProxy;

        public event Action<IServerInterop, ConnectionErrorInfo> Disconnected;
        
        public SfxInterop(ConnectionOptions options)
        {
            const int connectInterval = 10000;
#if DEBUG
            const int heartbeatInterval = 120000;
#else
            const int heartbeatInterval = 10000;
#endif
            const int connectAttempts = 1;
            const int reconnectAttempts = 0;

            _feedProxy = new FDK.QuoteFeed.Client("feed.proxy", options.EnableLogs, 5030, connectAttempts, reconnectAttempts, connectInterval, heartbeatInterval, options.LogsFolder);
            _feedHistoryProxy = new FDK.QuoteStore.Client("feed.history.proxy", options.EnableLogs, 5050, connectAttempts, reconnectAttempts, connectInterval, heartbeatInterval, options.LogsFolder);
            _tradeProxy = new FDK.OrderEntry.Client("trade.proxy", options.EnableLogs, 5040, connectAttempts, reconnectAttempts, connectInterval, heartbeatInterval, options.LogsFolder);
            _tradeHistoryProxy = new FDK.TradeCapture.Client("trade.history.proxy", options.EnableLogs, 5060, connectAttempts, reconnectAttempts, connectInterval, heartbeatInterval, options.LogsFolder);

            _feedProxy.InitTaskAdapter();
            _tradeProxy.InitTaskAdapter();
            _feedHistoryProxy.InitTaskAdapter();
            _tradeHistoryProxy.InitTaskAdapter();

            _feedProxy.QuoteUpdateEvent += (c, q) => Tick?.Invoke(Convert(q));
            _feedProxy.DisconnectEvent += (c, m) => OnDisconnect(m);
            _tradeProxy.DisconnectEvent += (c, m) => OnDisconnect(m);
            _tradeProxy.OrderUpdateEvent += (c, rep) => ExecutionReport?.Invoke(ConvertToEr(rep));
            _tradeProxy.PositionUpdateEvent += (c, rep) => PositionReport?.Invoke(Convert(rep));
            _tradeProxy.BalanceUpdateEvent += (c, rep) => BalanceOperation?.Invoke(Convert(rep));
            _tradeHistoryProxy.DisconnectEvent += (c, m) => OnDisconnect(m);
            _tradeHistoryProxy.TradeUpdateEvent += (c, rep) => TradeTransactionReport?.Invoke(Convert(rep));
            _feedHistoryProxy.DisconnectEvent += (c, m) => OnDisconnect(m);
        }

        public async Task<ConnectionErrorInfo> Connect(string address, string login, string password, CancellationToken cancelToken)
        {
            try
            {
                await Task.WhenAll(
                    ConnectFeed(address, login, password),
                    ConnectTrade(address, login, password),
                    ConnectFeedHistory(address, login, password),
                    ConnectTradeHistory(address, login, password))
                    .AddCancelation(cancelToken);
            }
            catch (AggregateException aex)
            {
                var ex = aex.InnerExceptions.First();
                if (ex is LoginException)
                {
                    var code = ((LoginException)ex).LogoutReason;
                    return new ConnectionErrorInfo(Convert(code), ex.Message);
                }

                return new ConnectionErrorInfo(ConnectionErrorCodes.Unknown, ex.Message);
            }
            catch (Exception ex)
            {
                return new ConnectionErrorInfo(ConnectionErrorCodes.Unknown, ex.Message);
            }

            return new ConnectionErrorInfo(ConnectionErrorCodes.None);
        }

        private async Task ConnectFeed(string address, string login, string password)
        {
            logger.Debug("Feed: Connecting...");
            await _feedProxy.ConnectAsync(address);
            logger.Debug("Feed: Connected.");
            await _feedProxy.LoginAsync(login, password, "", "", Guid.NewGuid().ToString());
            logger.Debug("Feed: Logged in.");
        }

        private async Task ConnectTrade(string address, string login, string password)
        {
            logger.Debug("Trade: Connecting...");
            await _tradeProxy.ConnectAsync(address);
            logger.Debug("Trade: Connected.");
            await _tradeProxy.LoginAsync(login, password, "", "", Guid.NewGuid().ToString());
            logger.Debug("Trade logged in.");
        }

        private async Task ConnectFeedHistory(string address, string login, string password)
        {
            logger.Debug("Feed.History: Connecting...");
            await _feedHistoryProxy.ConnectAsync(address);
            logger.Debug("Feed.History: Connected.");
            await _feedHistoryProxy.LoginAsync(login, password, "", "", Guid.NewGuid().ToString());
            logger.Debug("Feed.History: Logged in.");
        }

        private async Task ConnectTradeHistory(string address, string login, string password)
        {
            logger.Debug("Trade.History: Connecting...");
            await _tradeHistoryProxy.ConnectAsync(address);
            logger.Debug("Trade.History: Connected.");
            await _tradeHistoryProxy.LoginAsync(login, password, "", "", Guid.NewGuid().ToString());
            logger.Debug("Trade.History: Logged in.");
            await _tradeHistoryProxy.SubscribeTradesAsync(false);
            logger.Debug("Trade.History: Subscribed.");
        }

        private void OnDisconnect(string text)
        {
            Disconnected?.Invoke(this, new ConnectionErrorInfo(ConnectionErrorCodes.Unknown, text));
        }

        public Task Disconnect()
        {
            return Task.WhenAll(
                DisconnectFeed(),
                DisconnectTrade(),
                DisconnectFeedHstory(),
                DisconnectTradeHstory());
        }

        private async Task DisconnectFeed()
        {
            await Task.Factory.StartNew(() =>
            {
                try
                {
                    _feedProxy.Logout("", -1);
                }
                catch (Exception)
                {
                    _feedProxy.Disconnect("");
                }

                _feedProxy.Dispose();

                logger.Debug("Feed dicconnected.");
            });
        }

        private async Task DisconnectFeedHstory()
        {
            await Task.Factory.StartNew(() =>
            {
                try
                {
                    _feedHistoryProxy.Logout("", -1);
                }
                catch (Exception)
                {
                    _feedHistoryProxy.Disconnect("");
                }

                _feedHistoryProxy.Dispose();

                logger.Debug("Feed history dicconnected.");
            });
        }

        private async Task DisconnectTrade()
        {
            await Task.Factory.StartNew(() =>
            {
                try
                {
                    _tradeProxy.Logout("", -1);
                }
                catch (Exception)
                {
                    _tradeProxy.Disconnect("");
                }

                _tradeProxy.Dispose();

                logger.Debug("Trade dicconnected.");
            });
        }

        private async Task DisconnectTradeHstory()
        {
            await Task.Factory.StartNew(() =>
            {
                try
                {
                    _tradeHistoryProxy.Logout("", -1);
                }
                catch (Exception)
                {
                    _tradeHistoryProxy.Disconnect("");
                }

                _tradeHistoryProxy.Dispose();

                logger.Debug("Trade history dicconnected.");
            });
        }

        #region IFeedServerApi

        public event Action<QuoteEntity> Tick;

        public async Task<CurrencyEntity[]> GetCurrencies()
        {
            var currencies = await _feedProxy.GetCurrencyListAsync();
            return currencies.Select(Convert).ToArray();
        }

        public async Task<SymbolEntity[]> GetSymbols()
        {
            var symbols = await _feedProxy.GetSymbolListAsync();
            return symbols.Select(Convert).ToArray();
        }

        public Task<QuoteEntity[]> SubscribeToQuotes(string[] symbols, int depth)
        {
            return _feedProxy.SubscribeQuotesAsync(symbols, depth);
        }

        public async Task<QuoteEntity[]> GetQuoteSnapshot(string[] symbols, int depth)
        {
            var array = await _feedProxy.GetQuotesAsync(symbols, depth);
            return array.Select(Convert).ToArray();
        }

        public void DownloadBars(BlockingChannel<BarEntity> stream, string symbol, DateTime from, DateTime to, BarPriceType priceType, TimeFrames barPeriod)
        {
            Task.Factory.StartNew(() =>
            {
                try
                {
                    var e = _feedHistoryProxy.DownloadBars(symbol, ConvertBack(priceType), ToBarPeriod(barPeriod), from.ToUniversalTime(), to.ToUniversalTime(), DownloadTimeoutMs);

                    while (true)
                    {
                        var bar = e.Next(DownloadTimeoutMs);
                        if (bar == null || !stream.Write(Convert(bar)))
                        {
                            e.Close();
                            break;
                        }
                    }
                    stream.Close();
                }
                catch (Exception ex)
                {
                    stream.Close(ex);
                }
            });
        }

        public async Task<BarEntity[]> DownloadBarPage(string symbol, DateTime from, int count, BarPriceType priceType, TimeFrames barPeriod)
        {
            var result = new List<BarEntity>();

            try
            {
                var bars = await _feedHistoryProxy.GetBarListAsync(symbol, ConvertBack(priceType), ToBarPeriod(barPeriod), from.ToUniversalTime(), count);
                return bars.Select(Convert).ToArray();
            }
            catch (Exception ex)
            {
                throw new InteropException(ex.Message, ConnectionErrorCodes.NetworkError);
            }
        }

        public void DownloadQuotes(BlockingChannel<QuoteEntity> stream, string symbol, DateTime from, DateTime to, bool includeLevel2)
        {
            throw new NotImplementedException();

            //var buffer = new QuoteSliceBuffer(from, to);
            //var depth = includeLevel2 ? QuoteDepth.Level2 : QuoteDepth.Top;
            //var eTask = _feedHistoryProxy.DownloadQuotesAsync(Guid.NewGuid().ToString(), symbol, depth, from, to);
            //DownloadQuotesToBuffer(buffer, eTask);
            //return buffer;
        }

        //private async void DownloadQuotesToBuffer(SliceBuffer<QuoteEntity> buffer, Task<QuoteEnumerator> enumTask)
        //{
        //    const int pageSize = 2000;

        //    DateTime lastTickTime = DateTime.MinValue;

        //    try
        //    {
        //        using (var e = await enumTask)
        //        {
        //            var page = new SFX.Quote[pageSize];

        //            while (true)
        //            {
        //                var count = await e.NextAsync(page).ConfigureAwait(false);
        //                if (count <= 0)
        //                    break;

        //                var tickArray = ConvertAndFilter(page.Take(count), ref lastTickTime);
        //                await buffer.WriteAsync(tickArray).ConfigureAwait(false);
        //            }
        //        }

        //        await buffer.CompleteWriteAsync();
        //        buffer.Dispose();
        //    }
        //    catch (Exception ex)
        //    {
        //        buffer.SetFailed(ex);
        //    }
        //}

        public Task<QuoteEntity[]> DownloadQuotePage(string symbol, DateTime from, int count, bool includeLevel2)
        {
            throw new NotImplementedException();
        }

        public Task<Tuple<DateTime, DateTime>> GetAvailableRange(string symbol, BarPriceType priceType, TimeFrames timeFrame)
        {
            return Task.Factory.StartNew(() =>
            {
                var e = _feedHistoryProxy.DownloadBars(symbol, ConvertBack(priceType), ToBarPeriod(timeFrame), DateTime.MinValue.ToUniversalTime(), DateTime.MaxValue.ToUniversalTime(), DownloadTimeoutMs);
                e.Close();
                return new Tuple<DateTime, DateTime>(e.AvailFrom, e.AvailTo);
            });
        }

        #endregion

        #region ITradeServerApi

        public event Action<PositionEntity> PositionReport;
        public event Action<ExecutionReport> ExecutionReport;
        public event Action<TradeReportEntity> TradeTransactionReport;
        public event Action<BalanceOperationReport> BalanceOperation;
        public event Action<SymbolEntity[]> SymbolInfo;
        public event Action<CurrencyEntity[]> CurrencyInfo;

        public Task<AccountEntity> GetAccountInfo()
        {
            return _tradeProxy.GetAccountInfoAsync()
                .ContinueWith(t => Convert(t.Result));
        }

        public void GetTradeRecords(BlockingChannel<OrderEntity> rxStream)
        {
            _tradeProxy.GetOrdersAsync(rxStream);
        }

        public Task<PositionEntity[]> GetPositions()
        {
            return _tradeProxy.GetPositionsAsync()
                .ContinueWith(t => t.Result.Select(Convert).ToArray());
        }

        public void GetTradeHistory(BlockingChannel<TradeReportEntity> rxStream, DateTime? from, DateTime? to, bool skipCancelOrders)
        {
            _tradeHistoryProxy.DownloadTradesAsync(TimeDirection.Forward, from?.ToUniversalTime(), to?.ToUniversalTime(), skipCancelOrders, rxStream);
        }

        public Task<OrderInteropResult> SendOpenOrder(OpenOrderRequest request)
        {
            return ExecuteOrderOperation(request, r =>
            {
                var timeInForce = GetTimeInForce(r.Options, r.Expiration);
                var clientOrderId = Guid.NewGuid().ToString();

                return _tradeProxy.NewOrderAsync(clientOrderId, r.Symbol, Convert(r.Type), Convert(r.Side), r.Volume, r.MaxVisibleVolume,
                    r.Price, r.StopPrice, timeInForce, r.Expiration, r.StopLoss, r.TakeProfit, r.Comment, r.Tag, null);
            });
        }

        public Task<OrderInteropResult> SendCancelOrder(CancelOrderRequest request)
        {
            return ExecuteOrderOperation(request, r => _tradeProxy.CancelOrderAsync(r.OperationId, "", r.OrderId));
        }

        public Task<OrderInteropResult> SendModifyOrder(ReplaceOrderRequest request)
        {
            return ExecuteOrderOperation(request, r => _tradeProxy.ReplaceOrderAsync(r.OperationId, "",
                r.OrderId, r.Symbol, Convert(r.Type), Convert(r.Side), r.NewVolume ?? r.CurrentVolume, r.CurrentVolume,
                r.MaxVisibleVolume, r.Price, r.StopPrice, GetTimeInForceReplace(r.Options, r.Expiration), r.Expiration,
                r.StopLoss, r.TrakeProfit, r.Comment, r.Tag, null));
        }

        public Task<OrderInteropResult> SendCloseOrder(CloseOrderRequest request)
        {
            return ExecuteOrderOperation(request, r =>
            {
                if (request.ByOrderId != null)
                    return _tradeProxy.ClosePositionByAsync(r.OperationId, r.OrderId, r.ByOrderId);
                else
                    return _tradeProxy.ClosePositionAsync(r.OperationId, r.OrderId, r.Volume);
            });
        }

        private async Task<OrderInteropResult> ExecuteOrderOperation<TReq>(TReq request, Func<TReq, Task<List<SFX.ExecutionReport>>> operationDef)
            where TReq : OrderRequest
        {
            var operationId = request.OperationId;

            try
            {
                var result = await operationDef(request);
                return new OrderInteropResult(OrderCmdResultCodes.Ok, ConvertToEr(result, operationId));
            }
            catch (ExecutionException eex)
            {
                var reason = Convert(eex.Report.RejectReason, eex.Message);
                return new OrderInteropResult(reason, ConvertToEr(eex.Report, operationId));
            }
            catch (DisconnectException)
            {
                return new OrderInteropResult(OrderCmdResultCodes.ConnectionError);
            }
            catch (Exception ex)
            {
                if (ex.Message.StartsWith("Session is not active"))
                    return new OrderInteropResult(OrderCmdResultCodes.ConnectionError);
                logger.Error(ex);
                return new OrderInteropResult(OrderCmdResultCodes.UnknownError);
            }
        }

        private OrderTimeInForce? GetTimeInForceReplace(OrderExecOptions? options, DateTime? expiration)
        {
            if (options != null)
            {
                if (options.Value.HasFlag(OrderExecOptions.ImmediateOrCancel))
                    return OrderTimeInForce.ImmediateOrCancel;
                else if (expiration != null)
                    return OrderTimeInForce.GoodTillDate;
                else
                    return OrderTimeInForce.GoodTillCancel;
            }
            else
            {
                if (expiration != null)
                    return OrderTimeInForce.GoodTillDate;
                else
                    return null;
            }
        }

        private OrderTimeInForce GetTimeInForce(OrderExecOptions options, DateTime? expiration)
        {
            if (options.IsFlagSet(OrderExecOptions.ImmediateOrCancel))
                return OrderTimeInForce.ImmediateOrCancel;
            else if (expiration != null)
                return OrderTimeInForce.GoodTillDate;
            return OrderTimeInForce.GoodTillCancel;
        }

        private OrderTimeInForce GetTimeInForce(DateTime? expiration)
        {
            if (expiration == null)
                return OrderTimeInForce.GoodTillCancel;
            else
                return OrderTimeInForce.GoodTillDate;
        }

        #endregion

        #region Convertors

        private static SymbolEntity Convert(SymbolInfo info)
        {
            return new SymbolEntity(info.Name)
            {
                Digits = info.Precision,
                LotSize = info.RoundLot,
                MinAmount = info.RoundLot != 0 ? info.MinTradeVolume / info.RoundLot : double.NaN,
                MaxAmount = info.RoundLot != 0 ? info.MaxTradeVolume / info.RoundLot : double.NaN,
                AmountStep = info.RoundLot != 0 ? info.TradeVolumeStep / info.RoundLot : double.NaN,
                BaseCurrencyCode = info.Currency,
                CounterCurrencyCode = info.SettlementCurrency,
                IsTradeAllowed = info.IsTradeEnabled,
                Commission = info.Commission,
                LimitsCommission = info.LimitsCommission,
                CommissionChargeMethod = Convert(info.CommissionChargeMethod),
                CommissionChargeType = Convert(info.CommissionChargeType),
                CommissionType = Convert(info.CommissionType),
                ContractSizeFractional = info.RoundLot,
                MarginFactorFractional = info.MarginFactorFractional ?? 1,
                StopOrderMarginReduction = info.StopOrderMarginReduction ?? 0,
                MarginHedged = info.MarginHedge,
                MarginMode = Convert(info.MarginCalcMode),
                SwapEnabled = info.IsSwapEnabled(),
                SwapSizeLong = (float)(info.SwapSizeLong ?? 0),
                SwapSizeShort = (float)(info.SwapSizeShort ?? 0),
                Security = info.SecurityName,
                GroupSortOrder = info.GroupSortOrder,
                SortOrder = info.SortOrder,
                SwapType = Convert(info.SwapType),
                TripleSwapDay = info.TripleSwapDay,
                IsTradeEnabled = info.IsTradeEnabled,
                Description = info.Description,
                DefaultSlippage = info.DefaultSlippage,
                HiddenLimitOrderMarginReduction = info.HiddenLimitOrderMarginReduction                
            };
        }

        private static BO.SwapType Convert(SwapType type)
        {
            switch (type)
            {
                case SwapType.PercentPerYear: return BO.SwapType.PercentPerYear;
                case SwapType.Points: return BO.SwapType.Points;
                default: throw new NotImplementedException();
            }
        }

        private static Api.CommissionChargeType Convert(SFX.CommissionChargeType fdkChargeType)
        {
            switch (fdkChargeType)
            {
                case SFX.CommissionChargeType.PerLot: return Api.CommissionChargeType.PerLot;
                case SFX.CommissionChargeType.PerTrade: return Api.CommissionChargeType.PerTrade;

                default: throw new ArgumentException("Unsupported commission charge type: " + fdkChargeType);
            }
        }

        private static Api.CommissionChargeMethod Convert(SFX.CommissionChargeMethod fdkChargeMethod)
        {
            switch (fdkChargeMethod)
            {
                case SFX.CommissionChargeMethod.OneWay: return Api.CommissionChargeMethod.OneWay;
                case SFX.CommissionChargeMethod.RoundTurn: return Api.CommissionChargeMethod.RoundTurn;

                default: throw new ArgumentException("Unsupported commission charge method: " + fdkChargeMethod);
            }
        }

        private static Api.CommissionType Convert(SFX.CommissionType fdkType)
        {
            switch (fdkType)
            {
                case SFX.CommissionType.Absolute: return Api.CommissionType.Absolute;
                case SFX.CommissionType.PerBond: return Api.CommissionType.PerBond;
                case SFX.CommissionType.PerUnit: return Api.CommissionType.PerUnit;
                case SFX.CommissionType.Percent: return Api.CommissionType.Percent;
                case SFX.CommissionType.PercentageWaivedCash: return Api.CommissionType.PercentageWaivedCash;
                case SFX.CommissionType.PercentageWaivedEnhanced: return Api.CommissionType.PercentageWaivedEnhanced;

                default: throw new ArgumentException("Unsupported commission type: " + fdkType);
            }
        }

        private static TickTrader.BusinessObjects.MarginCalculationModes Convert(MarginCalcMode mode)
        {
            switch (mode)
            {
                case MarginCalcMode.Cfd: return BusinessObjects.MarginCalculationModes.CFD;
                case MarginCalcMode.CfdIndex: return BusinessObjects.MarginCalculationModes.CFD_Index;
                case MarginCalcMode.CfdLeverage: return BusinessObjects.MarginCalculationModes.CFD_Leverage;
                case MarginCalcMode.Forex: return BusinessObjects.MarginCalculationModes.Forex;
                case MarginCalcMode.Futures: return BusinessObjects.MarginCalculationModes.Futures;
                default: throw new NotImplementedException();
            }
        }

        private static CurrencyEntity Convert(CurrencyInfo info)
        {
            return new CurrencyEntity(info.Name)
            {
                Digits = info.Precision,
                SortOrder = info.SortOrder
            };
        }

        private static Api.AccountTypes Convert(AccountType fdkType)
        {
            switch (fdkType)
            {
                case AccountType.Cash: return Algo.Api.AccountTypes.Cash;
                case AccountType.Gross: return Algo.Api.AccountTypes.Gross;
                case AccountType.Net: return Algo.Api.AccountTypes.Net;

                default: throw new ArgumentException("Unsupported account type: " + fdkType);
            }
        }

        private static Api.OrderType Convert(SFX.OrderType fdkType)
        {
            switch (fdkType)
            {
                case SFX.OrderType.Limit: return Algo.Api.OrderType.Limit;
                case SFX.OrderType.Market: return Algo.Api.OrderType.Market;
                case SFX.OrderType.Position: return Algo.Api.OrderType.Position;
                case SFX.OrderType.Stop: return Algo.Api.OrderType.Stop;
                case SFX.OrderType.StopLimit: return Algo.Api.OrderType.StopLimit;

                default: throw new ArgumentException("Unsupported order type: " + fdkType);
            }
        }

        private static SFX.OrderType Convert(Api.OrderType type)
        {
            switch (type)
            {
                case API.OrderType.Limit: return SFX.OrderType.Limit;
                case API.OrderType.Market: return SFX.OrderType.Market;
                case API.OrderType.Position: return SFX.OrderType.Position;
                case API.OrderType.Stop: return SFX.OrderType.Stop;
                case API.OrderType.StopLimit: return SFX.OrderType.StopLimit;

                default: throw new ArgumentException("Unsupported order type: " + type);
            }
        }

        private static Api.OrderSide Convert(SFX.OrderSide fdkSide)
        {
            switch (fdkSide)
            {
                case SFX.OrderSide.Buy: return Api.OrderSide.Buy;
                case SFX.OrderSide.Sell: return Api.OrderSide.Sell;

                default: throw new ArgumentException("Unsupported order side: " + fdkSide);
            }
        }

        private static SFX.OrderSide Convert(Api.OrderSide side)
        {
            switch (side)
            {
                case Api.OrderSide.Buy: return SFX.OrderSide.Buy;
                case Api.OrderSide.Sell: return SFX.OrderSide.Sell;

                default: throw new ArgumentException("Unsupported order side: " + side);
            }
        }

        private static BarPeriod ToBarPeriod(Api.TimeFrames timeframe)
        {
            switch (timeframe)
            {
                case Api.TimeFrames.MN: return BarPeriod.MN1;
                case Api.TimeFrames.W: return BarPeriod.W1;
                case Api.TimeFrames.D: return BarPeriod.D1;
                case Api.TimeFrames.H4: return BarPeriod.H4;
                case Api.TimeFrames.H1: return BarPeriod.H1;
                case Api.TimeFrames.M30: return BarPeriod.M30;
                case Api.TimeFrames.M15: return BarPeriod.M15;
                case Api.TimeFrames.M5: return BarPeriod.M5;
                case Api.TimeFrames.M1: return BarPeriod.M1;
                case Api.TimeFrames.S10: return BarPeriod.S10;
                case Api.TimeFrames.S1: return BarPeriod.S1;

                default: throw new ArgumentException("Unsupported time frame: " + timeframe);
            }
        }

        private static AccountEntity Convert(AccountInfo info)
        {
            return new AccountEntity()
            {
                Id = info.AccountId,
                Balance = info.Balance,
                BalanceCurrency = info.Currency,
                Type = Convert(info.Type),
                Leverage = info.Leverage,
                Assets = info.Assets.Select(Convert).ToArray()
            };
        }

        private static AssetEntity Convert(AssetInfo info)
        {
            return new AssetEntity(info.Balance, info.Currency)
            {
                TradeVolume = info.TradeAmount
            };
        }

        public static OrderEntity Convert(SFX.ExecutionReport record)
        {
            return new OrderEntity(record.OrderId)
            {
                Symbol = record.Symbol,
                Comment = record.Comment,
                Type = Convert(record.OrderType),
                ClientOrderId = record.ClientOrderId,
                Price = record.Price,
                StopPrice = record.StopPrice,
                Side = Convert(record.OrderSide),
                Created = record.Created,
                Swap = record.Swap,
                Modified = record.Modified,
                StopLoss = record.StopLoss,
                TakeProfit = record.TakeProfit,
                Commission = record.Commission,
                ExecVolume = record.ExecutedVolume,
                UserTag = record.Tag,
                RemainingVolume = record.LeavesVolume,
                RequestedVolume = record.InitialVolume,
                Expiration = record.Expiration?.ToLocalTime(),
                MaxVisibleVolume = record.MaxVisibleVolume,
                ExecPrice = record.AveragePrice,
                Options = GetOptions(record),
                LastFillPrice = record.TradePrice,
                LastFillVolume = record.TradeAmount,
            };
        }

        private static OrderExecOptions GetOptions(SFX.ExecutionReport record)
        {
            var isLimit = record.OrderType == SFX.OrderType.Limit || record.OrderType == SFX.OrderType.StopLimit;
            if (isLimit && record.OrderTimeInForce == OrderTimeInForce.ImmediateOrCancel)
                return OrderExecOptions.ImmediateOrCancel;
            return OrderExecOptions.None;
        }

        private static List<ExecutionReport> ConvertToEr(List<SFX.ExecutionReport> reports, string operationId = null)
        {
            var result = new List<ExecutionReport>(reports.Count);
            for (int i = 0; i < reports.Count; i++)
                result.Add(ConvertToEr(reports[i], operationId));
            return result;
        }

        private static ExecutionReport ConvertToEr(SFX.ExecutionReport report, string operationId = null)
        {
            if (report.ExecutionType == SFX.ExecutionType.Rejected && report.RejectReason == RejectReason.None)
                report.RejectReason = RejectReason.Other; // Some plumbing. Sometimes we recieve Rejects with no RejectReason

            return new ExecutionReport()
            {
                OrderId = report.OrderId,
                TradeRequestId = operationId,
                Expiration = report.Expiration?.ToLocalTime(),
                Created = report.Created,
                Modified = report.Modified,
                RejectReason = Convert(report.RejectReason, report.Text ?? ""),
                TakeProfit = report.TakeProfit,
                StopLoss = report.StopLoss,
                Text = report.Text,
                Comment = report.Comment,
                Tag = report.Tag,
                Magic = report.Magic,
                IsReducedOpenCommission = report.ReducedOpenCommission,
                IsReducedCloseCommission = report.ReducedCloseCommission,
                ImmediateOrCancel = report.OrderTimeInForce == OrderTimeInForce.ImmediateOrCancel,
                MarketWithSlippage = report.MarketWithSlippage,
                TradePrice = report.TradePrice ?? 0,
                Assets = report.Assets.Select(Convert).ToArray(),
                StopPrice = report.StopPrice,
                AveragePrice = report.AveragePrice,
                ClientOrderId = report.ClientOrderId,
                OrderStatus = Convert(report.OrderStatus),
                ExecutionType = Convert(report.ExecutionType),
                Symbol = report.Symbol,
                ExecutedVolume = report.ExecutedVolume,
                InitialVolume = report.InitialVolume,
                LeavesVolume = report.LeavesVolume,
                MaxVisibleVolume = report.MaxVisibleVolume,
                TradeAmount = report.TradeAmount,
                Commission = report.Commission,
                AgentCommission = report.AgentCommission,
                Swap = report.Swap,
                OrderType = Convert(report.OrderType),
                OrderSide = Convert(report.OrderSide),
                Price = report.Price,
                Balance = report.Balance ?? 0,
            };
        }

        private static ExecutionType Convert(SFX.ExecutionType type)
        {
            return (ExecutionType)type;
        }

        private static OrderStatus Convert(SFX.OrderStatus status)
        {
            return (OrderStatus)status;
        }

        private static Api.OrderCmdResultCodes Convert(RejectReason reason, string message)
        {
            switch (reason)
            {
                case RejectReason.DealerReject: return Api.OrderCmdResultCodes.DealerReject;
                case RejectReason.UnknownSymbol: return Api.OrderCmdResultCodes.SymbolNotFound;
                case RejectReason.UnknownOrder: return Api.OrderCmdResultCodes.OrderNotFound;
                case RejectReason.IncorrectQuantity: return Api.OrderCmdResultCodes.IncorrectVolume;
                case RejectReason.OffQuotes: return Api.OrderCmdResultCodes.OffQuotes;
                case RejectReason.OrderExceedsLImit: return Api.OrderCmdResultCodes.NotEnoughMoney;
                case RejectReason.Other:
                    {
                        if (message == "Trade Not Allowed")
                            return Api.OrderCmdResultCodes.TradeNotAllowed;
                        else if (message != null && message.StartsWith("Not Enough Money"))
                            return Api.OrderCmdResultCodes.NotEnoughMoney;
                        else if (message != null && message.StartsWith("Rejected By Dealer"))
                            return Api.OrderCmdResultCodes.DealerReject;
                        else if (message != null && message.StartsWith("Dealer") && message.EndsWith("did not respond."))
                            return Api.OrderCmdResultCodes.DealingTimeout;
                        break;
                    }
                case RejectReason.None:
                    {
                        if (message != null && message.StartsWith("Order Not Found"))
                            return Api.OrderCmdResultCodes.OrderNotFound;
                        return OrderCmdResultCodes.Ok;
                    }
            }
            return Api.OrderCmdResultCodes.UnknownError;
        }

        private static PositionEntity Convert(SFX.Position p)
        {
            API.OrderSide side;
            double price;
            double amount;

            if (p.BuyAmount > 0)
            {
                side = API.OrderSide.Buy;
                price = p.BuyPrice ?? 0;
                amount = p.BuyAmount;
            }
            else
            {
                side = API.OrderSide.Sell;
                price = p.SellPrice ?? 0;
                amount = p.SellAmount;
            }

            return new PositionEntity()
            {
                Side = side,
                Volume = amount,
                Price = price,
                Symbol = p.Symbol,
                Commission = p.Commission,
                AgentCommission = p.AgentCommission,
            };
        }

        internal static BarEntity Convert(SFX.Bar fdkBar)
        {
            return new BarEntity()
            {
                Open = fdkBar.Open,
                Close = fdkBar.Close,
                High = fdkBar.High,
                Low = fdkBar.Low,
                Volume = fdkBar.Volume,
                OpenTime = fdkBar.From,
                CloseTime = fdkBar.To
            };
        }

        public static QuoteEntity[] ConvertAndFilter(IEnumerable<SFX.Quote> src, ref DateTime timeEdge)
        {
            var list = new List<QuoteEntity>();

            foreach (var item in src)
            {
                if (item.CreatingTime > timeEdge)
                {
                    list.Add(Convert(item));
                    timeEdge = item.CreatingTime;
                }
            }

            return list.ToArray();
        }

        internal static QuoteEntity[] Convert(SFX.Quote[] fdkQuoteSnapshot)
        {
            var result = new QuoteEntity[fdkQuoteSnapshot.Length];

            for (int i = 0; i < result.Length; i++)
                result[i] = Convert(fdkQuoteSnapshot[i]);
            return result;
        }

        private static QuoteEntity Convert(SFX.Quote fdkTick)
        {
            return new QuoteEntity(fdkTick.Symbol, fdkTick.CreatingTime, ConvertLevel2(fdkTick.Bids), ConvertLevel2(fdkTick.Asks));
        }

        public static TradeReportEntity Convert(TradeTransactionReport report)
        {
            bool isBalanceTransaction = report.TradeTransactionReportType == TradeTransactionReportType.Credit
                || report.TradeTransactionReportType == TradeTransactionReportType.BalanceTransaction;

            return new TradeReportEntity(report.Id + ":" + report.ActionId)
            {
                Id = report.Id,
                OrderId = report.Id,
                ReportTime = report.TransactionTime,
                OpenTime = report.OrderCreated,
                CloseTime = report.TransactionTime,
                Type = GetRecordType(report),
                //ActionType = Convert(report.TradeTransactionReportType),
                Balance = report.AccountBalance,
                Symbol = isBalanceTransaction ? report.TransactionCurrency : report.Symbol,
                TakeProfit = report.TakeProfit,
                StopLoss = report.StopLoss,
                OpenPrice = report.Price,
                Comment = report.Comment,
                Commission = report.Commission,
                CommissionCurrency = report.DstAssetCurrency ?? report.TransactionCurrency,
                OpenQuantity = report.Quantity,
                CloseQuantity = report.PositionLastQuantity,
                NetProfitLoss = report.TransactionAmount,
                GrossProfitLoss = report.TransactionAmount - report.Swap - report.Commission,
                ClosePrice = report.PositionClosePrice,
                Swap = report.Swap,
                RemainingQuantity = report.LeavesQuantity,
                AccountBalance = report.AccountBalance,
                ActionId = report.ActionId,
                AgentCommission = report.AgentCommission,
                ClientId = report.ClientId,
                CloseConversionRate = report.CloseConversionRate,
                CommCurrency = report.CommCurrency,
                DstAssetAmount = report.DstAssetAmount,
                DstAssetCurrency = report.DstAssetCurrency,
                DstAssetMovement = report.DstAssetMovement,
                DstAssetToUsdConversionRate = report.DstAssetToUsdConversionRate,
                Expiration = report.Expiration,
                ImmediateOrCancel = report.TimeInForce == OrderTimeInForce.ImmediateOrCancel,
                IsReducedCloseCommission = report.ReducedCloseCommission,
                IsReducedOpenCommission = report.ReducedOpenCommission,
                LeavesQuantity = report.LeavesQuantity,
                Magic = report.Magic,
                MarginCurrency = report.MarginCurrency,
                MarginCurrencyToUsdConversionRate = report.MarginCurrencyToUsdConversionRate,
                MarketWithSlippage = report.MarketWithSlippage,
                MaxVisibleQuantity = report.MaxVisibleQuantity,
                MinCommissionConversionRate = report.MinCommissionConversionRate,
                MinCommissionCurrency = report.MinCommissionCurrency,
                NextStreamPositionId = report.NextStreamPositionId,
                OpenConversionRate = report.OpenConversionRate,
                OrderCreated = report.OrderCreated,
                OrderFillPrice = report.OrderFillPrice,
                OrderLastFillAmount = report.OrderLastFillAmount,
                OrderModified = report.OrderModified,
                PositionById = report.PositionById,
                PositionClosed = report.PositionClosed,
                PositionClosePrice = report.PositionClosePrice,
                PositionCloseRequestedPrice = report.PositionCloseRequestedPrice,
                PositionId = report.PositionId,
                PositionLastQuantity = report.PositionLastQuantity,
                PositionLeavesQuantity = report.PositionLeavesQuantity,
                PositionModified = report.PositionModified,
                PositionOpened = report.PositionOpened,
                PositionQuantity = report.PositionQuantity,
                PosOpenPrice = report.PosOpenPrice,
                PosOpenReqPrice = report.PosOpenReqPrice,
                PosRemainingPrice = report.PosRemainingPrice,
                PosRemainingSide = Convert(report.PosRemainingSide),
                Price = report.Price,
                ProfitCurrency = report.ProfitCurrency,
                Quantity = report.Quantity,
                ProfitCurrencyToUsdConversionRate = report.ProfitCurrencyToUsdConversionRate,
                ReqClosePrice = report.ReqClosePrice,
                ReqCloseQuantity = report.ReqCloseQuantity,
                ReqOpenPrice = report.ReqOpenPrice,
                SrcAssetAmount = report.SrcAssetAmount,
                SrcAssetCurrency = report.SrcAssetCurrency,
                SrcAssetMovement = report.SrcAssetMovement,
                SrcAssetToUsdConversionRate = report.SrcAssetToUsdConversionRate,
                TradeRecordSide = Convert(report.OrderSide),
                TradeRecordType = Convert(report.OrderType),
                TradeTransactionReportType = Convert(report.TradeTransactionReportType),
                ReqOpenQuantity = report.ReqOpenQuantity,
                StopPrice = report.StopPrice,
                Tag = report.Tag,
                TransactionAmount = report.TransactionAmount,
                TransactionCurrency = report.TransactionCurrency,
                TransactionTime = report.TransactionTime,
                UsdToDstAssetConversionRate = report.UsdToDstAssetConversionRate,
                UsdToMarginCurrencyConversionRate = report.UsdToMarginCurrencyConversionRate,
                UsdToProfitCurrencyConversionRate = report.UsdToProfitCurrencyConversionRate,
                UsdToSrcAssetConversionRate = report.UsdToSrcAssetConversionRate,
            };
        }

        private static Api.TradeRecordTypes GetRecordType(TradeTransactionReport rep)
        {
            if (rep.TradeTransactionReportType == TradeTransactionReportType.BalanceTransaction)
            {
                if (rep.TransactionAmount >= 0)
                    return Api.TradeRecordTypes.Deposit;
                else
                    return Api.TradeRecordTypes.Withdrawal;
            }
            else if (rep.TradeTransactionReportType == TradeTransactionReportType.Credit)
            {
                return Api.TradeRecordTypes.Unknown;
            }
            else if (rep.OrderType == SFX.OrderType.Limit)
            {
                if (rep.OrderSide == SFX.OrderSide.Buy)
                    return Api.TradeRecordTypes.BuyLimit;
                else if (rep.OrderSide == SFX.OrderSide.Sell)
                    return Api.TradeRecordTypes.SellLimit;
            }
            else if (rep.OrderType == SFX.OrderType.Position || rep.OrderType == SFX.OrderType.Market)
            {
                if (rep.OrderSide == SFX.OrderSide.Buy)
                    return Api.TradeRecordTypes.Buy;
                else if (rep.OrderSide == SFX.OrderSide.Sell)
                    return Api.TradeRecordTypes.Sell;
            }
            else if (rep.OrderType == SFX.OrderType.Stop)
            {
                if (rep.OrderSide == SFX.OrderSide.Buy)
                    return Api.TradeRecordTypes.BuyStop;
                else if (rep.OrderSide == SFX.OrderSide.Sell)
                    return Api.TradeRecordTypes.SellStop;
            }

            return Api.TradeRecordTypes.Unknown;
        }

        private static Api.TradeExecActions Convert(TradeTransactionReportType type)
        {
            switch (type)
            {
                case TradeTransactionReportType.BalanceTransaction: return Api.TradeExecActions.BalanceTransaction;
                case TradeTransactionReportType.Credit: return Api.TradeExecActions.Credit;
                case TradeTransactionReportType.OrderActivated: return Api.TradeExecActions.OrderActivated;
                case TradeTransactionReportType.OrderCanceled: return Api.TradeExecActions.OrderCanceled;
                case TradeTransactionReportType.OrderExpired: return Api.TradeExecActions.OrderExpired;
                case TradeTransactionReportType.OrderFilled: return Api.TradeExecActions.OrderFilled;
                case TradeTransactionReportType.OrderOpened: return Api.TradeExecActions.OrderOpened;
                case TradeTransactionReportType.PositionClosed: return Api.TradeExecActions.PositionClosed;
                case TradeTransactionReportType.PositionOpened: return Api.TradeExecActions.PositionOpened;
                default: return Api.TradeExecActions.None;
            }
        }

        private static Api.BookEntry[] ConvertLevel2(List<QuoteEntry> book)
        {
            if (book == null || book.Count == 0)
                return QuoteEntity.EmptyBook;
            else
                return book.Select(b => Convert(b)).ToArray();
        }

        public static BookEntryEntity Convert(QuoteEntry fdkEntry)
        {
            return new BookEntryEntity()
            {
                Price = fdkEntry.Price,
                Volume = fdkEntry.Volume
            };
        }

        public static BalanceOperationReport Convert(SFX.BalanceOperation op)
        {
            return new BalanceOperationReport(op.Balance, op.TransactionCurrency, op.TransactionAmount);
        }

        public static PriceType ConvertBack(BarPriceType priceType)
        {
            switch (priceType)
            {
                case Api.BarPriceType.Ask: return PriceType.Ask;
                case Api.BarPriceType.Bid: return PriceType.Bid;
            }
            throw new NotImplementedException("Unsupported price type: " + priceType);
        }

        private ConnectionErrorCodes Convert(LogoutReason fdkCode)
        {
            switch (fdkCode)
            {
                case LogoutReason.BlockedAccount: return ConnectionErrorCodes.BlockedAccount;
                case LogoutReason.InvalidCredentials: return ConnectionErrorCodes.InvalidCredentials;
                case LogoutReason.NetworkError: return ConnectionErrorCodes.NetworkError;
                case LogoutReason.ServerError: return ConnectionErrorCodes.ServerError;
                case LogoutReason.ServerLogout: return ConnectionErrorCodes.ServerLogout;
                case LogoutReason.SlowConnection: return ConnectionErrorCodes.SlowConnection;
                case LogoutReason.LoginDeleted: return ConnectionErrorCodes.LoginDeleted;
                default: return ConnectionErrorCodes.Unknown;
            }
        }

        #endregion
    }    
}
