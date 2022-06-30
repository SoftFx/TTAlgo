using Machinarium.Var;
using System;
using TickTrader.Algo.Core.Subscriptions;
using TickTrader.Algo.Domain;
using TickTrader.BotTerminal.Converters;
using TickTrader.BotTerminal.Converters.Machinarium.Converters;

namespace TickTrader.BotTerminal
{
    public enum RateChangeDirections
    {
        Unknown,
        Up,
        Down,
    }

    internal sealed class SymbolViewModel : IDisposable
    {
        private readonly VarContext _varContext = new VarContext();
        private readonly PricePrecisionConverter<double> _symbolPrecision;

        private readonly SymbolInfo _model;
        private readonly IDisposable _subscription;

        public SymbolViewModel(SymbolInfo model, QuoteDistributor distributor)
        {
            _model = model;
            _symbolPrecision = new PricePrecisionConverter<double>(model?.Digits ?? 2);

            _subscription = distributor.AddListener(OnRateUpdate, model.Name);

            Bid = new RateViewModel(_symbolPrecision);
            Ask = new RateViewModel(_symbolPrecision);

            QuoteTime = _varContext.AddProperty(default, new DateTimeToUtc());
        }

        public void Dispose()
        {
            _subscription.Dispose();
        }

        public string SymbolName => _model.Name;

        public Property<DateTime?> QuoteTime { get; private set; }

        public RateViewModel Bid { get; private set; }

        public RateViewModel Ask { get; private set; }

        private void OnRateUpdate(QuoteInfo tick)
        {
            QuoteTime.Value = tick.TimeUtc;

            Bid.RateUpdate(tick.Bid);
            Ask.RateUpdate(tick.Ask);
        }

        internal sealed class RateViewModel
        {
            private readonly VarContext _varContext = new VarContext();

            public Property<double> Rate { get; }

            public Property<RateChangeDirections> Direction { get; }

            internal RateViewModel(IDisplayValueConverter<double> precisionConverter)
            {
                Rate = _varContext.AddProperty(displayConverter: precisionConverter);
                Direction = _varContext.AddProperty(default(RateChangeDirections));
            }

            public void RateUpdate(double rate)
            {
                if (double.IsNaN(rate))
                    Direction.Value = RateChangeDirections.Unknown;
                else if (Rate.Value < rate)
                    Direction.Value = RateChangeDirections.Up;
                else if (Rate.Value > rate)
                    Direction.Value = RateChangeDirections.Down;

                Rate.Value = rate;
            }
        }
    }
}