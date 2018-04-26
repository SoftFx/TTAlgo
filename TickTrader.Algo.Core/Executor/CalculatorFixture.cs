using System;
using TickTrader.Algo.Api;
using BL = TickTrader.BusinessLogic;
using BO = TickTrader.BusinessObjects;

namespace TickTrader.Algo.Core
{
    public interface ICalculatorApi
    {
        bool HasEnoughMarginToOpenOrder(OrderEntity orderEntity, SymbolAccessor symbol);
        bool HasEnoughMarginToModifyOrder(OrderAccessor oldOrder, OrderEntity orderEntity, SymbolAccessor symbol);
        double? GetSymbolMargin(SymbolAccessor symbol, OrderSide side);
        double CalculateOrderMargin(OrderEntity orderEntity, SymbolAccessor symbol);
    }


    internal class CalculatorFixture : ICalculatorApi
    {
        private BL.MarketState _state;
        private IFixtureContext _context;
        private MarginCalcAdapter marginCalc;
        private BL.CashAccountCalculator cashCalc;
        private AccountAccessor acc;

        public CalculatorFixture(IFixtureContext context)
        {
            _context = context;
        }

        public void Start()
        {
            _state = new BL.MarketState(BL.NettingCalculationTypes.OneByOne);
            _state.Set(_context.Builder.Symbols);
            _state.Set(_context.Builder.Currencies);

            foreach (var smb in _context.Builder.Symbols)
            {
                var rate = smb.LastQuote as QuoteEntity;
                if (rate != null)
                    _state.Update(rate);
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

        internal void UpdateRate(QuoteEntity entity)
        {
            _state?.Update(entity);
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

        public double? GetSymbolMargin(SymbolAccessor symbol, OrderSide side)
        {
            if (marginCalc != null)
            {
                return 0;
            }
            return null;
        }

        public double CalculateOrderMargin(OrderEntity orderEntity, SymbolAccessor symbol)
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
            catch (BL.NotEnoughMoneyException)
            {
                return double.NaN;
            }
            catch (Exception ex)
            {
                _context.Builder.Logger.OnError("Failed to calculate margin for new order", ex);
            }
            return double.NaN;
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
