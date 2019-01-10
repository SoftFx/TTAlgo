using System;
using System.Collections.Generic;
using System.Linq;
using TickTrader.Algo.Api;
using BL = TickTrader.BusinessLogic;
using BO = TickTrader.BusinessObjects;

namespace TickTrader.Algo.Core
{
    public interface ICalculatorApi
    {
        bool HasEnoughMarginToOpenOrder(OrderEntity orderEntity, SymbolAccessor symbol);
        bool HasEnoughMarginToModifyOrder(OrderAccessor oldOrder, OrderEntity orderEntity, SymbolAccessor symbol);
        double? GetSymbolMargin(string symbol, OrderSide side);
        double? CalculateOrderMargin(OrderEntity orderEntity, SymbolAccessor symbol);
    }

    internal class CalculatorFixture : ICalculatorApi
    {
        private BL.MarketState _state;
        private IFixtureContext _context;
        private MarginCalcAdapter marginCalc;
        private BL.CashAccountCalculator cashCalc;
        private AccountAccessor acc;
        private Dictionary<string, RateUpdate> _lastRates = new Dictionary<string, RateUpdate>();

        public CalculatorFixture(IFixtureContext context)
        {
            _context = context;
        }

        public AccountAccessor Acc => acc;
        public BL.MarketState Market => _state;
        public bool IsCalculated => marginCalc?.IsCalculated ?? true;
        public int RoundingDigits => marginCalc?.RoundingDigits ??  BL.AccountCalculator.DefaultRounding;

        public void Start()
        {
            _lastRates.Clear();

            _state = new BL.MarketState(BL.NettingCalculationTypes.OneByOne);
            _state.Set(_context.Builder.Symbols.OrderBy(s => s.Name).ThenBy(s => s.GroupSortOrder).ThenBy(s => s.SortOrder).ThenBy(s => s.Name));
            _state.Set(_context.Builder.Currencies);

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
                    marginCalc = new MarginCalcAdapter(acc, _state);
                else
                    cashCalc = new BL.CashAccountCalculator(acc, _state);
                acc.EnableBlEvents();
            }
            catch (Exception ex)
            {
                marginCalc = null;
                cashCalc = null;
                acc = null;
                _context.Builder.Logger.OnError("Failed to start account calculator", ex);
            }
        }

        public BL.OrderCalculator GetCalculator(string symbol, string accountCurrency)
        {
            return _state.GetCalculator(symbol, accountCurrency);
        }

        public CurrencyEntity GetCurrencyInfo(string currency)
        {
            return _context.Builder.Currencies.GetOrNull(currency);
        }

        internal void UpdateRate(RateUpdate entity)
        {
            _lastRates[entity.Symbol] = entity;
            _state?.Update((QuoteEntity)entity.LastQuote);
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

        internal void CalculateOrder(OrderAccessor order)
        {
            if (acc.IsMarginType)
                order.Calculator.UpdateOrder(order, acc);
            else if (acc.IsCashType)
            {
                // Calculate new order margin
                //ISymbolInfo symbol = ConfigurationManagerFull.ConvertFromEntity(model.SymbolRef);
                //model.Margin = CashAccountCalculator.CalculateMargin(model, symbol);
            }

        }

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
            if (marginCalc != null)
            {
                marginCalc.Dispose();
                marginCalc = null;
            }
        }

        #region Emulation

        public void ValidateNewOrder(OrderAccessor newOrder, BL.OrderCalculator fCalc)
        {
            if (acc.IsMarginType)
            {
                fCalc.UpdateMargin(newOrder, acc);

                ValidateOrderState(newOrder);

                // Update order initial margin rate.
                //newOrder.MarginRateInitial = newOrder.MarginRateCurrent;

                try
                {
                    // Check for margin
                    decimal oldMargin;
                    decimal newMargin;
                    if (!marginCalc.HasSufficientMarginToOpenOrder(newOrder, newOrder.Margin.NanAwareToDecimal(), out oldMargin, out newMargin))
                        throw new OrderValidationError($"Not Enough Money. {this}, NewMargin={newMargin}", Api.OrderCmdResultCodes.NotEnoughMoney);
                }
                catch (BL.MarketConfigurationException e)
                {
                    throw new OrderValidationError(e.Message, Api.OrderCmdResultCodes.InternalError);
                }
            }
            else if(acc.AccountingType == BusinessObjects.AccountingTypes.Cash)
            {

            }   
        }

        public void ValidateModifyOrder(OrderAccessor order, decimal newAmount, decimal? newPrice, decimal? newStopPrice)
        {
            if (Acc.IsMarginType)
                ValidateModifyOrder_MarginAccount(order, newAmount);
            else if(Acc.IsCashType)
                ValidateModifyOrder_CashAccount(order, newAmount, newPrice, newStopPrice );
        }

        private void ValidateModifyOrder_MarginAccount(OrderAccessor order, decimal newAmount)
        {
            BL.OrderCalculator fCalc = _state.GetCalculator(order.Symbol, Acc.BalanceCurrency);
            //tempOrder.Margin = fCalc.CalculateMargin(tempOrder, this);

            ValidateOrderState(order);

            //decimal filledAmount = order.Amount - order.RemainingAmount;
            //decimal newRemainingAmount = newAmount - filledAmount;
            decimal volumeDelta = newAmount - order.Amount;

            if (volumeDelta < 0)
                return;

            var additionalMargin = fCalc.CalculateMargin(newAmount, Acc.Leverage, TickTraderToAlgo.Convert(order.Type), TickTraderToAlgo.Convert(order.Side), false);
            if (!marginCalc.HasSufficientMarginToOpenOrder(order, additionalMargin))
                throw new OrderValidationError("Not Enough Money.", OrderCmdResultCodes.NotEnoughMoney);
        }

        private void ValidateModifyOrder_CashAccount(OrderAccessor modifiedOrderModel, decimal newAmount, decimal? newPrice, decimal? newStopPrice)
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

        #endregion

        private class MarginCalcAdapter : BL.AccountCalculator
        {
            public MarginCalcAdapter(AccountAccessor accEntity, BL.MarketState market) : base(accEntity, market)
            {
            }

            protected override void OnUpdated()
            {
                var accEntity = (AccountAccessor)Info;

                accEntity.Equity = (double)Equity;
                accEntity.Profit = (double)Profit;
                accEntity.Commision = (double)Commission;
                accEntity.MarginLevel = (double)MarginLevel;
                accEntity.Margin = (double)Margin;
            }
        }

        #region CalculatorApi implementation

        public bool HasEnoughMarginToOpenOrder(OrderEntity orderEntity, SymbolAccessor symbol)
        {
            var newOrder = GetOrderAccessor(orderEntity, symbol);

            try
            {
                if (marginCalc != null)
                {
                    var calc = _state.GetCalculator(newOrder.Symbol, acc.BalanceCurrency);
                    calc.UpdateMargin(newOrder, acc);
                    return marginCalc.HasSufficientMarginToOpenOrder(newOrder, ((BL.IOrderModel)newOrder).Margin);
                }
                if (cashCalc != null)
                {
                    var margin = BL.CashAccountCalculator.CalculateMargin(newOrder, symbol);
                    return cashCalc.HasSufficientMarginToOpenOrder(newOrder, margin);
                }
            }
            catch (BL.NotEnoughMoneyException) { }
            catch (Exception ex)
            {
                _context.Builder.Logger.OnError("Failed to calculate margin for new order", ex);
            }
            return false;
        }

        public bool HasEnoughMarginToModifyOrder(OrderAccessor oldOrder, OrderEntity orderEntity, SymbolAccessor symbol)
        {
            var newOrder = GetOrderAccessor(orderEntity, symbol);

            try
            {
                if (marginCalc != null)
                {
                    var calc = _state.GetCalculator(newOrder.Symbol, acc.BalanceCurrency);
                    calc.UpdateMargin(oldOrder, acc);
                    calc.UpdateMargin(newOrder, acc);
                    return marginCalc.HasSufficientMarginToOpenOrder(newOrder, ((BL.IOrderModel)newOrder).Margin - ((BL.IOrderModel)oldOrder).Margin);
                }
                if (cashCalc != null)
                {
                    var oldMargin = BL.CashAccountCalculator.CalculateMargin(oldOrder, symbol);
                    var newMargin = BL.CashAccountCalculator.CalculateMargin(newOrder, symbol);
                    return cashCalc.HasSufficientMarginToOpenOrder(newOrder, newMargin - oldMargin);
                }
            }
            catch (BL.NotEnoughMoneyException) { }
            catch (Exception ex)
            {
                _context.Builder.Logger.OnError($"Failed to calculate margin for order #{oldOrder.OrderId}", ex);
            }
            return false;
        }

        public double? GetSymbolMargin(string symbol, OrderSide side)
        {
            if (marginCalc != null)
            {
                var netting = marginCalc.GetNetting(symbol);
                if (netting == null)
                    return 0;

                return (double)(side == OrderSide.Buy ? netting.Buy.Margin : netting.Sell.Margin);
            }
            return null;
        }

        public double? CalculateOrderMargin(OrderEntity orderEntity, SymbolAccessor symbol)
        {
            var newOrder = GetOrderAccessor(orderEntity, symbol);

            try
            {
                if (marginCalc != null)
                {
                    var calc = _state.GetCalculator(newOrder.Symbol, acc.BalanceCurrency);
                    calc.UpdateMargin(newOrder, acc);
                    return newOrder.Margin;
                }
                if (cashCalc != null)
                {
                    return (double)BL.CashAccountCalculator.CalculateMargin(newOrder, symbol);
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

            return new OrderAccessor(orderEntity, symbol);
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
