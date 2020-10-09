using System;
using TickTrader.Algo.Api;
using TickTrader.Algo.Core.Calc;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Core
{
    public interface ICalculatorApi
    {
        bool HasEnoughMarginToOpenOrder(SymbolInfo symbol, double orderVol, Domain.OrderInfo.Types.Type type, Domain.OrderInfo.Types.Side side, double? price, double? stopPrice, bool isHidden, out CalcErrorCodes error);
        bool HasEnoughMarginToModifyOrder(OrderAccessor oldOrder, SymbolInfo smb, double newVolume, double? newPrice, double? newStopPrice, bool newIsHidden);
        double? GetSymbolMargin(string symbol, Domain.OrderInfo.Types.Side side);
        double? CalculateOrderMargin(SymbolInfo symbol, double orderVol, double? price, double? stopPrice, Domain.OrderInfo.Types.Type type, Domain.OrderInfo.Types.Side side, bool isHidden);
    }

    internal class CalculatorFixture : ICalculatorApi
    {
        private IFixtureContext _context;
        private MarginAccountCalculator _marginCalc;
        private CashAccountCalculator _cashCalc;
        private bool _isRunning;
        private bool _isRestart;

        public CalculatorFixture(IFixtureContext context)
        {
            _context = context;
        }

        public AccountAccessor Acc { get; private set; }
        public AlgoMarketState Market => _context.MarketData;
        public bool IsCalculated => _marginCalc?.IsCalculated ?? true;
        public int RoundingDigits => _marginCalc?.RoundingDigits ?? 2;

        public void Start()
        {
            Acc = _context.Builder.Account;

            //var orderedSymbols = _context.Builder.Symbols.OrderBy(s => s.Name).ThenBy(s => s.GroupSortOrder).ThenBy(s => s.SortOrder).ThenBy(s => s.Name);

            //_context.Dispenser.AddSubscription(orderedSymbols.
            _context.Builder.Account.CalcRequested += LazyInit;
        }

        public void PreRestart()
        {
            _isRestart = true;
        }

        public void PostRestart()
        {
            if (_isRestart)
            {
                _isRestart = false;
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
                    _marginCalc = new MarginAccountCalculator(Acc, Market, OnCalculatorError, true);
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

        private void OnCalculatorError(string msg, Exception ex)
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

        public OrderCalculator GetCalculator(string symbol, IMarginAccountInfo2 account)
        {
            LazyInit();

            return Market.GetCalculator(symbol, account);
        }

        public CurrencyAccessor GetCurrencyInfo(string currency)
        {
            return _context.Builder.Currencies.GetOrNull(currency);
        }

        internal IRateInfo GetCurrentRateOrNull(string symbol)
        {
            var tracker = Market.GetSymbolNodeOrNull(symbol);
            return tracker?.Rate;
        }

        internal IRateInfo GetCurrentRateOrThrow(string symbol)
        {
            var tracker = Market.GetSymbolNodeOrNull(symbol);
            if (tracker == null)
                throw new OrderValidationError("Off Quotes: " + symbol, OrderCmdResultCodes.OffQuotes);
            return tracker.Rate;
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

        public InvokeEmulator Emulator { get; set; }

        public void ValidateNewOrder(OrderAccessor newOrder, IOrderCalculator fCalc) //for Emulator
        {
            if (Acc.IsMarginType)
            {
                //fCalc.UpdateMargin(newOrder, acc);

                //ValidateOrderState(newOrder);

                // Update order initial margin rate.
                //newOrder.MarginRateInitial = newOrder.MarginRateCurrent;

                try
                {
                    // Check for margin
                    var hasMargin = _marginCalc.HasSufficientMarginToOpenOrder(newOrder.Info, out double newMargin, out var error);

                    if (error == CalcErrorCodes.NoCrossSymbol)
                        throw new MisconfigException("No cross symbol to convert from " + newOrder.Info.Symbol + " to " + Acc.BalanceCurrency + "!");

                    if (error != CalcErrorCodes.None)
                        throw new OrderValidationError($"Failed to calculate order margin: {error}", error.ToOrderError());

                    if (!hasMargin)
                        throw new OrderValidationError($"Not Enough Money. {this}, NewMargin={newMargin}", Api.OrderCmdResultCodes.NotEnoughMoney);
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

        public void ValidateModifyOrder(OrderAccessor order, decimal newAmount, double? newPrice, double? newStopPrice) // for Emulator
        {
            if (Acc.IsMarginType)
                ValidateModifyOrder_MarginAccount(order, newAmount);
            else if (Acc.IsCashType)
                ValidateModifyOrder_CashAccount(order, newAmount, newPrice, newStopPrice);
        }

        private void ValidateModifyOrder_MarginAccount(OrderAccessor order, decimal newAmount)
        {
            var fCalc = Market.GetCalculator(order.Info.Symbol, Acc);
            using (fCalc.UsageScope())
            {
                //tempOrder.Margin = fCalc.CalculateMargin(tempOrder, this);

                //ValidateOrderState(order);

                //decimal filledAmount = order.Amount - order.RemainingAmount;
                //decimal newRemainingAmount = newAmount - filledAmount;
                var volumeDelta = newAmount - order.Entity.RequestedAmount;

                if (volumeDelta < 0)
                    return;

                var additionalMargin = fCalc.CalculateMargin((double)newAmount, order.Info.Type, order.Info.Side, false, out var calcErro);

                if (calcErro == CalcErrorCodes.NoCrossSymbol)
                    throw new MisconfigException("No cross symbol to convert from " + order.Info.Symbol + " to " + Acc.BalanceCurrency + "!");

                if (calcErro != CalcErrorCodes.None)
                    throw new OrderValidationError("Not Enough Money.", OrderCmdResultCodes.NotEnoughMoney);

                if (!_marginCalc.HasSufficientMarginToOpenOrder(additionalMargin, order.Info.Symbol, order.Info.Side))
                    throw new OrderValidationError("Not Enough Money.", OrderCmdResultCodes.NotEnoughMoney);
            }
        }

        private void ValidateModifyOrder_CashAccount(OrderAccessor modifiedOrderModel, decimal newAmount, double? newPrice, double? newStopPrice)
        {
            //if (request.NewVolume.HasValue || request.Price.HasValue || request.StopPrice.HasValue)
            //{
            //    // Clone the order and update its price
            //    OrderLightClone tempOrder = new OrderLightClone(modifiedOrderModel);
            //    tempOrder.Price = request.Price ?? tempOrder.Price;
            //    tempOrder.StopPrice = request.StopPrice ?? tempOrder.StopPrice;

            //    if (request.NewVolume.HasValue && request.RemainingAmount.HasValue && modifiedOrderModel.IsPending)
            //    {
            //        tempOrder.Amount = request.Amount.Value;
            //        tempOrder.RemainingAmount = request.RemainingAmount.Value;
            //    }

            //    // Calculate temp order margin
            //    //ISymbolInfo symbol = ConfigurationManagerFull.ConvertFromEntity(modifiedOrderModel.SymbolRef);
            //    tempOrder.Margin = CashAccountCalculator.CalculateMargin(tempOrder, symbol);

            //    // Check for margin
            //    try
            //    {
            //        decimal oldMargin = modifiedOrderModel.Margin.GetValueOrDefault();
            //        decimal newMargin = tempOrder.Margin.GetValueOrDefault();
            //        calc.HasSufficientMarginToOpenOrder(tempOrder, newMargin - oldMargin);
            //    }
            //    catch (NotEnoughMoneyException ex)
            //    {
            //        throw new ServerFaultException<NotEnoughMoneyFault>($"Not Enough Money. {ex.Message}");
            //    }
            //}
        }

        //public AssetAccessor GetAsset(Currency currency)
        //{
        //    return Acc.Assets.GetOrAdd(currency.Name, out _);
        //}

        //private void ValidateOrderState(OrderAccessor order)
        //{
        //    if (order.CalculationError != null)
        //    {
        //        if (order.CalculationError.Code == BL.OrderErrorCode.Misconfiguration)
        //            throw new MisconfigException(order.CalculationError.Description);
        //        else
        //            throw new OrderValidationError(order.CalculationError.Description, OrderCmdResultCodes.OffQuotes);
        //    }
        //}

        //public IEnumerable<OrderAccessor> GetGrossPositions(string symbol)
        //{
        //    return _marginCalc.GetNetting(symbol)?.Orders.Where(o => o.Type == BO.OrderTypes.Position).Cast<OrderAccessor>();
        //}

        #endregion

        #region CalculatorApi implementation

        public bool HasEnoughMarginToOpenOrder(SymbolInfo symbol, double orderVol, Domain.OrderInfo.Types.Type type, Domain.OrderInfo.Types.Side side, double? price, double? stopPrice, bool isHidden, out CalcErrorCodes error)
        {
            LazyInit();

            error = CalcErrorCodes.None;

            try
            {
                if (_marginCalc != null)
                {
                    var result = _marginCalc.HasSufficientMarginToOpenOrder(orderVol, symbol.Name, type, side, isHidden, out _, out error);
                    HandleMarginCalcError(error, symbol.Name);
                    return result;
                }

                if (_cashCalc != null)
                {
                    var margin = CashAccountCalculator.CalculateMargin(type, (decimal)orderVol, price, stopPrice, side, symbol, isHidden);
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

            var side = oldOrder.Info.Side;
            var type = oldOrder.Info.Type;

            // OrderReplaceRequest with InFlightMitigation == false asks server to set RemainingVolume to specified value
            var newRemVolume = (decimal)newVolume;

            if (_marginCalc != null)
            {
                var calc = Market.GetCalculator(oldOrder.Info.Symbol, Acc);
                using (calc.UsageScope())
                {
                    var oldMargin = calc.CalculateMargin(oldOrder.Info.RequestedAmount / oldOrder.LotSize, type, side, oldOrder.Info.IsHidden, out var error);

                    if (error != CalcErrorCodes.None)
                        return false;

                    var newMargin = calc.CalculateMargin((double)newRemVolume, type, side, newIsHidden, out error);

                    HandleMarginCalcError(error, symbol.Name);

                    if (error != CalcErrorCodes.None)
                        return false;

                    var marginDelta = newMargin - oldMargin;

                    if (marginDelta <= 0)
                        return true;

                    return _marginCalc.HasSufficientMarginToOpenOrder(marginDelta, oldOrder.Info.Symbol, side);
                }
            }

            if (_cashCalc != null)
            {
                var oldMargin = CashAccountCalculator.CalculateMargin(oldOrder.Info, symbol);
                var newMargin = CashAccountCalculator.CalculateMargin(type, (decimal)newRemVolume, newPrice, newStopPrice, side, symbol, newIsHidden);
                return _cashCalc.HasSufficientMarginToOpenOrder(type, side, symbol, newMargin - oldMargin);
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
                    var calc = Market.GetCalculator(symbol.Name, Acc);
                    using (calc.UsageScope())
                    {
                        var result = calc.CalculateMargin(orderVol, type, side, isHidden, out var error);
                        HandleMarginCalcError(error, symbol.Name);
                        return result;
                    }
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

                    return (double)CashAccountCalculator.CalculateMargin(type, (decimal)orderVol, price, stopPrice, side, symbol, isHidden);
                }
            }
            catch (Exception ex)
            {
                _context.Builder.Logger.OnError("Failed to calculate margin for new order", ex);
            }
            return null;
        }

        private void HandleMarginCalcError(CalcErrorCodes code, string symbol)
        {
            if (code == CalcErrorCodes.NoCrossSymbol && Emulator != null)
            {
                var error = new MisconfigException("No cross symbol to convert from " + symbol + " to " + Acc.BalanceCurrency + "!");
                Emulator.SetFatalError(error);
                throw error;
            }
        }

        #endregion CalculatorApi implementation
    }
}
