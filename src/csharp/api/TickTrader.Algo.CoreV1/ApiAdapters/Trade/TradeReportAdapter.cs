using Google.Protobuf.WellKnownTypes;
using System;
using TickTrader.Algo.Api;
using TickTrader.Algo.Api.Math;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.CoreV1
{
    internal sealed class TradeReportAdapter : TradeReport
    {
        private readonly double _lotSize;
        private readonly double _volumeStep;


        public TradeReportInfo Info { get; }

        public TradeReportAdapter(TradeReportInfo entity, ISymbolInfo symbol)
        {
            _lotSize = symbol?.LotSize ?? 1;
            _volumeStep = symbol?.VolumeStep ?? 1;

            Info = entity;
        }

        public string ReportId => Info.Id;

        public string OrderId => Info.OrderId;

        public string PositionId => Info.PositionId;

        public string PositionById => Info.PositionById;

        public DateTime ReportTime => Info.ReportTime.ToDateTime();

        public DateTime OpenTime => Info.OpenTime.ToDateTime();

        public TradeRecordTypes Type => GetRecordType(Info);

        public TradeExecActions ActionType => Info.ReportType.ToApiEnum();

        public string Symbol => Info.Symbol;

        public double OpenQuantity => Info.OpenQuantity / _lotSize;

        public double OpenPrice => Info.OpenPrice;

        public double StopLoss => Info.StopLoss;

        public double TakeProfit => Info.TakeProfit;

        public DateTime CloseTime => Info.CloseTime.ToDateTime();

        public double CloseQuantity => Info.PositionCloseQuantity / _lotSize;

        public double ClosePrice => Info.PositionClosePrice;

        public double RemainingQuantity => Info.RemainingQuantity / _lotSize;

        public double Commission => Info.Commission;

        public string CommissionCurrency => Info.CommissionCurrency;

        public double Swap => Info.Swap;

        public double Balance => Info.Balance;

        public string Comment => Info.Comment;

        public double GrossProfitLoss => Info.GrossProfitLoss;

        public double NetProfitLoss => Info.NetProfitLoss;

        public OrderInfo.Types.Side TradeRecordSide => Info.OrderSide;

        OrderSide TradeReport.TradeRecordSide => Info.OrderSide.ToApiEnum();

        public OrderInfo.Types.Type TradeRecordType => Info.OrderType;

        OrderType TradeReport.TradeRecordType => Info.OrderType.ToApiEnum();

        public double? MaxVisibleQuantity => Info.MaxVisibleQuantity;

        public string Tag => Info.Tag;

        public double? Slippage => Info.Slippage;

        public double? ReqCloseQuantity => Info.RequestedCloseQuantity;

        public double? ReqClosePrice => Info.RequestedClosePrice;

        public double? ReqOpenQuantity => Info.RequestedOpenQuantity;

        public double? ReqOpenPrice => Info.RequestedOpenPrice;

        public bool ImmediateOrCancel => Info.OrderOptions.HasFlag(Domain.OrderOptions.ImmediateOrCancel);

        public double? LastFillQuantity => Info.OrderLastFillAmount / _lotSize;

        public string InstanceId => Info.InstanceId;

        #region Emulation

        public static TradeReportAdapter Create(Timestamp key, ISymbolInfo symbol, TradeReportInfo.Types.ReportType repType, TradeReportInfo.Types.Reason reason)
        {
            var ms = Math.DivRem(key.Nanos, 1_000_000, out var rem);

            var entity = new TradeReportInfo
            {
                TransactionTime = key,
                Symbol = symbol.Name,
                ReportType = repType,
                TransactionReason = reason,
                Id = $"{key.Seconds * 1_000 + ms}.{rem}",
                IsEmulated = true
            };

            return new TradeReportAdapter(entity, symbol);
        }

        public TradeReportAdapter FillGenericOrderData(CalculatorFixture acc, OrderAccessor orderAccessor)
        {
            var orderInfo = orderAccessor.Info;
            var order = orderAccessor.Entity;

            Info.OrderOpened = orderInfo.Created;
            Info.OrderModified = orderInfo.Modified;
            Info.OrderId = orderInfo.Id;
            Info.ActionId = order.ActionNo;
            //Entity.ParentOrderId = order.ParentOrderId;
            //ClientOrderId = order.ClientOrderId;
            Info.OrderType = orderInfo.Type;
            Info.RequestedOrderType = orderInfo.InitialType;
            Info.OpenQuantity = orderInfo.RequestedAmount;
            Info.RemainingQuantity = orderInfo.RemainingAmount;
            Info.MaxVisibleQuantity = orderInfo.MaxVisibleAmount;
            //Entity.OrderHiddenAmount = order.HiddenAmount;
            //Entity.OrderMaxVisibleAmount = order.MaxVisibleAmount;
            Info.Price = orderInfo.Price ?? double.NaN;
            Info.StopPrice = orderInfo.StopPrice ?? double.NaN;
            Info.OrderSide = orderInfo.Side;
            //Entity.SymbolRef = order.SymbolRef;
            //Entity.SymbolPrecision = order.SymbolPrecision;
            Info.Expiration = orderInfo.Expiration?.ToTimestamp();
            //Entity.Magic = order.Magic;
            Info.StopLoss = orderInfo.StopLoss ?? double.NaN;
            Info.TakeProfit = orderInfo.TakeProfit ?? double.NaN;
            //TransferringCoefficient = order.TransferringCoefficient;

            if (orderInfo.Type == Domain.OrderInfo.Types.Type.Position)
            {
                Info.PositionId = orderInfo.Id;
                //Entity.OrderId = order.ParentOrderId ?? -1;
            }

            //ReducedOpenCommissionFlag = order.IsReducedOpenCommission;
            //ReducedCloseCommissionFlag = order.IsReducedCloseCommission;

            if (orderInfo.IsSupportedSlippage)
                Info.Slippage = orderInfo.Slippage;

            // comments and tags
            Info.Comment = orderInfo.Comment;
            //ManagerComment = order.ManagerComment;
            Info.Tag = orderInfo.UserTag;
            //ManagerTag = order.ManagerTag;

            //rates
            //MarginRateInitial = order.MarginRateInitial;
            //Entity.OpenConversionRate = (double?)order.OpenConversionRate;
            //Entity.CloseConversionRate =  (double?)order.CloseConversionRate;

            //Entity.ReqOpenPrice = order.ReqOpenPrice;
            //Entity.ReqOpenQuantity = order.ReqOpenAmount;

            Info.OrderOptions = orderInfo.Options;
            //ClientApp = order.ClientApp;

            FillSymbolConversionRates(acc, orderAccessor.SymbolInfo);

            return this;
        }

        public TradeReportAdapter FillClosePosData(OrderAccessor order, DateTime closeTime, double closeAmount, double closePrice, double? requestAmount, double? requestPrice, string posById)
        {
            var orderInfo = order.Info;

            Info.PositionQuantity = orderInfo.RequestedAmount;
            Info.PositionLeavesQuantity = orderInfo.RemainingAmount;
            Info.PositionCloseQuantity = closeAmount;
            Info.PositionOpened = order.Entity.PositionCreated.ToUniversalTime().ToTimestamp();
            Info.PositionOpenPrice = orderInfo.Price ?? 0;
            Info.PositionClosed = closeTime.ToUniversalTime().ToTimestamp();
            Info.PositionClosePrice = closePrice;
            Info.PositionModified = orderInfo.Modified;
            Info.PositionById = posById;
            Info.RequestedClosePrice = requestPrice;
            Info.RequestedCloseQuantity = requestAmount;
            return this;
        }

        public TradeReportAdapter FillAccountSpecificFields(CalculatorFixture acc)
        {
            if (acc.Acc.IsMarginType)
            {
                Info.ProfitCurrency = acc.Acc.BalanceCurrency;
                Info.AccountBalance = Balance;
            }

            return this;
        }

        public TradeReportAdapter FillBalanceMovement(double balance, double movement)
        {
            Info.AccountBalance = balance;
            Info.TransactionAmount = movement;
            return this;
        }

        public TradeReportAdapter FillCharges(double commission, double swap, double profit, double balanceMovement)
        {
            Info.Commission += commission;
            //Entity.AgentCommission += (double)charges.AgentCommission;
            Info.Swap += swap;
            Info.TransactionAmount = balanceMovement;
            //Entity.BalanceMovement = balanceMovement;
            //Entity.MinCommissionCurrency = charges.MinCommissionCurrency;
            //Entity.MinCommissionConversionRate = charges.MinCommissionConversionRate;
            return this;
        }

        public TradeReportAdapter FillPosData(PositionAccessor position, double openPrice, double? openConversionRate)
        {
            //Entity.PositionId = position.Id;
            if (!position.Info.IsEmpty)
            {
                //Entity.PositionQuantity = position.VolumeUnits;
                Info.PositionLeavesQuantity = position.Info.Volume;
                Info.PositionRemainingPrice = position.Info.Price;
                Info.PositionRemainingSide = position.Info.Side;
                Info.PositionModified = position.Info.Modified;
            }
            else
            {
                Info.PositionQuantity = 0;
                Info.PositionLeavesQuantity = 0;
            }

            Info.PositionOpenPrice = openPrice;
            //Entity.OpenConversionRate = openConversionRate;

            return this;
        }

        public TradeReportAdapter FillProfitConversionRates(string balanceCurrency, double? profit, CalculatorFixture acc)
        {
            //try
            //{
            //    if (profit.HasValue && profit < 0)
            //        ProfitToUsdConversionRate = acc.Market.ConversionMap.GetNegativeAssetConversion(balanceCurrency, "USD").Value;
            //    else
            //        ProfitToUsdConversionRate = acc.Market.ConversionMap.GetPositiveAssetConversion(balanceCurrency, "USD").Value;
            //}
            //catch (Exception) { }
            //try
            //{
            //    if (profit.HasValue && profit < 0)
            //        UsdToProfitConversionRate = acc.Market.ConversionMap.GetNegativeAssetConversion("USD", balanceCurrency).Value;
            //    else
            //        UsdToProfitConversionRate = acc.Market.ConversionMap.GetPositiveAssetConversion("USD", balanceCurrency).Value;
            //}
            //catch (Exception) { }

            return this;
        }

        public TradeReportAdapter FillAccountBalanceConversionRates(CalculatorFixture acc, string balanceCurrency, double? balance)
        {
            //try
            //{
            //    if (balance.HasValue && balance < 0)
            //        BalanceToUsdConversionRate = acc.Market.ConversionMap.GetNegativeAssetConversion(balanceCurrency, "USD").Value;
            //    else
            //        BalanceToUsdConversionRate = acc.Market.ConversionMap.GetPositiveAssetConversion(balanceCurrency, "USD").Value;
            //}
            //catch (Exception) { }
            //try
            //{
            //    if (balance.HasValue && balance < 0)
            //        UsdToBalanceConversionRate = acc.Market.ConversionMap.GetNegativeAssetConversion("USD", balanceCurrency).Value;
            //    else
            //        UsdToBalanceConversionRate = acc.Market.ConversionMap.GetPositiveAssetConversion("USD", balanceCurrency).Value;
            //}
            //catch (Exception) { }

            return this;
        }

        public TradeReportAdapter FillAccountAssetsMovement(CalculatorFixture acc, string srcAssetCurrency, double srcAssetAmount, double srcAssetMovement, string dstAssetCurrency, double dstAssetAmount, double dstAssetMovement)
        {
            //try
            //{
            //    Entity.SrcAssetToUsdConversionRate = (double)acc.Market.Conversion.GetPositiveAssetConversion(srcAssetCurrency, "USD").Value;
            //}
            //catch (Exception) { }
            //try
            //{
            //    Entity.UsdToSrcAssetConversionRate = (double)acc.Market.Conversion.GetPositiveAssetConversion("USD", srcAssetCurrency).Value;
            //}
            //catch (Exception) { }
            //try
            //{
            //    Entity.DstAssetToUsdConversionRate = (double)acc.Market.Conversion.GetPositiveAssetConversion(dstAssetCurrency, "USD").Value;
            //}
            //catch (Exception) { }
            //try
            //{
            //    Entity.UsdToDstAssetConversionRate = (double)acc.Market.Conversion.GetPositiveAssetConversion("USD", dstAssetCurrency).Value;
            //}
            //catch (Exception) { }

            Info.SrcAssetCurrency = srcAssetCurrency;
            Info.SrcAssetAmount = srcAssetAmount;
            Info.SrcAssetMovement = srcAssetMovement;
            Info.DstAssetCurrency = dstAssetCurrency;
            Info.DstAssetAmount = dstAssetAmount;
            Info.DstAssetMovement = dstAssetMovement;

            return this;
        }

        public TradeReportAdapter FillSymbolConversionRates(CalculatorFixture acc, SymbolInfo symbol)
        {
            if (symbol == null)
                return this;

            if (symbol.BaseCurrency != null)
            {
                if (acc.Acc.Type != Domain.AccountInfo.Types.Type.Cash)
                {
                    //try
                    //{
                    //    Entity.MarginCurrencyToUsdConversionRate = (double)acc.Market.Conversion.GetPositiveAssetConversion(symbol.MarginCurrency, "USD").Value;
                    //}
                    //catch (Exception) { }
                    //try
                    //{
                    //    Entity.UsdToMarginCurrencyConversionRate = (double)acc.Market.Conversion.GetPositiveAssetConversion("USD", symbol.MarginCurrency).Value;
                    //}
                    //catch (Exception) { }
                }
                else
                {
                    //Entity.MarginCurrencyToUsdConversionRate = null;
                    //Entity.UsdToMarginCurrencyConversionRate = null;
                }

                Info.MarginCurrency = symbol.BaseCurrency;
            }

            if (symbol.CounterCurrency != null)
            {
                if (acc.Acc.Type != Domain.AccountInfo.Types.Type.Cash)
                {
                    //try
                    //{
                    //    Entity.ProfitCurrencyToUsdConversionRate = (double)acc.Market.Conversion.GetPositiveAssetConversion(symbol.ProfitCurrency, "USD").Value;
                    //}
                    //catch (Exception) { }
                    //try
                    //{
                    //    Entity.UsdToProfitCurrencyConversionRate = (double)acc.Market.Conversion.GetPositiveAssetConversion("USD", symbol.ProfitCurrency).Value;
                    //}
                    //catch (Exception) { }
                }
                else
                {
                    //Entity.ProfitCurrencyToUsdConversionRate = null;
                    //Entity.UsdToProfitCurrencyConversionRate = null;
                }

                Info.ProfitCurrency = symbol.CounterCurrency;
            }

            return this;
        }

        #endregion

        private static TradeRecordTypes GetRecordType(TradeReportInfo rep)
        {
            if (rep.ReportType == TradeReportInfo.Types.ReportType.BalanceTransaction)
            {
                if (rep.TransactionAmount >= 0)
                    return TradeRecordTypes.Deposit;
                else
                    return TradeRecordTypes.Withdrawal;
            }
            else if (rep.ReportType == TradeReportInfo.Types.ReportType.Credit)
            {
                return TradeRecordTypes.Unknown;
            }
            else if (rep.OrderType == OrderInfo.Types.Type.Limit)
            {
                if (rep.OrderSide == OrderInfo.Types.Side.Buy)
                    return TradeRecordTypes.BuyLimit;
                else if (rep.OrderSide == OrderInfo.Types.Side.Sell)
                    return TradeRecordTypes.SellLimit;
            }
            else if (rep.OrderType == OrderInfo.Types.Type.Position || rep.OrderType == Domain.OrderInfo.Types.Type.Market)
            {
                if (rep.OrderSide == OrderInfo.Types.Side.Buy)
                    return TradeRecordTypes.Buy;
                else if (rep.OrderSide == OrderInfo.Types.Side.Sell)
                    return TradeRecordTypes.Sell;
            }
            else if (rep.OrderType == OrderInfo.Types.Type.Stop)
            {
                if (rep.OrderSide == OrderInfo.Types.Side.Buy)
                    return TradeRecordTypes.BuyStop;
                else if (rep.OrderSide == OrderInfo.Types.Side.Sell)
                    return TradeRecordTypes.SellStop;
            }

            return TradeRecordTypes.Unknown;
        }

        public TradeReportAdapter RoundVolumes()
        {
            Info.OpenQuantity = Info.OpenQuantity.Floor(_volumeStep);
            Info.MaxVisibleQuantity = Info.MaxVisibleQuantity.Floor(_volumeStep);
            Info.RemainingQuantity = Info.RemainingQuantity.Floor(_volumeStep);
            Info.OrderLastFillAmount = Info.OrderLastFillAmount.Floor(_volumeStep);
            Info.PositionQuantity = Info.PositionQuantity.Floor(_volumeStep);
            Info.PositionCloseQuantity = Info.PositionCloseQuantity.Floor(_volumeStep);
            Info.PositionLeavesQuantity = Info.PositionLeavesQuantity.Floor(_volumeStep);
            Info.RequestedOpenQuantity = Info.RequestedOpenQuantity.Floor(_volumeStep);
            Info.RequestedCloseQuantity = Info.RequestedCloseQuantity.Floor(_volumeStep);
            Info.TransactionAmount = Info.TransactionAmount.Floor(_volumeStep);

            return this;
        }
    }
}
