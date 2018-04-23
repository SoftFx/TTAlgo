using System;
using BL = TickTrader.BusinessLogic;
using BO = TickTrader.BusinessObjects;

namespace TickTrader.Algo.Core
{
    public interface ICalculatorApi
    {
        bool HasEnoughMarginToOpenOrder(BL.IOrderModel newOrder, BO.ISymbolInfo symbolInfo);
        bool HasEnoughMarginToModifyOrder(BL.IOrderModel oldOrder, BL.IOrderModel newOrder, BO.ISymbolInfo symbolInfo);
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

        #region ICalculatorApi implementation

        public bool HasEnoughMarginToOpenOrder(BL.IOrderModel newOrder, BO.ISymbolInfo symbolInfo)
        {
            try
            {
                if (marginCalc != null)
                {
                    var calc = _state.GetCalculator(newOrder.Symbol, acc.BalanceCurrency);
                    calc.UpdateMargin(newOrder, acc);
                    return marginCalc.HasSufficientMarginToOpenOrder(newOrder, newOrder.Margin);
                }
                if (cashCalc != null)
                {
                    var margin = BL.CashAccountCalculator.CalculateMargin(newOrder, symbolInfo);
                    return cashCalc.HasSufficientMarginToOpenOrder(newOrder, margin);
                }
            }
            catch (BL.NotEnoughMoneyException)
            {
                return false;
            }
            catch (Exception ex)
            {
                _context.Builder.Logger.OnError("Failed to calculate margin for new order", ex);
            }
            return false;
        }

        public bool HasEnoughMarginToModifyOrder(BL.IOrderModel oldOrder, BL.IOrderModel newOrder, BO.ISymbolInfo symbolInfo)
        {
            try
            {
                if (marginCalc != null)
                {
                    var calc = _state.GetCalculator(newOrder.Symbol, acc.BalanceCurrency);
                    calc.UpdateMargin(oldOrder, acc);
                    calc.UpdateMargin(newOrder, acc);
                    return marginCalc.HasSufficientMarginToOpenOrder(newOrder, newOrder.Margin - oldOrder.Margin);
                }
                if (cashCalc != null)
                {
                    var oldMargin = BL.CashAccountCalculator.CalculateMargin(oldOrder, symbolInfo);
                    var newMargin = BL.CashAccountCalculator.CalculateMargin(newOrder, symbolInfo);
                    return cashCalc.HasSufficientMarginToOpenOrder(newOrder, newMargin - oldMargin);
                }
            }
            catch (BL.NotEnoughMoneyException)
            {
                return false;
            }
            catch (Exception ex)
            {
                _context.Builder.Logger.OnError($"Failed to calculate margin for order #{oldOrder.OrderId}", ex);
            }
            return false;
        }

        #endregion IAccountCalculator implementation
    }
}
