﻿using Machinarium.Var;
using System;
using TickTrader.Algo.Common;
using TickTrader.Algo.Common.Model;
using TickTrader.Algo.Core;
using TickTrader.BotTerminal.Converters;

namespace TickTrader.BotTerminal
{
    internal abstract class BaseTransactionViewModel : IDisposable
    {
        protected readonly VarContext _varContext = new VarContext();

        protected readonly AmountToLotsConverter<double?> _amountToLots;
        protected readonly PricePrecisionConverter<double?> _symbolPrecision;
        protected readonly PricePrecisionConverter<decimal?> _profitPrecision;

        protected readonly SymbolModel _symbol;

        protected BaseTransactionViewModel(SymbolModel symbol, int accountDigits)
        {
            _symbol = symbol;

            _symbolPrecision = new PricePrecisionConverter<double?>(symbol?.PriceDigits ?? 5);
            _profitPrecision = new PricePrecisionConverter<decimal?>(accountDigits);
            _amountToLots = new AmountToLotsConverter<double?>(_symbol.LotSize);

            Modified = _varContext.AddProperty(default(DateTime?));

            Price = _varContext.AddProperty(displayConverter: _symbolPrecision);
            DeviationPrice = _varContext.AddProperty(displayConverter: _symbolPrecision);
            Volume = _varContext.AddProperty(displayConverter: _amountToLots);

            Side = _varContext.AddProperty(default(Algo.Domain.OrderInfo.Types.Side));
            Type = _varContext.AddProperty(default(Algo.Domain.OrderInfo.Types.Type));

            Swap = _varContext.AddProperty(default(decimal?));
            Commission = _varContext.AddProperty(displayConverter: _profitPrecision);
            NetProfit = _varContext.AddProperty(displayConverter: _profitPrecision);

            if (symbol != null) // server misconfiguration can cause unexisting symbols
                symbol.RateUpdated += RateUpdate;
        }

        public abstract string Id { get; }
        public abstract decimal Profit { get; }

        protected abstract void Update();

        public string Symbol => _symbol.Name;

        public bool IsPosition => Type.Value != Algo.Domain.OrderInfo.Types.Type.Position;

        public bool IsBuy => Side.Value == Algo.Domain.OrderInfo.Types.Side.Buy;

        public string SortedNumber => $"{Modified.Value?.ToString("dd.MM.yyyyHH:mm:ss.fff")}-{Id}"; //use on UI for correct sorting

        public RateDirectionTracker CurrentPrice => IsPosition ? IsBuy ? _symbol?.AskTracker : _symbol?.BidTracker :
                                                                 IsBuy ? _symbol?.BidTracker : _symbol?.AskTracker;

        public Property<DateTime?> Modified { get; }

        public Property<double?> Price { get; }
        public Property<double?> DeviationPrice { get; }
        public Property<double?> Volume { get; }

        public Property<Algo.Domain.OrderInfo.Types.Side> Side { get; }
        public Property<Algo.Domain.OrderInfo.Types.Type> Type { get; }

        public Property<decimal?> Swap { get; }
        public Property<decimal?> Commission { get; }
        public Property<decimal?> NetProfit { get; }

        private void RateUpdate(ISymbolInfo2 symbols)
        {
            DeviationPrice.Value = IsBuy ? CurrentPrice?.Rate - Price.Value : Price.Value - CurrentPrice?.Rate;
            NetProfit.Value = Swap.Value + Commission.Value + Profit;
        }

        public void Dispose()
        {
            if (_symbol != null) // server misconfiguration can cause unexisting symbols
                _symbol.RateUpdated -= RateUpdate;
        }
    }
}