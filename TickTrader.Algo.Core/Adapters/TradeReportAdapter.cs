using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;
using Bo = TickTrader.BusinessObjects;

namespace TickTrader.Algo.Core
{
    internal class TradeReportAdapter : Api.TradeReport
    {
        private double _lotSize;

        public TradeReportAdapter(TradeReportEntity entity, SymbolAccessor smbInfo)
        {
            Entity = entity;
            _lotSize = smbInfo?.ContractSize ?? 1;
        }

        public TradeReportEntity Entity { get; }

        public string ReportId => Entity.Id;
        public string OrderId => Entity.OrderId;
        public string PositionId => Entity.PositionId;
        public string PositionById => Entity.PositionById;
        public DateTime ReportTime => Entity.ReportTime;
        public DateTime OpenTime => Entity.OpenTime;
        public TradeRecordTypes Type => Entity.Type;
        public TradeExecActions ActionType => Entity.ActionType;
        public string Symbol => Entity.Symbol;
        public double OpenQuantity => Entity.OpenQuantity / _lotSize;
        public double OpenPrice => Entity.OpenPrice;
        public double StopLoss => Entity.StopLoss;
        public double TakeProfit => Entity.TakeProfit;
        public DateTime CloseTime => Entity.CloseTime;
        public double CloseQuantity => Entity.CloseQuantity / _lotSize;
        public double ClosePrice => Entity.ClosePrice;
        public double RemainingQuantity => Entity.RemainingQuantity / _lotSize;
        public double Commission => Entity.Commission;
        public string CommissionCurrency => Entity.CommissionCurrency;
        public double Swap => Entity.Swap;
        public double Balance => Entity.Balance;
        public string Comment => Entity.Comment;
        public double GrossProfitLoss => Entity.GrossProfitLoss;
        public double NetProfitLoss => Entity.NetProfitLoss;
        public Domain.OrderInfo.Types.Side TradeRecordSide => Entity.TradeRecordSide;
        OrderSide TradeReport.TradeRecordSide => Entity.TradeRecordSide.ToApiEnum();
        public Domain.OrderInfo.Types.Type TradeRecordType => Entity.TradeRecordType;
        OrderType TradeReport.TradeRecordType => Entity.TradeRecordType.ToApiEnum();
        public double? MaxVisibleQuantity => Entity.MaxVisibleQuantity;
        public string Tag => Entity.Tag;
        public double? Slippage => Entity.Slippage;
        public double? ReqCloseQuantity => Entity.ReqCloseQuantity;
        public double? ReqClosePrice => Entity.ReqClosePrice;
        public double? ReqOpenQuantity => Entity.ReqOpenQuantity;
        public double? ReqOpenPrice => Entity.ReqOpenPrice;
        public bool ImmediateOrCancel => Entity.ImmediateOrCancel;

        #region Emulation

        public static TradeReportAdapter Create(TimeKey key, SymbolAccessor symbol, TradeExecActions repType, TradeTransactionReason reason)
        {
            var entity = new TradeReportEntity();
            entity.TransactionTime = key.Timestamp;
            entity.Symbol = symbol.Name;
            entity.ActionType = repType;
            entity.Id = key.ToString();
            entity.IsEmulatedEntity = true;
            return new TradeReportAdapter(entity, symbol);
        }

        public TradeReportAdapter FillGenericOrderData(CalculatorFixture acc, OrderAccessor order)
        {
            Entity.OrderCreated = order.Created;
            Entity.OrderModified = order.Modified;
            Entity.OrderId = order.Id;
            Entity.ActionId = order.ActionNo;
            //Entity.ParentOrderId = order.ParentOrderId;
            //ClientOrderId = order.ClientOrderId;
            Entity.TradeRecordType = order.Type;
            Entity.ReqOrderType = order.InitialType;
            Entity.OpenQuantity = (double)order.Amount;
            Entity.RemainingQuantity = (double)order.RemainingAmount;
            //Entity.OrderHiddenAmount = order.HiddenAmount;
            //Entity.OrderMaxVisibleAmount = order.MaxVisibleAmount;
            Entity.Price = order.Price;
            Entity.StopPrice = order.StopPrice;
            Entity.TradeRecordSide = order.Side;
            //Entity.SymbolRef = order.SymbolRef;
            //Entity.SymbolPrecision = order.SymbolPrecision;
            Entity.Expiration = order.Expiration;
            //Entity.Magic = order.Magic;
            Entity.StopLoss = order.StopLoss;
            Entity.TakeProfit = order.TakeProfit;
            //TransferringCoefficient = order.TransferringCoefficient;

            if (order.Type == Domain.OrderInfo.Types.Type.Position)
            {
                Entity.PositionId = order.Id;
                //Entity.OrderId = order.ParentOrderId ?? -1;
            }

            //ReducedOpenCommissionFlag = order.IsReducedOpenCommission;
            //ReducedCloseCommissionFlag = order.IsReducedCloseCommission;

            // comments and tags
            Entity.Comment = order.Comment;
            //ManagerComment = order.ManagerComment;
            Entity.Tag = order.Tag;
            //ManagerTag = order.ManagerTag;

            //rates
            //MarginRateInitial = order.MarginRateInitial;
            Entity.OpenConversionRate = (double?)order.OpenConversionRate;
            //Entity.CloseConversionRate =  (double?)order.CloseConversionRate;

            //Entity.ReqOpenPrice = order.ReqOpenPrice;
            //Entity.ReqOpenQuantity = order.ReqOpenAmount;

            Entity.ImmediateOrCancel = order.HasOption(OrderExecOptions.ImmediateOrCancel);
            //ClientApp = order.ClientApp;

            FillSymbolConversionRates(acc, order.SymbolInfo);

            return this;
        }

        public TradeReportAdapter FillClosePosData(OrderAccessor order, DateTime closeTime, decimal closeAmount, double closePrice, decimal? requestAmount, double? requestPrice, string posById)
        {
            Entity.PositionQuantity = (double)order.Entity.RequestedVolume;
            Entity.PositionLeavesQuantity = (double)order.Entity.RemainingVolume;
            Entity.CloseQuantity = (double)closeAmount;
            Entity.PositionOpened = order.PositionCreated;
            Entity.PosOpenPrice = order.Price;
            Entity.PositionClosed = closeTime;
            Entity.ClosePrice = (double)closePrice;
            Entity.PositionModified = order.Modified;
            Entity.PositionById = posById;
            Entity.ReqClosePrice = (double?)requestPrice;
            Entity.ReqCloseQuantity = (double?)requestAmount;
            return this;
        }

        public TradeReportAdapter FillAccountSpecificFields(CalculatorFixture acc)
        {
            if (acc.Acc.IsMarginType)
            {
                Entity.ProfitCurrency = acc.Acc.BalanceCurrency;
                Entity.AccountBalance = Balance;
            }

            return this;
        }

        public TradeReportAdapter FillBalanceMovement(decimal balance, decimal movement)
        {
            Entity.AccountBalance = (double)balance;
            Entity.TransactionAmount = (double)movement;
            return this;
        }

        public TradeReportAdapter FillCharges(TradeChargesInfo charges, decimal profit, decimal balanceMovement)
        {
            Entity.Commission += (double)charges.Commission;
            //Entity.AgentCommission += (double)charges.AgentCommission;
            Entity.Swap += (double)charges.Swap;
            Entity.TransactionAmount = (double)balanceMovement;
            //Entity.BalanceMovement = balanceMovement;
            //Entity.MinCommissionCurrency = charges.MinCommissionCurrency;
            //Entity.MinCommissionConversionRate = charges.MinCommissionConversionRate;
            return this;
        }

        public TradeReportAdapter FillPosData(PositionAccessor position, double openPrice, double? openConversionRate)
        {
            //Entity.PositionId = position.Id;
            if (!position.IsEmpty)
            {
                //Entity.PositionQuantity = position.VolumeUnits;
                Entity.PositionLeavesQuantity = (double)position.VolumeUnits;
                Entity.PosRemainingPrice = position.Price;
                Entity.PosRemainingSide = position.Side;
                Entity.PositionModified = position.Modified ?? DateTime.MinValue;
            }
            else
            {
                Entity.PositionQuantity = 0;
                Entity.PositionLeavesQuantity = 0;
            }

            Entity.PosOpenPrice = openPrice;
            Entity.OpenConversionRate = openConversionRate;

            return this;
        }

        public TradeReportAdapter FillProfitConversionRates(string balanceCurrency, decimal? profit, CalculatorFixture acc)
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

        public TradeReportAdapter FillAccountBalanceConversionRates(CalculatorFixture acc, string balanceCurrency, decimal? balance)
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

            Entity.SrcAssetCurrency = srcAssetCurrency;
            Entity.SrcAssetAmount = (double)srcAssetAmount;
            Entity.SrcAssetMovement = (double)srcAssetMovement;
            Entity.DstAssetCurrency = dstAssetCurrency;
            Entity.DstAssetAmount = (double)dstAssetAmount;
            Entity.DstAssetMovement = (double)dstAssetMovement;

            return this;
        }

        public TradeReportAdapter FillSymbolConversionRates(CalculatorFixture acc, SymbolAccessor symbol)
        {
            if (symbol == null)
                return this;

            if (symbol.MarginCurrency != null)
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
                    Entity.MarginCurrencyToUsdConversionRate = null;
                    Entity.UsdToMarginCurrencyConversionRate = null;
                }

                Entity.MarginCurrency = symbol.MarginCurrency;
            }

            if (symbol.ProfitCurrency != null)
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
                    Entity.ProfitCurrencyToUsdConversionRate = null;
                    Entity.UsdToProfitCurrencyConversionRate = null;
                }

                Entity.ProfitCurrency = symbol.ProfitCurrency;
            }

            return this;
        }

        #endregion
    }
}
