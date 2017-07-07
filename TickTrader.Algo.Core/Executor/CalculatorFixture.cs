﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BL = TickTrader.BusinessLogic;

namespace TickTrader.Algo.Core
{
    internal class CalculatorFixture
    {
        private BL.MarketState _state;
        private IFixtureContext _context;
        private MarginCalcAdapter marginCalc;
        private BL.CashAccountCalculator cashCalc;

        public CalculatorFixture(IFixtureContext context)
        {
            _context = context;
        }

        public void Start()
        {
            _state = new BL.MarketState(BL.NettingCalculationTypes.OneByOne);
            _state.Set(_context.Builder.Symbols);
            _state.Set(_context.Builder.Currencies);

            var acc = _context.Builder.Account;
            if (acc.Type == Api.AccountTypes.Gross || acc.Type == Api.AccountTypes.Net)
                marginCalc = new MarginCalcAdapter(acc, _state);
            else
                cashCalc = new BL.CashAccountCalculator(acc, _state);
        }

        internal void UpdateRate(QuoteEntity entity)
        {
            _state.Update(entity);
        }

        public void Stop()
        {
            _state = null;
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
            public MarginCalcAdapter(AccountEntity accEntity, BL.MarketState market) : base(accEntity, market)
            {
            }

            protected override void OnUpdated()
            {
                var accEntity = (AccountEntity)Info;

                accEntity.Equity = (double)Equity;
                accEntity.Profit = (double)Profit;
                accEntity.Commision = (double)Commission;
                accEntity.MarginLevel = (double)MarginLevel;
                accEntity.Margin = (double)Margin;
            }
        }
    }
}
