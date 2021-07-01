using System;
using System.Collections.Generic;
using TickTrader.Algo.Calculator.AlgoMarket;
using TickTrader.Algo.Calculator.TradeSpeсificsCalculators;
using TickTrader.Algo.Core;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Domain.CalculatorInterfaces;

namespace TickTrader.Algo.Calculator
{
    public class MarginAccountCalculator
    {
        private readonly AlgoMarketState _market;
        private readonly IDictionary<string, SymbolCalc> _bySymbolMap = new Dictionary<string, SymbolCalc>();
        private int _errorCount;
        //private bool _autoUpdate;
        private double _cms;
        private double _swap;
        //private double _dblCms;
        //private double _dblSwap;
        private Action<Exception, string> _onLogError;

        public event Action<MarginAccountCalculator> Updated;

        public MarginAccountCalculator(IMarginAccountInfo2 accInfo, AlgoMarketState market, Action<Exception, string> onLogError/*, bool autoUpdate = false*/)
        {
            Info = accInfo;
            _market = market;
            //_autoUpdate = autoUpdate;
            _onLogError = onLogError;

            //_market.CurrenciesChanged += InitRounding;

            RoundingDigits = market.Account.BalanceCurrency.Digits;

            AddOrdersBunch(accInfo.Orders);
            AddPositions(accInfo.Positions);

            Info.OrderAdded += AddOrder;
            Info.OrderRemoved += RemoveOrder;
            Info.OrdersAdded += AddOrdersBunch;
            Info.PositionChanged += AddModifyNetPos;
            Info.PositionRemoved += RemoveNetPos;
        }

        public IMarginAccountInfo2 Info { get; }
        public bool IsCalculated => _errorCount <= 0;
        public int RoundingDigits { get; private set; }
        public double Profit { get; private set; }
        public double Equity => Info.Balance + Floating;
        public double Floating => Profit + Commission + Swap;
        public double Margin { get; private set; }
        public double MarginLevel => CalculateMarginLevel();

        public double Commission
        {
            get => _cms;
            set
            {
                _cms = value;
                //_dblCms = value;

                Updated?.Invoke(this);
            }
        }

        public double Swap
        {
            get => _swap;
            set
            {
                _swap = value;
                //_dblSwap = (double)value;

                Updated?.Invoke(this);
            }
        }

        public void Dispose()
        {
            //_market.CurrenciesChanged -= InitRounding;

            Info.OrderAdded -= AddOrder;
            Info.OrderRemoved -= RemoveOrder;
            Info.OrdersAdded -= AddOrdersBunch;
            Info.PositionChanged -= AddModifyNetPos;
            Info.PositionRemoved -= RemoveNetPos;

            foreach (var smbCalc in _bySymbolMap.Values)
                DisposeCalc(smbCalc);

            _bySymbolMap.Clear();
        }

        public CalculateResponseBase<bool> HasSufficientMarginToOpenOrder(IOrderCalcInfo order)
        {
            return HasSufficientMarginToOpenOrder(order, out _);
        }

        public CalculateResponseBase<bool> HasSufficientMarginToOpenOrder(IOrderCalcInfo order, out double newAccountMargin)
        {
            var netting = GetSymbolStats(order.Symbol);
            var calc = netting?.Calc ?? _market.GetCalculator(order.SymbolInfo);
            //using (calc.UsageScope())
            //{
                var orderMargin = calc.Margin.Calculate(new MarginRequest(order.RemainingAmount, order.Type, order.IsHidden));
                return HasSufficientMarginToOpenOrder(orderMargin.Value, netting, order.Side, out newAccountMargin);
            //}
        }

        public CalculateResponseBase<bool> HasSufficientMarginToOpenOrder(double orderAmount, SymbolInfo symbol, Domain.OrderInfo.Types.Type type, Domain.OrderInfo.Types.Side side, bool isHidden, out double newAccountMargin)
        {
            var netting = GetSymbolStats(symbol.Name);
            var calc = netting?.Calc ?? _market.GetCalculator(symbol);
            //using (calc.UsageScope())
            //{
                var orderMargin = calc.Margin.Calculate(new MarginRequest(orderAmount, type, isHidden));
                if (orderMargin.IsFailed)
                {
                    newAccountMargin = 0;
                    //return new CalculateResponseBase<bool>(orderMargin.Error);
                    return new CalculateResponseBase<bool>(false);
                }
                return HasSufficientMarginToOpenOrder(orderMargin.Value, netting, side, out newAccountMargin);
            //}
        }

        public CalculateResponseBase<bool> HasSufficientMarginToOpenOrder(double orderMargin, string symbol, Domain.OrderInfo.Types.Side orderSide)
        {
            var netting = GetSymbolStats(symbol);
            return HasSufficientMarginToOpenOrder(orderMargin, netting, orderSide, out _);
        }

        public CalculateResponseBase<bool> HasSufficientMarginToOpenOrder(double orderMargin, SymbolCalc netting, Domain.OrderInfo.Types.Side orderSide, out double newAccountMargin)
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
                if (Info.Type == Domain.AccountInfo.Types.Type.Gross)
                {
                    var marginBuy = netting.Buy.Margin;
                    var marginSell = netting.Sell.Margin;
                    smbMargin = Math.Max(marginBuy, marginSell);

                    if (orderSide == Domain.OrderInfo.Types.Side.Buy)
                        newSmbMargin = Math.Max(marginSell, marginBuy + orderMargin);
                    else
                        newSmbMargin = Math.Max(marginSell + orderMargin, marginBuy);
                }
                else if (Info.Type == Domain.AccountInfo.Types.Type.Net)
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

            return new CalculateResponseBase<bool>(marginIncrement <= 0 || newAccountMargin < Equity);
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

        private void AddOrder(IOrderCalcInfo order)
        {
            try
            {
                AddInternal(order);
                GetOrAddSymbolCalculator(order.Symbol).AddOrder(order);
            }
            catch (SymbolNotFoundException snfex)
            {
                _onLogError?.Invoke(null, $"{nameof(MarginAccountCalculator)} failed to add order: {snfex.Message}. {order?.GetSnapshotString()}");
            }
            catch (Exception ex)
            {
                _onLogError?.Invoke(ex, $"{nameof(MarginAccountCalculator)} failed to add order. {order?.GetSnapshotString()}");
            }
        }

        private void AddOrderWithoutCalculation(IOrderCalcInfo order)
        {
            try
            {
                AddInternal(order);
                GetOrAddSymbolCalculator(order.Symbol).AddOrderWithoutCalculation(order);
            }
            catch (SymbolNotFoundException snfex)
            {
                _onLogError?.Invoke(null, $"{nameof(MarginAccountCalculator)} failed to add order without calculation: {snfex.Message}. {order?.GetSnapshotString()}");
            }
            catch (Exception ex)
            {
                _onLogError?.Invoke(ex, $"{nameof(MarginAccountCalculator)} failed to add order without calculation. {order?.GetSnapshotString()}");
            }
        }

        private void AddInternal(IOrderCalcInfo order)
        {
            Swap += order.Swap;
            Commission += order.Commission;
            order.SwapChanged += Order_SwapChanged;
            order.CommissionChanged += Order_CommissionChanged;
        }

        private void AddOrdersBunch(IEnumerable<IOrderCalcInfo> bunch)
        {
            foreach (var order in bunch)
                AddOrderWithoutCalculation(order);

            foreach (var smb in _bySymbolMap.Values)
                smb.Recalculate(null);
        }

        private void RemoveOrder(IOrderCalcInfo order)
        {
            try
            {
                Swap -= order.Swap;
                Commission -= order.Commission;
                order.SwapChanged -= Order_SwapChanged;
                order.CommissionChanged -= Order_CommissionChanged;
                var smbCalc = GetOrAddSymbolCalculator(order.Symbol);
                smbCalc.RemoveOrder(order);
                RemoveIfEmpty(smbCalc);
            }
            catch (SymbolNotFoundException snfex)
            {
                _onLogError?.Invoke(null, $"{nameof(MarginAccountCalculator)} failed to remove order: {snfex.Message}. {order?.GetSnapshotString()}");
            }
            catch (Exception ex)
            {
                _onLogError?.Invoke(ex, $"{nameof(MarginAccountCalculator)} failed to remove order. {order?.GetSnapshotString()}");
            }
        }

        private void AddPositions(IEnumerable<IPositionInfo> positions)
        {
            if (positions != null)
            {
                foreach (var pos in positions)
                    AddModifyNetPos(pos);
            }
        }

        private void AddModifyNetPos(IPositionInfo position) => UpdateNetPos(position, PositionChangeTypes.AddedModified);

        private void RemoveNetPos(IPositionInfo position) => UpdateNetPos(position, PositionChangeTypes.Removed);

        private void UpdateNetPos(IPositionInfo position, PositionChangeTypes type)
        {
            try
            {
                var smbCalc = GetOrAddSymbolCalculator(position.Symbol);
                smbCalc.UpdatePosition(position, type, out var dSwap, out var dComm);
                Swap += dSwap;
                Commission += dComm;
            }
            catch(SymbolNotFoundException snfex)
            {
                _onLogError?.Invoke(null, $"{nameof(MarginAccountCalculator)} failed to update net position: {snfex.Message}. {position?.GetSnapshotString()}");
            }
            catch (Exception ex)
            {
                _onLogError?.Invoke(ex, $"{nameof(MarginAccountCalculator)} failed to update net position. {position?.GetSnapshotString()}");
            }
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
            if (!_bySymbolMap.TryGetValue(symbol, out SymbolCalc calc))
            {
                calc = new SymbolCalc(symbol, _market);
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

            Updated?.Invoke(this);
        }

        private void Order_SwapChanged(OrderPropArgs<double> args)
        {
            Swap += args.NewVal - args.OldVal;
        }

        private void Order_CommissionChanged(OrderPropArgs<double> args)
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
    }
}
