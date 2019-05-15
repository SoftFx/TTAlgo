using System;
using System.Collections.Generic;
using System.Linq;
using TickTrader.Algo.Api;
using TickTrader.Algo.Core.Calc;
using BL = TickTrader.BusinessLogic;
using BO = TickTrader.BusinessObjects;

namespace TickTrader.Algo.Core
{
    public interface ICalculatorApi
    {
        bool HasEnoughMarginToOpenOrder(string symbol, double orderVol, OrderType type, OrderSide side, bool isHidden);
        bool HasEnoughMarginToModifyOrder(OrderEntity oldOrder, double newVolume, bool newIsHidden);
        double? GetSymbolMargin(string symbol, OrderSide side);
        double? CalculateOrderMargin(string symbol, double orderVol, OrderType type, OrderSide side, bool isHidden);
    }

    internal class CalculatorFixture : ICalculatorApi
    {
        private MarketState _state;
        private IFixtureContext _context;
        private MarginAccountCalc _marginCalc;
        private BL.CashAccountCalculator cashCalc;
        private AccountAccessor acc;
        private Dictionary<string, RateUpdate> _lastRates = new Dictionary<string, RateUpdate>();

        public CalculatorFixture(IFixtureContext context)
        {
            _context = context;
        }

        public AccountAccessor Acc => acc;
        public MarketState Market => _state;
        public bool IsCalculated => _marginCalc?.IsCalculated ?? true;
        public int RoundingDigits => _marginCalc?.RoundingDigits ??  BL.AccountCalculator.DefaultRounding;

        public void Start()
        {
            _lastRates.Clear();

            var orderedSymbols = _context.Builder.Symbols.OrderBy(s => s.Name).ThenBy(s => s.GroupSortOrder).ThenBy(s => s.SortOrder).ThenBy(s => s.Name);

            _state = new MarketState();
            _state.Init(orderedSymbols, _context.Builder.Currencies);

            foreach (var smb in _context.Builder.Symbols)
            {
                var rate = smb.LastQuote as QuoteEntity;
                if (rate != null)
                {
                    _state.Update(rate);
                    _lastRates[smb.Name] = rate;
                }
            }

            try
            {
                acc = _context.Builder.Account;
                if (acc.Type == Api.AccountTypes.Gross || acc.Type == Api.AccountTypes.Net)
                {
                    _marginCalc = new MarginAccountCalc(acc, _state);
                    _marginCalc.EnableAutoUpdate();
                    acc.MarginCalc = _marginCalc;
                }
                //else
                //    cashCalc = new BL.CashAccountCalculator(acc, _state);
                acc.EnableBlEvents();
            }
            catch (Exception ex)
            {
                _marginCalc = null;
                cashCalc = null;
                acc = null;
                _context.Builder.Logger.OnError("Failed to start account calculator", ex);
            }
        }

        public OrderCalculator GetCalculator(string symbol, string accountCurrency)
        {
            return _state.GetCalculator(symbol, accountCurrency);
        }

        public CurrencyEntity GetCurrencyInfo(string currency)
        {
            return _context.Builder.Currencies.GetOrNull(currency);
        }

        internal void UpdateRate(RateUpdate rate)
        {
            _lastRates[rate.Symbol] = rate;
            _state?.Update(rate);
        }

        internal RateUpdate GetCurrentRateOrNull(string symbol)
        {
            RateUpdate rate;
            _lastRates.TryGetValue(symbol, out rate);
            return rate;
        }

        internal RateUpdate GetCurrentRateOrThrow(string symbol)
        {
            RateUpdate rate;
            if (!_lastRates.TryGetValue(symbol, out rate))
                throw new OrderValidationError("Off Quotes: " + symbol, OrderCmdResultCodes.OffQuotes);
            return rate;
        }

        //internal void CalculateOrder(OrderAccessor order)
        //{
        //    if (acc.IsMarginType)
        //        order.Calculator.UpdateOrder(order, acc);
        //    else if (acc.IsCashType)
        //    {
        //        // Calculate new order margin
        //        //ISymbolInfo symbol = ConfigurationManagerFull.ConvertFromEntity(model.SymbolRef);
        //        //model.Margin = CashAccountCalculator.CalculateMargin(model, symbol);
        //    }

        //}

        public void Stop()
        {
            _state = null;
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

        public void ValidateNewOrder(OrderAccessor newOrder, OrderCalculator fCalc)
        {
            if (acc.IsMarginType)
            {
                //fCalc.UpdateMargin(newOrder, acc);

                ValidateOrderState(newOrder);

                // Update order initial margin rate.
                //newOrder.MarginRateInitial = newOrder.MarginRateCurrent;

                try
                {
                    // Check for margin
                    double newMargin;
                    var hasMargin = _marginCalc.HasSufficientMarginToOpenOrder(newOrder, out newMargin, out var error);

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

        public void ValidateModifyOrder(OrderAccessor order, double newAmount, double? newPrice, double? newStopPrice)
        {
            if (Acc.IsMarginType)
                ValidateModifyOrder_MarginAccount(order, newAmount);
            else if(Acc.IsCashType)
                ValidateModifyOrder_CashAccount(order, newAmount, newPrice, newStopPrice );
        }

        private void ValidateModifyOrder_MarginAccount(OrderAccessor order, double newAmount)
        {
            var fCalc = _state.GetCalculator(order.Symbol, Acc.BalanceCurrency);
            using (fCalc.UsageScope())
            {
                //tempOrder.Margin = fCalc.CalculateMargin(tempOrder, this);

                ValidateOrderState(order);

                //decimal filledAmount = order.Amount - order.RemainingAmount;
                //decimal newRemainingAmount = newAmount - filledAmount;
                double volumeDelta = newAmount - order.Amount;

                if (volumeDelta < 0)
                    return;

                var additionalMargin = fCalc.CalculateMargin(newAmount, Acc.Leverage, order.Type.ToBoType(), order.Side.ToBoSide(), false, out var calcErro);

                if (calcErro != CalcErrorCodes.None)
                    throw new OrderValidationError("Not Enough Money.", OrderCmdResultCodes.NotEnoughMoney);

                if (!_marginCalc.HasSufficientMarginToOpenOrder(additionalMargin, order.Symbol, order.Side.ToBoSide()))
                    throw new OrderValidationError("Not Enough Money.", OrderCmdResultCodes.NotEnoughMoney);
            }
        }

        private void ValidateModifyOrder_CashAccount(OrderAccessor modifiedOrderModel, double newAmount, double? newPrice, double? newStopPrice)
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

        private void ValidateOrderState(OrderAccessor order)
        {
            if (order.CalculationError != null)
            {
                if (order.CalculationError.Code == BL.OrderErrorCode.Misconfiguration)
                    throw new MisconfigException(order.CalculationError.Description);
                else
                    throw new OrderValidationError(order.CalculationError.Description, OrderCmdResultCodes.OffQuotes);
            }
        }

        //public IEnumerable<OrderAccessor> GetGrossPositions(string symbol)
        //{
        //    return _marginCalc.GetNetting(symbol)?.Orders.Where(o => o.Type == BO.OrderTypes.Position).Cast<OrderAccessor>();
        //}

        #endregion

        //private class MarginCalcAdapter : BL.AccountCalculator
        //{
        //    public MarginCalcAdapter(AccountAccessor accEntity, BL.MarketState market) : base(accEntity, market)
        //    {
        //    }

        //    protected override void OnUpdated()
        //    {
        //        var accEntity = (AccountAccessor)Info;

        //        accEntity.Equity = (double)Equity;
        //        accEntity.Profit = (double)Profit;
        //        accEntity.Commision = (double)Commission;
        //        accEntity.MarginLevel = (double)MarginLevel;
        //        accEntity.Margin = (double)Margin;
        //    }
        //}

        #region CalculatorApi implementation

        public bool HasEnoughMarginToOpenOrder(string symbol, double orderVol, OrderType type, OrderSide side, bool isHidden)
        {
            //var newOrder = GetOrderAccessor(orderEntity, symbol);

            try
            {
                if (_marginCalc != null)
                {
                    //var calc = _state.GetCalculator(newOrder.Symbol, acc.BalanceCurrency);
                    //calc.UpdateMargin(newOrder, acc);
                    return _marginCalc.HasSufficientMarginToOpenOrder(orderVol, symbol, type.ToBoType(), side.ToBoSide(), isHidden, out _, out var error);
                }
                //if (cashCalc != null)
                //{
                //    var margin = BL.CashAccountCalculator.CalculateMargin(newOrder, symbol);
                //    return cashCalc.HasSufficientMarginToOpenOrder(newOrder, margin);
                //}
            }
            catch (BL.NotEnoughMoneyException) { }
            catch (Exception ex)
            {
                _context.Builder.Logger.OnError("Failed to calculate margin for new order", ex);
            }
            return false;
        }

        public bool HasEnoughMarginToModifyOrder(OrderEntity oldOrder, double newVolume, bool newIsHidden)
        {
            //var newOrder = GetOrderAccessor(orderEntity, symbol);

            if (_marginCalc != null)
            {
                var side = oldOrder.GetBlOrderSide();
                var type = oldOrder.GetBlOrderType();

                var volumeDelta = newVolume - oldOrder.Volume;
                var newRemVolume = newVolume + volumeDelta;

                var calc = _state.GetCalculator(oldOrder.Symbol, acc.BalanceCurrency);
                using (calc.UsageScope())
                {
                    var oldMargin = calc.CalculateMargin(oldOrder.RemainingVolume, acc.Leverage, type, side, oldOrder.IsHidden, out var error);

                    if (error != CalcErrorCodes.None)
                        return false;

                    var newMargin = calc.CalculateMargin(newRemVolume, acc.Leverage, type, side, newIsHidden, out error);

                    if (error != CalcErrorCodes.None)
                        return false;

                    var marginDelta = newMargin - oldMargin;

                    if (marginDelta <= 0)
                        return true;

                    return _marginCalc.HasSufficientMarginToOpenOrder(marginDelta, oldOrder.Symbol, side);
                }
            }
            //catch (BL.NotEnoughMoneyException) { }
            //catch (Exception ex)
            //{
            //    _context.Builder.Logger.OnError($"Failed to calculate margin for order #{oldOrder.OrderId}", ex);
            //}
            return false;
        }

        public double? GetSymbolMargin(string symbol, OrderSide side)
        {
            if (_marginCalc != null)
            {
                var netting = _marginCalc.GetSymbolStats(symbol);
                if (netting == null)
                    return 0;

                return side == OrderSide.Buy ? netting.Buy.Margin : netting.Sell.Margin;
            }
            return null;
        }

        public double? CalculateOrderMargin(string symbol, double orderVol, OrderType type, OrderSide side, bool isHidden)
        {
            //var newOrder = GetOrderAccessor(orderEntity, symbol);

            try
            {
                if (_marginCalc != null)
                {
                    var calc = _state.GetCalculator(symbol, acc.BalanceCurrency);
                    using (calc.UsageScope())
                        return calc?.CalculateMargin(orderVol, acc.Leverage, type.ToBoType(), side.ToBoSide(), isHidden, out var error);
                }
                if (cashCalc != null)
                {
                    //return (double)BL.CashAccountCalculator.CalculateMargin(newOrder, symbol);
                    return null;
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
                    order.Price += symbol.Point * symbol.DefaultSlippage * (order.Side == OrderSide.Buy ? 1 : -1);
                }
            }

            //convert order types for cash accounts
            if (acc.Type == AccountTypes.Cash)
            {
                switch (order.Type)
                {
                    case OrderType.Market:
                        order.Type = OrderType.Limit;
                        order.Options |= OrderExecOptions.ImmediateOrCancel;
                        break;
                    case OrderType.Stop:
                        order.Type = OrderType.StopLimit;
                        order.Price = order.StopPrice + symbol.Point * symbol.DefaultSlippage * (order.Side == OrderSide.Buy ? -1 : 1);
                        break;
                }
            }
        }

        #endregion CalculatorApi implementation
    }
}
