using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.BusinessLogic;
using TickTrader.BusinessObjects;

namespace TickTrader.Algo.Core.Calc
{
    internal class MarginAccountCalc
    {
        private readonly MarketState _market;
        private readonly IDictionary<string, SymbolCalc> _bySymbolMap = new Dictionary<string, SymbolCalc>();

        public MarginAccountCalc(IMarginAccountInfo2 accInfo, MarketState market)
        {
            Info = accInfo;
            _market = market;

            _market.CurrenciesChanged += InitRounding;
            InitRounding();

            AddOrdersBunch(accInfo.Orders);

            Info.OrderAdded += AddOrder;
            Info.OrderRemoved += RemoveOrder;
            Info.OrdersAdded += AddOrdersBunch;
            Info.PositionChanged += UpdateNetPos;
        }

        public IMarginAccountInfo2 Info { get; }
        public bool IsCalculated => true;
        public int RoundingDigits { get; private set; }
        public decimal Profit { get; private set; }
        public decimal Equity => Info.Balance + Profit + Commission + Swap;
        public decimal Margin { get; private set; }
        public decimal MarginLevel => CalculateMarginLevel();
        public decimal Commission { get; private set; }
        //public decimal AgentCommission { get; private set; }
        public decimal Swap { get; private set; }

        public void Dispose()
        {
            _market.CurrenciesChanged -= InitRounding;
        }

        public bool HasSufficientMarginToOpenOrder(IOrderModel2 order)
        {
            return HasSufficientMarginToOpenOrder(order, out _);
        }

        public bool HasSufficientMarginToOpenOrder(IOrderModel2 order, out decimal newAccountMargin)
        {
            var netting = GetSymbolStats(order.Symbol);
            var calc = netting?.Calc ?? _market.GetCalculator(order.Symbol, Info.BalanceCurrency);
            var orderMargin = calc.CalculateMargin(order.RemainingAmount, Info.Leverage, order.Type, order.Side, order.IsHidden);
            return HasSufficientMarginToOpenOrder(orderMargin, netting, order.Side, out newAccountMargin);
        }

        public bool HasSufficientMarginToOpenOrder(decimal orderAmount, string symbol, OrderTypes type, OrderSides side, bool isHidden, out decimal newAccountMargin)
        {
            var netting = GetSymbolStats(symbol);
            var calc = netting?.Calc ?? _market.GetCalculator(symbol, Info.BalanceCurrency);
            var orderMargin = calc.CalculateMargin(orderAmount, Info.Leverage, type, side, isHidden);
            return HasSufficientMarginToOpenOrder(orderMargin, netting, side, out newAccountMargin);
        }

        public bool HasSufficientMarginToOpenOrder(decimal orderMargin, string symbol, OrderSides orderSide)
        {
            var netting = GetSymbolStats(symbol);
            return HasSufficientMarginToOpenOrder(orderMargin, netting, orderSide, out _);
        }

        public bool HasSufficientMarginToOpenOrder(decimal orderMargin, SymbolCalc netting, OrderSides orderSide, out decimal newAccountMargin)
        {
            decimal smbMargin;
            decimal newSmbMargin;

            if (netting == null)
            {
                smbMargin = 0;
                newSmbMargin = orderMargin;
            }
            else
            {
                if (Info.AccountingType == AccountingTypes.Gross)
                {
                    var marginBuy = netting.Buy.Margin;
                    var marginSell = netting.Sell.Margin;
                    smbMargin = Math.Max(marginBuy, marginSell);

                    if (orderSide == OrderSides.Buy)
                        newSmbMargin = Math.Max(marginSell, marginBuy + orderMargin);
                    else
                        newSmbMargin = Math.Max(marginSell + orderMargin, marginBuy);
                }
                else if (Info.AccountingType == AccountingTypes.Net)
                {
                    var marginBuy = netting.Buy.Margin;
                    var marginSell = netting.Sell.Margin;
                    smbMargin = Math.Max(marginBuy, marginSell);
                    newSmbMargin = orderMargin;

                    if ((orderSide == OrderSides.Buy) && (marginBuy > 0))
                        newSmbMargin = marginBuy + orderMargin;
                    else if ((orderSide == OrderSides.Buy) && (marginSell > 0))
                        newSmbMargin = Math.Abs(marginSell - orderMargin);
                    else if ((orderSide == OrderSides.Sell) && (marginSell > 0))
                        newSmbMargin = marginSell + orderMargin;
                    else if ((orderSide == OrderSides.Sell) && (marginBuy > 0))
                        newSmbMargin = Math.Abs(marginBuy - orderMargin);
                }
                else
                    throw new Exception("Not a margin account!");
            }

            var marginIncrement = newSmbMargin - smbMargin;
            newAccountMargin = Margin + marginIncrement;

            return marginIncrement <= 0 || newAccountMargin < Equity;
        }

        public SymbolCalc GetSymbolStats(string symbol)
        {
            SymbolCalc calc;
            _bySymbolMap.TryGetValue(symbol, out calc);
            return calc;
        }

        public void EnableAutoUpdate()
        {
            _market.RateChanged += a =>
            {
                var smbCalc = GetSymbolStats(a.Symbol);
                if (smbCalc != null)
                    smbCalc.Recalculate();
            };
        }

        private void AddOrder(IOrderModel2 order)
        {
            AddInternal(order);
            GetOrAddSymbolCalculator(order.Symbol).AddOrder(order);
        }

        private void AddOrderWithoutCalculation(IOrderModel2 order)
        {
            AddInternal(order);
            GetOrAddSymbolCalculator(order.Symbol).AddOrderWithoutCalculation(order);
        }

        private void AddInternal(IOrderModel2 order)
        {
            Swap += order.Swap ?? 0;
            Commission += order.Commission ?? 0;
            order.SwapChanged += Order_SwapChanged;
            order.CommissionChanged += Order_CommissionChanged;
        }

        private void AddOrdersBunch(IEnumerable<IOrderModel2> bunch)
        {
            foreach (var order in bunch)
                AddOrderWithoutCalculation(order);

            foreach (var smb in _bySymbolMap.Values)
                smb.Recalculate();
        }

        private void RemoveOrder(IOrderModel2 order)
        {
            Swap -= order.Swap ?? 0;
            Commission -= order.Commission ?? 0;
            order.SwapChanged -= Order_SwapChanged;
            order.CommissionChanged -= Order_CommissionChanged;
            var smbCalc = GetOrAddSymbolCalculator(order.Symbol);
            smbCalc.RemoveOrder(order);
            RemoveIfEmpty(smbCalc);
        }

        private void UpdateNetPos(IPositionModel position, PositionChageTypes chType)
        {
            var smbCalc = GetOrAddSymbolCalculator(position.Symbol);
            smbCalc.UpdatePosition(position, out var dSwap, out var dComm);
            Swap += dSwap;
            Commission += dComm;
        }

        private void RemoveIfEmpty(SymbolCalc calc)
        {
            if (calc.IsEmpty)
            {
                calc.StatsChanged -= Calc_StatsChanged;
                _bySymbolMap.Remove(calc.Symbol);
                //netting.Dispose();
            }
        }

        private SymbolCalc GetOrAddSymbolCalculator(string symbol)
        {
            SymbolCalc calc;
            if (!_bySymbolMap.TryGetValue(symbol, out calc))
            {
                calc = new SymbolCalc(symbol, Info, _market);
                calc.StatsChanged += Calc_StatsChanged;
                _bySymbolMap.Add(symbol, calc);
            }
            return calc;
        }

        private void Calc_StatsChanged(StatsChange args)
        {
            Profit += args.ProfitDelta;
            Margin += args.MarginDelta;
        }

        private void Order_SwapChanged(OrderPropArgs<decimal> args)
        {
            Swap += args.NewVal - args.OldVal;
        }

        private void Order_CommissionChanged(OrderPropArgs<decimal> args)
        {
            Commission += args.NewVal - args.OldVal;
        }

        private decimal CalculateMarginLevel()
        {
            if (Margin > 0)
                return 100 * Equity / Margin;
            else
                return 0;
        }

        private void InitRounding()
        {
            ICurrencyInfo curr = _market.GetCurrencyOrThrow(Info.BalanceCurrency);
            if (curr != null && curr.Precision >= 0)
                RoundingDigits = curr.Precision;
            else
                RoundingDigits = 2;
        }
    }
}
