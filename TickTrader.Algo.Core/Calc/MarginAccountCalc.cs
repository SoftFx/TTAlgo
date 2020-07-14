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
        private readonly MarketStateBase _market;
        private readonly IDictionary<string, SymbolCalc> _bySymbolMap = new Dictionary<string, SymbolCalc>();
        private int _errorCount;
        private bool _autoUpdate;
        private decimal _cms;
        private decimal _swap;
        private double _dblCms;
        private double _dblSwap;

        public MarginAccountCalc(IMarginAccountInfo2 accInfo, MarketStateBase market, bool autoUpdate = false)
        {
            Info = accInfo;
            _market = market;
            _autoUpdate = autoUpdate;

            _market.CurrenciesChanged += InitRounding;
            InitRounding();

            AddOrdersBunch(accInfo.Orders);
            AddPositions(accInfo.Positions);

            Info.OrderAdded += AddOrder;
            Info.OrderRemoved += RemoveOrder;
            Info.OrdersAdded += AddOrdersBunch;
            Info.PositionChanged += UpdateNetPos;
        }

        public IMarginAccountInfo2 Info { get; }
        public bool IsCalculated => _errorCount <= 0;
        public int RoundingDigits { get; private set; }
        public double Profit { get; private set; }
        public double Equity => Info.Balance + Profit + _dblCms + _dblSwap;
        public double Margin { get; private set; }
        public double MarginLevel => CalculateMarginLevel();

        public decimal Commission
        {
            get => _cms;
            set
            {
                _cms = value;
                _dblCms = (double)value;
            }
        }

        public decimal Swap
        {
            get => _swap;
            set
            {
                _swap = value;
                _dblSwap = (double)value;
            }
        }

        public void Dispose()
        {
            _market.CurrenciesChanged -= InitRounding;

            Info.OrderAdded -= AddOrder;
            Info.OrderRemoved -= RemoveOrder;
            Info.OrdersAdded -= AddOrdersBunch;
            Info.PositionChanged -= UpdateNetPos;

            foreach (var smbCalc in _bySymbolMap.Values)
                DisposeCalc(smbCalc);

            _bySymbolMap.Clear();
        }

        public bool HasSufficientMarginToOpenOrder(IOrderModel2 order, out CalcErrorCodes error)
        {
            return HasSufficientMarginToOpenOrder(order, out _, out error);
        }

        public bool HasSufficientMarginToOpenOrder(IOrderModel2 order, out double newAccountMargin, out CalcErrorCodes error)
        {
            var netting = GetSymbolStats(order.Symbol);
            var calc = netting?.Calc ?? _market.GetCalculator(order.Symbol, Info.BalanceCurrency);
            using (calc.UsageScope())
            {
                var orderMargin = calc.CalculateMargin((double)order.RemainingAmount, Info.Leverage, order.Type, order.Side, order.IsHidden, out error);
                return HasSufficientMarginToOpenOrder(orderMargin, netting, order.Side, out newAccountMargin);
            }
        }

        public bool HasSufficientMarginToOpenOrder(double orderAmount, string symbol, OrderTypes type, Domain.OrderInfo.Types.Side side, bool isHidden,
            out double newAccountMargin, out CalcErrorCodes error)
        {
            var netting = GetSymbolStats(symbol);
            var calc = netting?.Calc ?? _market.GetCalculator(symbol, Info.BalanceCurrency);
            using (calc.UsageScope())
            {
                var orderMargin = calc.CalculateMargin(orderAmount, Info.Leverage, type, side, isHidden, out error);
                if (error != CalcErrorCodes.None)
                {
                    newAccountMargin = 0;
                    return false;
                }
                return HasSufficientMarginToOpenOrder(orderMargin, netting, side, out newAccountMargin);
            }
        }

        public bool HasSufficientMarginToOpenOrder(double orderMargin, string symbol, Domain.OrderInfo.Types.Side orderSide)
        {
            var netting = GetSymbolStats(symbol);
            return HasSufficientMarginToOpenOrder(orderMargin, netting, orderSide, out _);
        }

        public bool HasSufficientMarginToOpenOrder(double orderMargin, SymbolCalc netting, Domain.OrderInfo.Types.Side orderSide, out double newAccountMargin)
        {
            double smbMargin;
            double newSmbMargin;

            if (netting == null)
            {
                smbMargin = 0;
                newSmbMargin = orderMargin;
            }
            else
            {
                if (Info.AccountingType == Domain.AccountInfo.Types.Type.Gross)
                {
                    var marginBuy = netting.Buy.Margin;
                    var marginSell = netting.Sell.Margin;
                    smbMargin = Math.Max(marginBuy, marginSell);

                    if (orderSide == Domain.OrderInfo.Types.Side.Buy)
                        newSmbMargin = Math.Max(marginSell, marginBuy + orderMargin);
                    else
                        newSmbMargin = Math.Max(marginSell + orderMargin, marginBuy);
                }
                else if (Info.AccountingType == Domain.AccountInfo.Types.Type.Net)
                {
                    var marginBuy = netting.Buy.Margin;
                    var marginSell = netting.Sell.Margin;
                    smbMargin = Math.Max(marginBuy, marginSell);
                    newSmbMargin = orderMargin;

                    if ((orderSide == Domain.OrderInfo.Types.Side.Buy) && (marginBuy > 0))
                        newSmbMargin = marginBuy + orderMargin;
                    else if ((orderSide == Domain.OrderInfo.Types.Side.Buy) && (marginSell > 0))
                        newSmbMargin = Math.Abs(marginSell - orderMargin);
                    else if ((orderSide == Domain.OrderInfo.Types.Side.Sell) && (marginSell > 0))
                        newSmbMargin = marginSell + orderMargin;
                    else if ((orderSide == Domain.OrderInfo.Types.Side.Sell) && (marginBuy > 0))
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

        //public void EnableAutoUpdate()
        //{
        //    _market.RateChanged += a =>
        //    {
        //        var smbCalc = GetSymbolStats(a.Symbol);
        //        if (smbCalc != null)
        //            smbCalc.Recalculate();
        //    };
        //}

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

        private void AddPositions(IEnumerable<IPositionModel2> positions)
        {
            if (positions != null)
            {
                foreach (var pos in positions)
                    UpdateNetPos(pos);
            }
        }

        private void UpdateNetPos(IPositionModel2 position)
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
                _bySymbolMap.Remove(calc.Symbol);
                DisposeCalc(calc);
            }
        }

        private void DisposeCalc(SymbolCalc calc)
        {
            calc.StatsChanged -= Calc_StatsChanged;
            calc.Dispose();
        }

        private SymbolCalc GetOrAddSymbolCalculator(string symbol)
        {
            SymbolCalc calc;
            if (!_bySymbolMap.TryGetValue(symbol, out calc))
            {
                calc = new SymbolCalc(symbol, Info, _market, _autoUpdate);
                calc.StatsChanged += Calc_StatsChanged;
                _bySymbolMap.Add(symbol, calc);
            }
            return calc;
        }

        private void Calc_StatsChanged(StatsChange args)
        {
            Profit += args.ProfitDelta;
            Margin += args.MarginDelta;
            _errorCount += args.ErrorDelta;
        }

        private void Order_SwapChanged(OrderPropArgs<decimal> args)
        {
            Swap += args.NewVal - args.OldVal;
        }

        private void Order_CommissionChanged(OrderPropArgs<decimal> args)
        {
            Commission += args.NewVal - args.OldVal;
        }

        private double CalculateMarginLevel()
        {
            if (Margin > 0)
                return 100 * Equity / Margin;
            else
                return 0;
        }

        private void InitRounding()
        {
            var curr = _market.GetCurrencyOrThrow(Info.BalanceCurrency);
            RoundingDigits = curr != null && curr.Digits >= 0 ? curr.Digits : 2;
        }
    }
}
