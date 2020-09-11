using System;
using System.Collections.Generic;
using System.Linq;
using TickTrader.Algo.Api;
using TickTrader.Algo.Core.Calc;
using TickTrader.Algo.Core.Infrastructure;
using BL = TickTrader.BusinessLogic;
using BO = TickTrader.BusinessObjects;

namespace TickTrader.Algo.Core
{
    public interface ICalculatorApi
    {
        bool HasEnoughMarginToOpenOrder(SymbolAccessor symbol, double orderVol, OrderType type, OrderSide side, double? price, double? stopPrice, bool isHidden, out CalcErrorCodes error);
        bool HasEnoughMarginToModifyOrder(OrderAccessor oldOrder, SymbolAccessor smb, double newVolume, double? newPrice, double? newStopPrice, bool newIsHidden);
        double? GetSymbolMargin(string symbol, OrderSide side);
        double? CalculateOrderMargin(SymbolAccessor symbol, double orderVol, double? price, double? stopPrice, OrderType type, OrderSide side, bool isHidden);
    }

    internal class CalculatorFixture : ICalculatorApi
    {
        private IFixtureContext _context;
        private MarginAccountCalc _marginCalc;
        private CashAccountCalculator cashCalc;
        private AccountAccessor acc;
        private bool _isRunning;
        private bool _isRestart;

        public CalculatorFixture(IFixtureContext context)
        {
            _context = context;
        }

        public AccountAccessor Acc => acc;
        public AlgoMarketState Market => _context.MarketData;
        public bool IsCalculated => _marginCalc?.IsCalculated ?? true;
        public int RoundingDigits => _marginCalc?.RoundingDigits ??  BL.AccountCalculator.DefaultRounding;

        public void Start()
        {
            acc = _context.Builder.Account;

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

                if (acc.Type == Api.AccountTypes.Gross || acc.Type == Api.AccountTypes.Net)
                {
                    _marginCalc = new MarginAccountCalc(acc, Market, OnCalculatorError, true);
                    acc.MarginCalc = _marginCalc;
                }
                else
                    cashCalc = new CashAccountCalculator(acc, Market, OnCalculatorError);
                acc.EnableBlEvents();
            }
            catch (Exception ex)
            {
                _context.Builder.Logger.OnError("Failed to start account calculator", ex);
                _context.Builder.Logger.OnError(Market.GetSnapshotString());
                _context.Builder.Logger.OnError(acc.GetSnapshotString());
                _marginCalc = null;
                cashCalc = null;
                acc = null;
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

        public OrderCalculator GetCalculator(string symbol, string accountCurrency)
        {
            LazyInit();

            return Market.GetCalculator(symbol, accountCurrency);
        }

        public CurrencyEntity GetCurrencyInfo(string currency)
        {
            return _context.Builder.Currencies.GetOrDefault(currency);
        }

        internal RateUpdate GetCurrentRateOrNull(string symbol)
        {
            var tracker = Market.GetSymbolNodeOrNull(symbol);
            return tracker?.Rate;
        }

        internal RateUpdate GetCurrentRateOrThrow(string symbol)
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
            if (acc != null)
            {
                acc.DisableBlEvents();
                acc = null;
            }
            if (cashCalc != null)
            {
                cashCalc.Dispose();
                cashCalc = null;
            }
            if (_marginCalc != null)
            {
                _marginCalc.Dispose();
                _marginCalc = null;
            }
        }

        #region Emulation

        public InvokeEmulator Emulator { get; set; }

        public void ValidateNewOrder(OrderAccessor newOrder, OrderCalculator fCalc)
        {
            if (acc.IsMarginType)
            {
                //fCalc.UpdateMargin(newOrder, acc);

                //ValidateOrderState(newOrder);

                // Update order initial margin rate.
                //newOrder.MarginRateInitial = newOrder.MarginRateCurrent;

                try
                {
                    // Check for margin
                    double newMargin;
                    var hasMargin = _marginCalc.HasSufficientMarginToOpenOrder(newOrder, out newMargin, out var error);

                    if (error == CalcErrorCodes.NoCrossSymbol)
                        throw new MisconfigException("No cross symbol to convert from " + newOrder.Symbol + " to " + Acc.BalanceCurrency + "!");

                    if (error != CalcErrorCodes.None)
                        throw new OrderValidationError($"Failed to calculate order margin: {error}", error.ToOrderError());

                    if (!hasMargin)
                        throw new OrderValidationError($"Not Enough Money. {this}, NewMargin={newMargin}", Api.OrderCmdResultCodes.NotEnoughMoney);
                }
                catch (BL.MarketConfigurationException e)
                {
                    throw new OrderValidationError(e.Message, Api.OrderCmdResultCodes.InternalError);
                }
            }
            else if (acc.AccountingType == BusinessObjects.AccountingTypes.Cash)
            {

            }
        }

        public void ValidateModifyOrder(OrderAccessor order, decimal newAmount, double? newPrice, double? newStopPrice)
        {
            if (Acc.IsMarginType)
                ValidateModifyOrder_MarginAccount(order, newAmount);
            else if(Acc.IsCashType)
                ValidateModifyOrder_CashAccount(order, newAmount, newPrice, newStopPrice );
        }

        private void ValidateModifyOrder_MarginAccount(OrderAccessor order, decimal newAmount)
        {
            var fCalc = Market.GetCalculator(order.Symbol, Acc.BalanceCurrency);
            using (fCalc.UsageScope())
            {
                //tempOrder.Margin = fCalc.CalculateMargin(tempOrder, this);

                //ValidateOrderState(order);

                //decimal filledAmount = order.Amount - order.RemainingAmount;
                //decimal newRemainingAmount = newAmount - filledAmount;
                var volumeDelta = newAmount - order.Amount;

                if (volumeDelta < 0)
                    return;

                var additionalMargin = fCalc.CalculateMargin((double)newAmount, Acc.Leverage, order.Type.ToBoType(), order.Side.ToBoSide(), false, out var calcErro);

                if (calcErro == CalcErrorCodes.NoCrossSymbol)
                    throw new MisconfigException("No cross symbol to convert from " + order.Symbol + " to " + Acc.BalanceCurrency + "!");

                if (calcErro != CalcErrorCodes.None)
                    throw new OrderValidationError("Not Enough Money.", OrderCmdResultCodes.NotEnoughMoney);

                if (!_marginCalc.HasSufficientMarginToOpenOrder(additionalMargin, order.Symbol, order.Side.ToBoSide()))
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

        public AssetAccessor GetAsset(Currency currency)
        {
            AssetChangeType cType;
            return Acc.Assets.GetOrCreateAsset(currency.Name, out cType);
        }

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

        public bool HasEnoughMarginToOpenOrder(SymbolAccessor symbol, double orderVol, OrderType type, OrderSide side, double? price, double? stopPrice, bool isHidden, out CalcErrorCodes error)
        {
            LazyInit();

            error = CalcErrorCodes.None;

            try
            {
                if (_marginCalc != null)
                {
                    //var calc = _state.GetCalculator(newOrder.Symbol, acc.BalanceCurrency);
                    //calc.UpdateMargin(newOrder, acc);
                    var result = _marginCalc.HasSufficientMarginToOpenOrder(orderVol, symbol.Name, type.ToBoType(), side.ToBoSide(), isHidden, out _, out error);
                    HandleMarginCalcError(error, symbol.Name);
                    return result;
                }

                if (cashCalc != null)
                {
                    var margin = CashAccountCalculator.CalculateMargin(type.ToBoType(), (decimal)orderVol, price, stopPrice, side.ToBoSide(), symbol, isHidden);
                    return cashCalc.HasSufficientMarginToOpenOrder(type.ToBoType(), side.ToBoSide(), symbol, margin);
                }
            }
            catch (BL.NotEnoughMoneyException)
            {}
            catch (Exception ex)
            {
                _context.Builder.Logger.OnError("Failed to calculate margin for new order", ex);
            }
            return false;
        }

        public bool HasEnoughMarginToModifyOrder(OrderAccessor oldOrder, SymbolAccessor symbol, double newVolume, double? newPrice, double? newStopPrice, bool newIsHidden)
        {
            LazyInit();

            var side = oldOrder.Entity.GetBlOrderSide();
            var type = oldOrder.Entity.GetBlOrderType();

            // OrderReplaceRequest with InFlightMitigation == false asks server to set RemainingVolume to specified value
            var newRemVolume = (decimal)newVolume;

            if (_marginCalc != null)
            {
                var calc = Market.GetCalculator(oldOrder.Symbol, acc.BalanceCurrency);
                using (calc.UsageScope())
                {
                    var oldMargin = calc.CalculateMargin(oldOrder.RemainingVolume, acc.Leverage, type, side, oldOrder.Entity.IsHidden, out var error);

                    if (error != CalcErrorCodes.None)
                        return false;

                    var newMargin = calc.CalculateMargin((double)newRemVolume, acc.Leverage, type, side, newIsHidden, out error);

                    HandleMarginCalcError(error, symbol.Name);

                    if (error != CalcErrorCodes.None)
                        return false;

                    var marginDelta = newMargin - oldMargin;

                    if (marginDelta <= 0)
                        return true;

                    return _marginCalc.HasSufficientMarginToOpenOrder(marginDelta, oldOrder.Symbol, side);
                }
            }

            if (cashCalc != null)
            {
                var ordType = oldOrder.Entity.GetBlOrderType();

                var oldMargin = CashAccountCalculator.CalculateMargin(oldOrder, symbol);
                var newMargin = CashAccountCalculator.CalculateMargin(type, (decimal)newRemVolume, newPrice, newStopPrice, side, symbol, newIsHidden);
                return cashCalc.HasSufficientMarginToOpenOrder(type, side, symbol, newMargin - oldMargin);
            }

            return false;
        }

        public double? GetSymbolMargin(string symbol, OrderSide side)
        {
            LazyInit();

            if (_marginCalc != null)
            {
                var netting = _marginCalc.GetSymbolStats(symbol);
                if (netting == null)
                    return 0;

                return side == OrderSide.Buy ? netting.Buy.Margin : netting.Sell.Margin;
            }
            return null;
        }

        public double? CalculateOrderMargin(SymbolAccessor symbol, double orderVol, double? price, double? stopPrice, OrderType type, OrderSide side, bool isHidden)
        {
            LazyInit();

            var boType = type.ToBoType();
            var boSide = side.ToBoSide();

            try
            {
                if (_marginCalc != null)
                {
                    var calc = Market.GetCalculator(symbol.Name, acc.BalanceCurrency);
                    using (calc.UsageScope())
                    {
                        var result = calc.CalculateMargin(orderVol, acc.Leverage, boType, boSide, isHidden, out var error);
                        HandleMarginCalcError(error, symbol.Name);
                        return result;
                    }
                }

                if (cashCalc != null)
                {
                    if (type == OrderType.Stop || type == OrderType.StopLimit)
                    {
                        if (stopPrice == null)
                            return null;
                    }
                    else
                    {
                        if (price == null)
                            return null;
                    }

                    return (double)CashAccountCalculator.CalculateMargin(boType, (decimal)orderVol, price, stopPrice, boSide, symbol, isHidden);
                }
            }
            catch (Exception ex)
            {
                _context.Builder.Logger.OnError("Failed to calculate margin for new order", ex);
            }
            return null;
        }

        private OrderAccessor GetOrderAccessor(OrderEntity orderEntity, SymbolAccessor symbol)
        {
            ApplyHiddenServerLogic(orderEntity, symbol);

            return new OrderAccessor(orderEntity, symbol, acc.Leverage);
        }

        private void ApplyHiddenServerLogic(OrderEntity order, SymbolAccessor symbol)
        {
            //add prices for market orders
            if (order.Type == OrderType.Market && order.Price == null)
            {
                order.Price = order.Side == OrderSide.Buy ? symbol.Ask : symbol.Bid;
                if (acc.Type == AccountTypes.Cash)
                {
                    order.Price += symbol.Point * symbol.Slippage * (order.Side == OrderSide.Buy ? 1 : -1);
                }
            }

            //convert order types for cash accounts
            if (acc.Type == AccountTypes.Cash)
            {
                switch (order.Type)
                {
                    case OrderType.Market:
                        order.Type = OrderType.Limit;
                        order.Options |= OrderOptions.ImmediateOrCancel;
                        break;
                    case OrderType.Stop:
                        order.Type = OrderType.StopLimit;
                        order.Price = order.StopPrice + symbol.Point * symbol.Slippage * (order.Side == OrderSide.Buy ? -1 : 1);
                        break;
                }
            }
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
