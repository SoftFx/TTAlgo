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

namespace TickTrader.Algo.Common.Model
{
    internal class SfxInterop : CrossDomainObject, IServerInterop, IFeedServerApi, ITradeServerApi
    {
        private const int ConnectTimeoutMs = 60 * 1000;
        private const int LogoutTimeoutMs = 60 * 1000;
        private const int DisconnectTimeoutMs = 60 * 1000;

        public IFeedServerApi FeedApi => this;
        public ITradeServerApi TradeApi => this;

        private FDK.QuoteFeed.Client _feedProxy;
        private FDK.QuoteStore.Client _feedHistoryProxy;
        private FDK.OrderEntry.Client _tradeProxy;

        public event Action<IServerInterop, ConnectionErrorCodes> Disconnected;
        
        public SfxInterop()
        {
            _feedProxy = new FDK.QuoteFeed.Client("feed.proxy");
            _feedHistoryProxy = new FDK.QuoteStore.Client("feed.history.proxy");
            _tradeProxy = new FDK.OrderEntry.Client("trade.proxy");

            _feedProxy.QuoteUpdateEvent += (c, q) => Tick?.Invoke(Convert(q));
        }

        public async Task<ConnectionErrorCodes> Connect(string address, string login, string password, CancellationToken cancelToken)
        {
            await Task.WhenAll(
                ConnectFeed(address, login, password),
                ConnectTrade(address, login, password),
                ConnectFeedHistory(address, login, password));

            return ConnectionErrorCodes.None;
        }

        private async Task ConnectFeed(string address, string login, string password)
        {
            await _feedProxy.ConnectAsync(address);
            await _feedProxy.LoginAsync(login, password, "", "", Guid.NewGuid().ToString());
        }

        private async Task ConnectTrade(string address, string login, string password)
        {
            await _tradeProxy.ConnectAsync(address);
            await _tradeProxy.LoginAsync(login, password, "", "", Guid.NewGuid().ToString());
        }

        private async Task ConnectFeedHistory(string address, string login, string password)
        {
            await _feedHistoryProxy.ConnectAsync(address);
            await _feedHistoryProxy.LoginAsync(login, password, "", "", Guid.NewGuid().ToString());
        }

        public Task Disconnect()
        {
            return Task.WhenAll(
                DisconnectFeed(),
                DisconnectTrade(),
                DisconnectFeedHstory());
        }

        private async Task DisconnectFeed()
        {
            await _feedProxy.LogoutAsync("").AddTimeout(LogoutTimeoutMs);
            await _feedProxy.DisconnectAsync("");
        }

        private async Task DisconnectFeedHstory()
        {
            await _feedHistoryProxy.LogoutAsync("").AddTimeout(LogoutTimeoutMs);
            await _feedHistoryProxy.DisconnectAsync("");
        }

        private async Task DisconnectTrade()
        {
            await _feedProxy.LogoutAsync("").AddTimeout(LogoutTimeoutMs);
            await _feedProxy.DisconnectAsync("");
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

        public Task SubscribeToQuotes(string[] symbols, int depth)
        {
            return _feedProxy.SubscribeQuotesAsync(symbols, depth);
        }

        public IAsyncEnumerator<BarEntity[]> GetHistoryBars(string symbol, DateTime from, DateTime to, BarPriceType priceType, TimeFrames barPeriod)
        {
            throw new NotImplementedException();
        }

        public async Task<List<BarEntity>> GetHistoryBars(string symbol, DateTime startTime, int count, BarPriceType priceType, TimeFrames timeFrame)
        {
            var result = new List<BarEntity>();

            using (var enumerator = await _feedHistoryProxy.DownloadBarsAsync(
                Guid.NewGuid().ToString(), symbol, ConvertBack(priceType), ConvertBack(timeFrame), DateTime.MinValue, startTime))
            {
                for (int i = 0; i < count; i++)
                {
                    var bar = await enumerator.NextAsync();
                    if (bar == null)
                        break;

                    result.Add(Convert(bar));
                }
            }

            return result;
        }

        #endregion

        #region ITradeServerApi

        public event Action<PositionEntity> PositionReport;
        public event Action<ExecutionReport> ExecutionReport;
        public event Action<TradeReportEntity> TradeTransactionReport;
        public event Action<BalanceOperationReport> BalanceOperation;

        public Task<AccountEntity> GetAccountInfo()
        {
            return _tradeProxy.GetAccountInfoAsync()
                .ContinueWith(t => Convert(t.Result));
        }

        public Task<OrderEntity[]> GetTradeRecords()
        {
            return _tradeProxy.GetOrdersAsync()
                .ContinueWith(t => t.Result.Select(Convert).ToArray());
        }

        public Task<PositionEntity[]> GetPositions()
        {
            return _tradeProxy.GetPositionsAsync()
                .ContinueWith(t => t.Result.Select(Convert).ToArray());
        }

        public Task<HistoryFilesPackage> DownloadTickFiles(string symbol, DateTime refTimePoint, bool includeLevel2)
        {
            throw new NotImplementedException();
        }

        public Task<OrderCmdResult> OpenOrder(OpenOrderRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<OrderCmdResult> CancelOrder(CancelOrderRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<OrderCmdResult> ModifyOrder(ReplaceOrderRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<OrderCmdResult> CloseOrder(CloseOrderRequest request)
        {
            throw new NotImplementedException();
        }

        public IAsyncEnumerator<TradeReportEntity[]> GetTradeHistory(DateTime? from, DateTime? to, bool skipCancelOrders)
        {
            throw new NotImplementedException();
        }

        public void SendOpenOrder(CrossDomainCallback<OrderCmdResultCodes> callback, OpenOrderRequest request)
        {
            throw new NotImplementedException();
        }

        public void SendCancelOrder(CrossDomainCallback<OrderCmdResultCodes> callback, CancelOrderRequest request)
        {
            throw new NotImplementedException();
        }

        public void SendModifyOrder(CrossDomainCallback<OrderCmdResultCodes> callback, ReplaceOrderRequest request)
        {
            throw new NotImplementedException();
        }

        public void SendCloseOrder(CrossDomainCallback<OrderCmdResultCodes> callback, CloseOrderRequest request)
        {
            throw new NotImplementedException();
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
                SwapEnabled = true, // ???
                SwapSizeLong = (float)(info.SwapSizeLong ?? 0),
                SwapSizeShort = (float)(info.SwapSizeShort ?? 0),
                Security = info.SecurityName,
                SortOrder = 0 // ??
            };
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

        private static Api.OrderSide Convert(SFX.OrderSide fdkSide)
        {
            switch (fdkSide)
            {
                case SFX.OrderSide.Buy: return Api.OrderSide.Buy;
                case SFX.OrderSide.Sell: return Api.OrderSide.Sell;

                default: throw new ArgumentException("Unsupported order side: " + fdkSide);
            }
        }

        private static string ConvertBack(Api.TimeFrames timeframe)
        {
            return ToBarPeriod(timeframe).ToString();
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

        private static OrderEntity Convert(SFX.ExecutionReport record)
        {
            return new OrderEntity(record.OrderId)
            {
                Symbol = record.Symbol,
                Comment = record.Comment,
                Type = Convert(record.OrderType),
                ClientOrderId = record.ClientOrderId,
                Price = record.Price ?? double.NaN,
                StopPrice = record.StopPrice ?? double.NaN,
                Side = Convert(record.OrderSide),
                Created = record.Created ?? DateTime.MinValue,
                Swap = record.Swap,
                Modified = record.Modified ?? DateTime.MinValue,
                StopLoss = record.StopLoss ?? double.NaN,
                TakeProfit = record.TakeProfit ?? double.NaN,
                Commission = record.Commission,
            };
        }

        private static PositionEntity Convert(SFX.Position p)
        {
            return new PositionEntity()
            {
                Symbol = p.Symbol,
                Margin = p.Margin ?? 0,
                Commission = p.Commission,
                AgentCommission = p.AgentCommission,
            };
        }

        private static BarEntity Convert(SFX.Bar fdkBar)
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

        private static QuoteEntity Convert(SFX.Quote fdkTick)
        {
            return new QuoteEntity(fdkTick.Symbol, fdkTick.CreatingTime, ConvertLevel2(fdkTick.Bids), ConvertLevel2(fdkTick.Asks));
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

        public static PriceType ConvertBack(BarPriceType priceType)
        {
            switch (priceType)
            {
                case Api.BarPriceType.Ask: return PriceType.Ask;
                case Api.BarPriceType.Bid: return PriceType.Bid;
            }
            throw new NotImplementedException("Unsupported price type: " + priceType);
        }

        #endregion
    }
}
