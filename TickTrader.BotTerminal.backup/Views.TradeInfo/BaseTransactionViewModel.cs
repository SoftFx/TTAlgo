using Machinarium.Var;
using System;
using TickTrader.Algo.Domain;
using TickTrader.BotTerminal.Converters;

namespace TickTrader.BotTerminal
{
    internal abstract class BaseTransactionViewModel : IDisposable
    {
        protected readonly VarContext _varContext = new VarContext();

        protected readonly AmountToLotsConverter<double?> _amountToLots;
        protected readonly PricePrecisionConverter<double?> _symbolPrecision;
        protected readonly PricePrecisionConverter<double> _profitPrecision;

        protected readonly SymbolInfo _symbol;

        protected BaseTransactionViewModel(SymbolInfo symbol, int accountDigits)
        {
            _symbol = symbol;

            _symbolPrecision = new PricePrecisionConverter<double?>(symbol?.Digits ?? 5);
            _profitPrecision = new PricePrecisionConverter<double>(accountDigits);
            _amountToLots = new AmountToLotsConverter<double?>(_symbol?.LotSize ?? 1);

            Modified = _varContext.AddProperty(default(DateTime?));

            Price = _varContext.AddProperty(displayConverter: _symbolPrecision);
            DeviationPrice = _varContext.AddProperty(displayConverter: _symbolPrecision);
            CurrentPrice = _varContext.AddProperty(displayConverter: _symbolPrecision);
            Volume = _varContext.AddProperty(displayConverter: _amountToLots);

            Side = _varContext.AddProperty(default(Algo.Domain.OrderInfo.Types.Side));
            Type = _varContext.AddProperty(default(Algo.Domain.OrderInfo.Types.Type));

            Swap = _varContext.AddProperty(default(double));
            Commission = _varContext.AddProperty(displayConverter: _profitPrecision);
            NetProfit = _varContext.AddProperty(displayConverter: _profitPrecision);

            if (symbol != null) // server misconfiguration can cause unexisting symbols
                symbol.RateUpdated += RateUpdate;
        }

        public abstract string Id { get; }
        public abstract double Profit { get; }

        protected abstract void Update();

        public string Symbol => _symbol?.Name;

        public bool IsPosition => Type?.Value != Algo.Domain.OrderInfo.Types.Type.Position;

        public bool IsBuy => Side.Value == Algo.Domain.OrderInfo.Types.Side.Buy;

        public string SortedNumber => $"{Modified.Value?.ToString("dd.MM.yyyyHH:mm:ss.fff")}-{Id}"; //use on UI for correct sorting

        public Property<DateTime?> Modified { get; }

        public Property<double?> Price { get; }
        public Property<double?> DeviationPrice { get; }
        public Property<double?> CurrentPrice { get; }
        public Property<double?> Volume { get; }

        public Property<Algo.Domain.OrderInfo.Types.Side> Side { get; }
        public Property<Algo.Domain.OrderInfo.Types.Type> Type { get; }

        public Property<double> Swap { get; }
        public Property<double> Commission { get; }
        public Property<double> NetProfit { get; }

        protected void RateUpdate(ISymbolInfo symbols)
        {
            CurrentPrice.Value = IsPosition ? IsBuy ? _symbol?.Ask : _symbol?.Bid : IsBuy ? _symbol?.Bid : _symbol?.Ask;
            DeviationPrice.Value = IsBuy ? CurrentPrice.Value - Price.Value : Price.Value - CurrentPrice.Value;
            NetProfit.Value = Swap.Value + Commission.Value + Profit;
        }

        public void Dispose()
        {
            if (_symbol != null) // server misconfiguration can cause unexisting symbols
                _symbol.RateUpdated -= RateUpdate;
        }
    }
}
