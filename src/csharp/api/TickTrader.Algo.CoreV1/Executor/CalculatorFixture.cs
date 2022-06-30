using System;
using TickTrader.Algo.Api;
using TickTrader.Algo.Calculator;
using TickTrader.Algo.Calculator.AlgoMarket;
using TickTrader.Algo.Calculator.TradeSpecificsCalculators;
using TickTrader.Algo.Core;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Domain.CalculatorInterfaces;

namespace TickTrader.Algo.CoreV1
{
    public interface ICalculatorApi
    {
        bool HasEnoughMarginToOpenOrder(SymbolInfo symbol, double orderVol, Domain.OrderInfo.Types.Type type, Domain.OrderInfo.Types.Side side, double? price, double? stopPrice, bool isHidden, out CalculationError error);
        bool HasEnoughMarginToModifyOrder(OrderAccessor oldOrder, SymbolInfo smb, double newVolume, double? newPrice, double? newStopPrice, bool newIsHidden);
        double? GetSymbolMargin(string symbol, OrderInfo.Types.Side side);
        double? CalculateOrderMargin(SymbolInfo symbol, double orderVol, double? price, double? stopPrice, Domain.OrderInfo.Types.Type type, Domain.OrderInfo.Types.Side side, bool isHidden);
    }

    internal class CalculatorFixture : ICalculatorApi
    {
        private IFixtureContext _context;
        private MarginAccountCalculator _marginCalc;
        private CashAccountCalculator _cashCalc;
        private bool _isRunning;
        private bool _shouldRestart;

        public CalculatorFixture(IFixtureContext context)
        {
            _context = context;
        }

        public AccountAccessor Acc { get; private set; }
        public AlgoMarketState Market => _context.MarketData;
        public bool IsCalculated => _marginCalc?.IsCalculated ?? true;
        public int RoundingDigits => Acc?.BalanceCurrencyInfo.Digits ?? 2;

        public void Start()
        {
            Acc = _context.Builder.Account;

            //var orderedSymbols = _context.Builder.Symbols.OrderBy(s => s.Name).ThenBy(s => s.GroupSortOrder).ThenBy(s => s.SortOrder).ThenBy(s => s.Name);

            //_context.Dispenser.AddSubscription(orderedSymbols.
            _context.Builder.Account.CalcRequested += LazyInit;
        }

        public void PreRestart()
        {
            _shouldRestart = _isRunning;
        }

        public void PostRestart()
        {
            if (_shouldRestart)
            {
                _shouldRestart = false;
                LazyInit();
            }
        }

        private void LazyInit()
        {
            if (!_isRunning)
            {
                _isRunning = true;
                StartCalculator();
            }
        }

        private void StartCalculator()
        {
            try
            {
                _context.Builder.Account.OnTradeInfoAccess();

                if (Acc.IsMarginType)
                {
                    _marginCalc = new MarginAccountCalculator(Acc, Market, OnCalculatorError);
                    Acc.MarginCalc = _marginCalc;
                }
                else
                    _cashCalc = new CashAccountCalculator(Acc, Market, OnCalculatorError);
                Acc.EnableBlEvents();
            }
            catch (Exception ex)
            {
                _context.Builder.Logger.OnError("Failed to start account calculator", ex);
                _context.Builder.Logger.OnError(Market.GetSnapshotString());
                _context.Builder.Logger.OnError(Acc.GetSnapshotString());
                _marginCalc = null;
                _cashCalc = null;
                Acc = null;
            }
        }

        private void OnCalculatorError(Exception ex, string msg)
        {
            if (ex != null)
            {
                _context.Builder.Logger.OnError(msg, ex);
            }
            else
            {
                _context.Builder.Logger.OnError(msg);
            }
        }

        //public ISymbolCalculator GetCalculator(string symbol, IMarginAccountInfo2 account) // emulator
        //{
        //    LazyInit();

        //    return Market.GetCalculator(symbol);
        //}

        public CurrencyAccessor GetCurrencyInfo(string currency)
        {
            return _context.Builder.Currencies.GetOrNull(currency);
        }

        internal IQuoteInfo GetCurrentRateOrNull(string symbol)
        {
            var tracker = Market.GetSymbolNodeOrNull(symbol);
            return tracker?.SymbolInfo?.LastQuote;
        }

        internal IQuoteInfo GetCurrentRateOrThrow(string symbol)
        {
            var tracker = Market.GetSymbolNodeOrNull(symbol);
            if (tracker == null)
                throw new OrderValidationError("Off Quotes: " + symbol, OrderCmdResultCodes.OffQuotes);
            return tracker?.SymbolInfo?.LastQuote;
        }

        public void Stop()
        {
            _isRunning = false;

            if (_context.Builder != null)
                _context.Builder.Account.CalcRequested -= LazyInit;

            //_state = null;
            if (Acc != null)
            {
                Acc.DisableBlEvents();
                Acc = null;
            }
            if (_cashCalc != null)
            {
                _cashCalc.Dispose();
                _cashCalc = null;
            }
            if (_marginCalc != null)
            {
                _marginCalc.Dispose();
                _marginCalc = null;
            }
        }

        #region Emulation

        public Action<Exception> OnFatalError { get; set; }

        public void ValidateNewOrder(OrderAccessor newOrder) //for Emulator
        {
            LazyInit();

            if (Acc.IsMarginType)
            {
                //fCalc.UpdateMargin(newOrder, acc);

                //ValidateOrderState(newOrder);

                // Update order initial margin rate.
                //newOrder.MarginRateInitial = newOrder.MarginRateCurrent;

                try
                {
                    // Check for margin
                    var hasMargin = _marginCalc.HasSufficientMarginToOpenOrder(newOrder.Info);

                    HandleMarginCalcResult(hasMargin, newOrder);
                }
                catch (MarketConfigurationException e)
                {
                    throw new OrderValidationError(e.Message, Api.OrderCmdResultCodes.InternalError);
                }
            }
            else if (Acc.IsCashType)
            {

            }
        }

        public void ValidateModifyOrder(OrderAccessor order, double newAmount, double? newPrice, double? newStopPrice) // for Emulator
        {
            if (Acc.IsMarginType)
                ValidateModifyOrder_MarginAccount(order, newAmount);
        }

        private void ValidateModifyOrder_MarginAccount(OrderAccessor order, double newAmount)
        {
            try
            {
                //tempOrder.Margin = fCalc.CalculateMargin(tempOrder, this);

                //ValidateOrderState(order);

                //decimal filledAmount = order.Amount - order.RemainingAmount;
                //decimal newRemainingAmount = newAmount - filledAmount;
                var volumeDelta = newAmount - order.Info.RequestedAmount;

                if (volumeDelta < 0)
                    return;

                var orderInfo = order.Info;
                var hasMargin = _marginCalc.HasSufficientMarginToOpenOrder(volumeDelta, orderInfo.SymbolInfo, orderInfo.Type, orderInfo.Side, orderInfo.IsHidden);

                HandleMarginCalcResult(hasMargin, order);
            }
            catch (MarketConfigurationException e)
            {
                throw new OrderValidationError(e.Message, Api.OrderCmdResultCodes.InternalError);
            }
        }

        private void HandleMarginCalcResult(CalculateResponseBase<bool> marginRes, OrderAccessor order)
        {
            var error = marginRes.Error;
            if (error == CalculationError.NoCrossSymbol)
                throw new MisconfigException("No cross symbol to convert from " + order.Info.Symbol + " to " + Acc.BalanceCurrency + "!");

            if (error != CalculationError.None)
                throw new OrderValidationError($"Failed to calculate order {order.Info.Id} margin: {error}", ToOrderError(error));

            if (!marginRes.Value)
                throw new OrderValidationError("Not Enough Money.", OrderCmdResultCodes.NotEnoughMoney);
        }

        private OrderCmdResultCodes ToOrderError(CalculationError error)
        {
            switch (error)
            {
                case CalculationError.SymbolNotFound:
                    return OrderCmdResultCodes.SymbolNotFound;
                case CalculationError.OffQuote:
                case CalculationError.OffCrossQuote:
                    return OrderCmdResultCodes.OffQuotes;
                default:
                    return OrderCmdResultCodes.UnknownError;
            }
        }

        #endregion

        #region CalculatorApi implementation

        public bool HasEnoughMarginToOpenOrder(SymbolInfo symbol, double orderVol, Domain.OrderInfo.Types.Type type, Domain.OrderInfo.Types.Side side, double? price, double? stopPrice, bool isHidden, out CalculationError error)
        {
            LazyInit();

            error = CalculationError.None;

            try
            {
                if (_marginCalc != null)
                {
                    var result = _marginCalc.HasSufficientMarginToOpenOrder(orderVol, symbol, type, side, isHidden);
                    HandleMarginCalcError(result.Error, symbol.Name);
                    return result.Value;
                }

                if (_cashCalc != null)
                {
                    var margin = CashAccountCalculator.CalculateMargin(type, orderVol, price, stopPrice, side, symbol, isHidden, null);
                    return _cashCalc.HasSufficientMarginToOpenOrder(type, side, symbol, margin);
                }
            }
            catch (NotEnoughMoneyException)
            { }
            catch (Exception ex)
            {
                _context.Builder.Logger.OnError("Failed to calculate margin for new order", ex);
            }
            return false;
        }

        public bool HasEnoughMarginToModifyOrder(OrderAccessor oldOrder, SymbolInfo symbol, double newVolume, double? newPrice, double? newStopPrice, bool newIsHidden)
        {
            LazyInit();

            try
            {
                var side = oldOrder.Info.Side;
                var type = oldOrder.Info.Type;

                // OrderReplaceRequest with InFlightMitigation == false asks server to set RemainingVolume to specified value
                var newRemVolume = newVolume;

                if (_marginCalc != null)
                {
                    var calc = Market.GetCalculator(oldOrder.Info.SymbolInfo);
                    //using (calc.UsageScope())
                    //{
                    var oldMargin = calc.Margin.Calculate(new MarginRequest(oldOrder.Info.RemainingAmount / oldOrder.LotSize, type, oldOrder.Info.IsHidden));

                    if (oldMargin.IsFailed)
                        return false;

                    var newMargin = calc.Margin.Calculate(new MarginRequest(newRemVolume, type, newIsHidden));

                    HandleMarginCalcError(newMargin.Error, symbol.Name);

                    if (newMargin.IsFailed)
                        return false;

                    var marginDelta = newMargin.Value - oldMargin.Value;

                    if (marginDelta <= 0)
                        return true;

                    return _marginCalc.HasSufficientMarginToOpenOrder(marginDelta, oldOrder.Info.Symbol, side);
                    //}
                }

                if (_cashCalc != null)
                {
                    var oldMargin = CashAccountCalculator.CalculateMargin(oldOrder.Info, symbol);
                    var newMargin = CashAccountCalculator.CalculateMargin(type, newRemVolume, newPrice, newStopPrice, side, symbol, newIsHidden, null);
                    return _cashCalc.HasSufficientMarginToOpenOrder(type, side, symbol, newMargin - oldMargin);
                }
            }
            catch (NotEnoughMoneyException)
            { }
            catch (Exception ex)
            {
                _context.Builder.Logger.OnError("Failed to calculate margin for modify order", ex);
            }

            return false;
        }

        public double? GetSymbolMargin(string symbol, Domain.OrderInfo.Types.Side side)
        {
            LazyInit();

            if (_marginCalc != null)
            {
                var netting = _marginCalc.GetSymbolStats(symbol);
                if (netting == null)
                    return 0;

                return side == Domain.OrderInfo.Types.Side.Buy ? netting.Buy.Margin : netting.Sell.Margin;
            }
            return null;
        }

        public double? CalculateOrderMargin(SymbolInfo symbol, double orderVol, double? price, double? stopPrice, Domain.OrderInfo.Types.Type type, Domain.OrderInfo.Types.Side side, bool isHidden)
        {
            LazyInit();

            try
            {
                if (_marginCalc != null)
                {
                    var calc = Market.GetCalculator(symbol);
                    //using (calc.UsageScope())
                    //{
                    var result = calc.Margin.Calculate(new MarginRequest(orderVol, type, isHidden));
                    HandleMarginCalcError(result.Error, symbol.Name);
                    return result.Value;
                    //}
                }

                if (_cashCalc != null)
                {
                    if (type == Domain.OrderInfo.Types.Type.Stop || type == Domain.OrderInfo.Types.Type.StopLimit)
                    {
                        if (stopPrice == null)
                            return null;
                    }
                    else
                    {
                        if (price == null)
                            return null;
                    }

                    return CashAccountCalculator.CalculateMargin(type, orderVol, price, stopPrice, side, symbol, isHidden, null);
                }
            }
            catch (Exception ex)
            {
                _context.Builder.Logger.OnError("Failed to calculate margin for new order", ex);
            }
            return null;
        }

        private void HandleMarginCalcError(CalculationError code, string symbol)
        {
            if (code == CalculationError.NoCrossSymbol && OnFatalError != null)
            {
                var error = new MisconfigException($"No cross symbol to convert from {symbol} to {Acc.BalanceCurrency}!");
                OnFatalError(error);
                throw error;
            }
        }

        #endregion CalculatorApi implementation
    }
}
