using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;
using TickTrader.Algo.Core.Calc;
using TickTrader.Algo.Domain;
using BO = TickTrader.BusinessObjects;

namespace TickTrader.Algo.Core
{
    internal static class CommisionEmulator
    {
        public static decimal GetPartialSwap(decimal swap, decimal k, int precision)
        {
            if (k > 1)
                return swap;

            return (swap * k).FloorBy(precision);
        }

        #region Gross & Net

        public static void OnGrossPositionOpened(OrderAccessor position, SymbolAccessor cfg, CalculatorFixture calc)
        {
            var commis = CalculateMarginCommission((OrderCalculator)position.Calculator, position.Entity.RequestedAmount, cfg, calc, position.IsReducedOpenCommission());
            position.ChangeCommission(commis);
        }

        public static void OnGrossPositionClosed(OrderAccessor position, decimal closeAmount, SymbolAccessor cfg, TradeChargesInfo charges, CalculatorFixture calc)
        {
            decimal k = closeAmount / (closeAmount + position.RemainingAmount);

            if (position.Entity.Commission != null)
            {
                if (k == 1)
                    charges.Commission = position.Entity.Commission.Value;
                else
                {
                    // charge comission in proportion of close amount to remaining amount
                    charges.Commission = RoundValue((position.Entity.Commission.Value * k), calc.RoundingDigits);
                    position.Entity.Commission = RoundValue((position.Entity.Commission.Value - charges.Commission), calc.RoundingDigits);
                }
            }

            charges.CurrencyInfo = (CurrencyEntity)calc.Acc.BalanceCurrencyInfo;

            var commiss = CalculateMarginCommission((OrderCalculator)position.Calculator, closeAmount, cfg, calc, position.IsReducedCloseCommission());
            charges.Commission += RoundValue(commiss, calc.RoundingDigits);

            //if (k == 1)
            //    position.Entity.Commission = (double)charges.Commission;
        }

        public static void OnNetPositionOpened(OrderAccessor fromOrder, PositionAccessor position, decimal fillAmount, SymbolAccessor cfg, TradeChargesInfo charges, CalculatorFixture calc)
        {
            charges.Commission = CalculateMarginCommission((OrderCalculator)position.Info.Calculator, fillAmount, cfg, calc, fromOrder.IsReducedOpenCommission());
            charges.CurrencyInfo = (CurrencyEntity)calc.Acc.BalanceCurrencyInfo;
        }

        //public static void OnNetRollover(OrderAccessor position, decimal closeAmount, SymbolAccessor cfg, TradeChargesInfo charges, CalculatorFixture account)
        //{
        //    if (position.Type == OrderType.Position)
        //    {
        //        if (position.Commission != null)
        //        {
        //            decimal k = closeAmount / (closeAmount + position.RemainingAmount);
        //            if (k == 1)
        //                charges.Commission = position.Commission.Value;
        //            else
        //            {
        //                // charge comission in proportion of close amount to remaining amount
        //                charges.Commission = RoundValue((position.Commission.Value * k), account.RoundingDigits);
        //                position.Entity.Commission = (double)RoundValue((position.Commission.Value - charges.Commission), account.RoundingDigits);
        //            }
        //        }
        //    }
        //}

        public static void OnOrderModified(OrderAccessor order, SymbolAccessor cfg, TradeChargesInfo charges, CalculatorFixture account)
        {
            //if (order.Type == OrderType.Position)
            //{
            //    decimal cmsValue = order.IsReducedOpenCommission()
            //        ? (decimal)cfg.CmsValueBookOrders()
            //        : (decimal)cfg.CmsValue();

            //    decimal commiss = order.Calculator.CalculateCommission(order.Amount, cmsValue, cfg.CmsValueType(), cfg.CmsChType());
            //    commiss = ApplyMinimalMarginCommission(commiss, account, cfg);

            //    order.Entity.Commission = (double)RoundValue(commiss, account.RoundingDigits);
            //}
        }

        private static double ApplyMinimalMarginCommission(double commiss, CalculatorFixture account, SymbolAccessor cfg)
        {
            //decimal minCommissConvRate = account.GetMinCommissionConversionRate(cfg.CmsMinValueCurrency);
            //decimal minCommiss = -(decimal)cfg.CmsMinValue * minCommissConvRate;

            //if (minCommiss < commiss)
            //{
            //    commiss = minCommiss;
            //    if (charges != null)
            //    {
            //        charges.MinCommissionCurrency = cfg.CmsMinValueCurrency;
            //        charges.MinCommissionConversionRate = minCommissConvRate;
            //    }
            //}

            return commiss;
        }

        private static decimal CalculateMarginCommission(OrderCalculator orderCalc, decimal amount, SymbolAccessor cfg, CalculatorFixture accCalc, bool isReduced)
        {
            double cmsValue = isReduced
               ? cfg.CmsValueBookOrders()
               : cfg.CmsValue();

            double commiss = orderCalc.CalculateCommission((double)amount, cmsValue, cfg.CommissionValueType, out var error);
            commiss = ApplyMinimalMarginCommission(commiss, accCalc, cfg);
            return RoundValue(commiss, accCalc.RoundingDigits);
        }

        #endregion

        #region Cash

        public static void OnOrderFilled(OrderAccessor order, double fillAmount, double fillPrice, CalculatorFixture acc, SymbolAccessor cfg, TradeChargesInfo charges)
        {
            var currency = (order.Side == Domain.OrderInfo.Types.Side.Buy) ? cfg.MarginCurrencyInfo : cfg.ProfitCurrencyInfo;
            var asset = acc.GetAsset(currency);

            var amount = (order.Side == Domain.OrderInfo.Types.Side.Buy) ? fillAmount : fillAmount * fillPrice;
            var commiss = CalculateCommission(amount, cfg, order.IsReducedOpenCommission(), acc, currency.Name, charges);
            var commission = RoundValue(commiss, currency.Digits);

            ChargeCommission(commission, asset, acc);
            //tradeReport.Commission = commission;
            charges.Commission = commission;
            charges.CurrencyInfo = (CurrencyEntity)currency;
            //FillExecutionReport(execReport, asset, commission);
        }

        private static void ChargeCommission(decimal commisionAmount, AssetAccessor asset, CalculatorFixture acc)
        {
            if (asset != null && asset.FreeVolume > Math.Abs(commisionAmount))
                acc.Acc.IncreaseAsset(asset.Currency, commisionAmount);
            //else
            //    infrustructure.Logger.Warn(() => "Cannot charge commission for account " + acc.Id + " because it lacks free amount of " + asset.Currency);
        }

        private static double CalculateCommission(double amount, SymbolAccessor cfg, bool isReduced, CalculatorFixture acc, string commissCurrency, TradeChargesInfo charges)
        {
            double commiss = 0;
            if (isReduced)
                // special commission for Book orders
                commiss = CalculateCommission(amount, cfg.CmsValueBookOrders(), cfg.CommissionValueType);
            else
                // ordinary comission
                commiss = CalculateCommission(amount, cfg.CmsValue(), cfg.CommissionValueType);

            commiss = ApplyMinimalCashCommission(commiss, commissCurrency, acc, cfg, charges);

            return commiss;
        }

        private static double CalculateCommission(double amount, float cmsValue, Domain.CommissonInfo.Types.ValueType cmsType)
        {
            if (cmsType == Domain.CommissonInfo.Types.ValueType.Percentage)
                return -((amount * cmsValue) / 100);

            return 0;
        }

        private static double ApplyMinimalCashCommission(double commiss, string commissCurrency, CalculatorFixture account, SymbolAccessor cfg, TradeChargesInfo charges)
        {
            //decimal minCommissConvRate = account.GetMinCommissionConversionRate(cfg.CmsMinValueCurrency, commissCurrency);
            //decimal minCommiss = -(decimal)cfg.CmsMinValue * minCommissConvRate;

            //if (minCommiss < commiss)
            //{
            //    commiss = minCommiss;
            //    charges.MinCommissionCurrency = cfg.CmsMinValueCurrency;
            //    charges.MinCommissionConversionRate = minCommissConvRate;
            //    tradeReport.MinCommissionCurrency = cfg.CmsMinValueCurrency;
            //    tradeReport.MinCommissionConversionRate = minCommissConvRate;
            //}

            return commiss;
        }

        #endregion

        private static decimal? RoundValue(double? volume, int precision)
        {
            return RoundValue((decimal?)volume, precision);
        }

        private static decimal? RoundValue(decimal? volume, int precision)
        {
            return volume?.FloorBy(precision);
        }

        private static decimal RoundValue(double volume, int precision)
        {
            return RoundValue((decimal)volume, precision);
        }

        private static decimal RoundValue(decimal volume, int precision)
        {
            return volume.FloorBy(precision);
        }
    }
}
