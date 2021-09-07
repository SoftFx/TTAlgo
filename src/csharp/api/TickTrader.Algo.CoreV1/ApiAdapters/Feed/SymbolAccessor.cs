using System;
using TickTrader.Algo.Api;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.CoreV1
{
    public sealed class SymbolAccessor : BaseSymbolAccessor<SymbolInfo>, Symbol
    {
        private readonly CurrenciesCollection _currencies;
        private readonly FeedProvider _feed;

        internal SymbolAccessor(Domain.SymbolInfo info, FeedProvider feed, CurrenciesCollection currencies) : base(info)
        {
            _feed = feed;
            _currencies = currencies;
        }

        string Symbol.Name => Info.Name;

        int Symbol.Digits => Info.Digits;

        double Symbol.Point => Math.Pow(10, -Info.Digits);

        double Symbol.ContractSize => Info.LotSize;

        double Symbol.MaxTradeVolume => Info.MaxTradeVolume;

        double Symbol.MinTradeVolume => Info.MinTradeVolume;

        double Symbol.TradeVolumeStep => Info.TradeVolumeStep;

        bool Symbol.IsTradeAllowed => Info.TradeAllowed;

        bool Symbol.IsNull => IsNull;

        string Symbol.BaseCurrency => Info.BaseCurrency;

        Currency Symbol.BaseCurrencyInfo => _currencies.GetOrNull(Info.BaseCurrency) ?? Null.Currency;

        string Symbol.CounterCurrency => Info.CounterCurrency;

        Currency Symbol.CounterCurrencyInfo => _currencies.GetOrNull(Info.CounterCurrency) ?? Null.Currency;

        double Symbol.Bid => Info.Bid;

        double Symbol.Ask => Info.Ask;

        Quote Symbol.LastQuote => Info.LastQuote == null ? Null.Quote : new QuoteEntity((QuoteInfo)Info.LastQuote);

        double Symbol.Commission => Info.Commission.Commission;

        double Symbol.LimitsCommission => Info.Commission.LimitsCommission;

        CommissionChargeMethod Symbol.CommissionChargeMethod => CommissionChargeMethod.OneWay;

        CommissionChargeType Symbol.CommissionChargeType => CommissionChargeType.PerLot;

        CommissionType Symbol.CommissionType => Info.Commission.ValueType.ToApiEnum();

        double Symbol.HedgingFactor => Info.Margin.Hedged;

        double Symbol.Slippage => Info.Slippage.DefaultValue ?? 0;

        SlippageType Symbol.SlippageType => Info.Slippage.Type.ToApiEnum();

        void Symbol.Subscribe(int depth)
        {
            _feed.CustomCommds.Subscribe(Info.Name, depth);
        }

        void Symbol.Unsubscribe()
        {
            _feed.CustomCommds.Unsubscribe(Info.Name);
        }
    }

    public class NullSymbol : Api.Symbol
    {
        public NullSymbol() : this("") { }

        public NullSymbol(string code)
        {
            Name = code;
        }

        public string Name { get; }
        public int Digits => 3;
        public double ContractSize => double.NaN;
        public double MaxTradeVolume => double.NaN;
        public double MinTradeVolume => double.NaN;
        public double TradeVolumeStep => double.NaN;
        public string BaseCurrency => string.Empty;
        public Currency BaseCurrencyInfo => Null.Currency;
        public string CounterCurrency => string.Empty;
        public Currency CounterCurrencyInfo => Null.Currency;
        public bool IsNull => true;
        public double Point => double.NaN;
        public double Bid => double.NaN;
        public double Ask => double.NaN;
        public bool IsTradeAllowed => false;
        public Api.Quote LastQuote => Null.Quote;
        public double Commission => double.NaN;
        public double LimitsCommission => double.NaN;
        public CommissionChargeMethod CommissionChargeMethod => CommissionChargeMethod.OneWay;
        public CommissionChargeType CommissionChargeType => CommissionChargeType.PerTrade;
        public CommissionType CommissionType => CommissionType.Percent;
        public double HedgingFactor => double.NaN;
        public double Slippage => double.NaN;
        public SlippageType SlippageType => SlippageType.Percent;

        public void Subscribe(int depth = 1) { }

        public void Unsubscribe() { }
    }
}
