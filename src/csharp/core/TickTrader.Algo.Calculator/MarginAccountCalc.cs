using System;
using System.Collections.Generic;
using TickTrader.Algo.Calculator.AlgoMarket;
using TickTrader.Algo.Calculator.TradeSpeсificsCalculators;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Lib.Math;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Calculator
{
    public sealed class MarginAccountCalculator
    {
        private readonly Dictionary<string, SymbolCalc> _bySymbolMap = new Dictionary<string, SymbolCalc>();
        private readonly Action<Exception, string> _onLogError;
        private readonly AlgoMarketState _market;
        private readonly IMarginAccountInfo2 _account;

        private int _totalErrorCount;
        private double _totalCms;
        private double _totalSwap;


        public event Action<MarginAccountCalculator> Updated;

        public MarginAccountCalculator(IMarginAccountInfo2 accInfo, AlgoMarketState market, Action<Exception, string> onLogError)
        {
            _account = accInfo;
            _market = market;
            _onLogError = onLogError;

            AddOrdersBunch(accInfo.Orders);
            AddPositions(accInfo.Positions);

            _account.OrderAdded += AddOrder;
            _account.OrderRemoved += RemoveOrder;
            _account.OrdersAdded += AddOrdersBunch;
            _account.PositionChanged += AddModifyNetPos;
            _account.PositionRemoved += RemoveNetPos;
        }

        public bool IsCalculated => _totalErrorCount <= 0;

        public double Profit { get; private set; }

        public double Equity => _account.Balance + Floating;

        public double Floating => Profit + Commission + Swap;

        public double Margin { get; private set; }

        public double MarginLevel => Margin.Gt(0.0) ? 100 * Equity / Margin : 0.0;

        public double Commission
        {
            get => _totalCms;
            set
            {
                _totalCms = value;

                Updated?.Invoke(this);
            }
        }

        public double Swap
        {
            get => _totalSwap;
            set
            {
                _totalSwap = value;

                Updated?.Invoke(this);
            }
        }

        public void Dispose()
        {
            _account.OrderAdded -= AddOrder;
            _account.OrderRemoved -= RemoveOrder;
            _account.OrdersAdded -= AddOrdersBunch;
            _account.PositionChanged -= AddModifyNetPos;
            _account.PositionRemoved -= RemoveNetPos;

            foreach (var smbCalc in _bySymbolMap.Values)
                DisposeCalc(smbCalc);

            _bySymbolMap.Clear();
        }

        public CalculateResponseBase<bool> HasSufficientMarginToOpenOrder(IOrderCalcInfo order)
        {
            var netting = GetSymbolStats(order.Symbol);
            var calc = netting?.Calculator ?? _market.GetCalculator(order.SymbolInfo);

            var orderMargin = calc.Margin.Calculate(new MarginRequest(order.RemainingAmount, order.Type, order.IsHidden));
            return HasSufficientMarginToOpenOrder(orderMargin.Value, netting, order.Side);
        }

        public CalculateResponseBase<bool> HasSufficientMarginToOpenOrder(double orderAmount, SymbolInfo symbol, OrderInfo.Types.Type type, OrderInfo.Types.Side side, bool isHidden)
        {
            var netting = GetSymbolStats(symbol.Name);
            var calc = netting?.Calculator ?? _market.GetCalculator(symbol);

            var orderMargin = calc.Margin.Calculate(new MarginRequest(orderAmount, type, isHidden));
            if (orderMargin.IsFailed)
            {
                // newAccountMargin = 0;
                //return new CalculateResponseBase<bool>(orderMargin.Error);
                return new CalculateResponseBase<bool>(false);
            }
            return HasSufficientMarginToOpenOrder(orderMargin.Value, netting, side);
        }

        public CalculateResponseBase<bool> HasSufficientMarginToOpenOrder(double orderMargin, string symbol, OrderInfo.Types.Side orderSide)
        {
            var netting = GetSymbolStats(symbol);
            return HasSufficientMarginToOpenOrder(orderMargin, netting, orderSide);
        }

        public CalculateResponseBase<bool> HasSufficientMarginToOpenOrder(double orderMargin, SymbolCalc netting, OrderInfo.Types.Side orderSide)
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
                if (_account.Type == Domain.AccountInfo.Types.Type.Gross)
                {
                    var marginBuy = netting.Buy.Margin;
                    var marginSell = netting.Sell.Margin;
                    smbMargin = Math.Max(marginBuy, marginSell);

                    if (orderSide == Domain.OrderInfo.Types.Side.Buy)
                        newSmbMargin = Math.Max(marginSell, marginBuy + orderMargin);
                    else
                        newSmbMargin = Math.Max(marginSell + orderMargin, marginBuy);
                }
                else if (_account.Type == Domain.AccountInfo.Types.Type.Net)
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
            var newAccountMargin = Margin + marginIncrement;

            return new CalculateResponseBase<bool>(marginIncrement <= 0 || newAccountMargin < Equity);
        }

        public SymbolCalc GetSymbolStats(string symbol)
        {
            _bySymbolMap.TryGetValue(symbol, out SymbolCalc calc);
            return calc;
        }

        private void AddOrder(IOrderCalcInfo order)
        {
            if (order.IgnoreCalculation)
                return;

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
            if (order.IgnoreCalculation)
                return;

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
                if (!order.IgnoreCalculation)
                    AddOrderWithoutCalculation(order);

            foreach (var smb in _bySymbolMap.Values)
                smb.Recalculate(null);
        }

        private void RemoveOrder(IOrderCalcInfo order)
        {
            if (order.IgnoreCalculation)
                return;

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
                smbCalc.UpdateNetPosition(position, type, out var dSwap, out var dComm);

                Swap += dSwap;
                Commission += dComm;
            }
            catch (SymbolNotFoundException snfex)
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
                calc = new SymbolCalc(_market.GetSymbolNodeOrNull(symbol)); //check on null
                calc.StatsChanged += Calc_StatsChanged;
                _bySymbolMap.Add(symbol, calc);
            }
            return calc;
        }

        private void Calc_StatsChanged(StatsChangeToken args)
        {
            Profit += args.ProfitDelta;
            Margin += args.MarginDelta;
            _totalErrorCount += args.ErrorDelta;

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
    }
}
