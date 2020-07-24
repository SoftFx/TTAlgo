using System;
using System.Globalization;
using TickTrader.Algo.Api;
using TickTrader.Algo.Api.Math;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Core
{
    public class SymbolAccessor : Api.Symbol, ISymbolInfo
    {
        private Domain.SymbolInfo _info;
        private FeedProvider feed;

        public event Action<ISymbolInfo> RateUpdated;

        internal SymbolAccessor(Domain.SymbolInfo entity, FeedProvider feed, CurrenciesCollection currencies)
        {
            this._info = entity;
            this.feed = feed;

            Ask = double.NaN;
            Bid = double.NaN;
            LastQuote = Null.Quote;

            Update(entity, currencies);
        }

        public string Name => _info.Name;
        public int Digits => _info.Digits;
        public double ContractSize => _info.LotSize;
        public double MaxTradeVolume => _info.MaxTradeVolume;
        public double MinTradeVolume => _info.MinTradeVolume;
        public double TradeVolumeStep => _info.TradeVolumeStep;
        public string BaseCurrency => _info.BaseCurrency;
        public Currency BaseCurrencyInfo { get; private set; }
        public string CounterCurrency => _info.CounterCurrency;
        public Currency CounterCurrencyInfo { get; private set; }
        public bool IsNull { get; private set; }
        public double Point { get; private set; }
        public double Bid { get; set; }
        public double Ask { get; set; }
        public Api.Quote LastQuote { get; set; }
        public bool IsTradeAllowed => _info.TradeAllowed;
        public double Commission => _info.Commission.Commission;
        public double LimitsCommission => _info.Commission.LimitsCommission;
        public CommissionChargeMethod CommissionChargeMethod => CommissionChargeMethod.OneWay;
        public CommissionChargeType CommissionChargeType => CommissionChargeType.PerLot;
        public CommissionType CommissionType { get; private set; }
        public double HedgingFactor => _info.Margin.Hedged;
        public NumberFormatInfo PriceFormat { get; private set; }
        public int AmountDigits { get; private set; }
        public double? Slippage => _info.Slippage.DefaultValue;

        public Domain.CommissonInfo.Types.ValueType CommissionValueType => _info.Commission.ValueType;
        public double MarginFactorFractional => _info.Margin.Factor;
        public double StopOrderMarginReduction => _info.Margin.StopOrderReduction ?? 1;
        public double HiddenLimitOrderMarginReduction => _info.Margin.HiddenLimitOrderReduction ?? 1;
        public double MarginHedged => _info.Margin.Hedged;
        public Domain.MarginInfo.Types.CalculationMode MarginMode => _info.Margin.Mode;
        public bool SwapEnabled => _info.Swap.Enabled;
        public double SwapSizeLong => _info.Swap.SizeLong ?? 0;
        public double SwapSizeShort => _info.Swap.SizeShort ?? 0;
        public string Security => _info.Security;
        public int SortOrder => _info.SortOrder;
        public int GroupSortOrder => _info.GroupSortOrder;
        public Domain.SwapInfo.Types.Type SwapType => _info.Swap.Type;
        public int TripleSwapDay => _info.Swap.TripleSwapDay;
        public double DefaultSlippage => _info.Slippage.DefaultValue ?? 0;

        #region Aliases

        public string MarginCurrency => BaseCurrency;
        public string ProfitCurrency => CounterCurrency;
        public Currency MarginCurrencyInfo => BaseCurrencyInfo;
        public Currency ProfitCurrencyInfo => CounterCurrencyInfo;
        public int Precision => Digits;
        public double ContractSizeFractional => _info.LotSize;

        IQuoteInfo ISymbolInfo.LastQuote => (IQuoteInfo)LastQuote;

        public int ProfitDigits => CounterCurrencyInfo.Digits;

        #endregion Aliases

        public void Subscribe(int depth = 1)
        {
            feed.CustomCommds.Subscribe(Name, depth);
        }

        public void Unsubscribe()
        {
            feed.CustomCommds.Unsubscribe(Name);
        }

        public void UpdateRate(Api.Quote quote)
        {
            Ask = quote.Ask;
            Bid = quote.Bid;
            LastQuote = quote;

            RateUpdated?.Invoke(this);
        }

        public void Update(Domain.SymbolInfo info, CurrenciesCollection currencies)
        {
            if (info == null)
            {
                IsNull = true;
            }
            else
            {
                IsNull = false;
                _info = info;

                if (info.Commission != null)
                {
                    CommissionType = info.Commission.ValueType.ToApiEnum();
                }

                Point = System.Math.Pow(10, -info.Digits);
                BaseCurrencyInfo = currencies.GetOrDefault(info.BaseCurrency) ?? Null.Currency;
                CounterCurrencyInfo = currencies.GetOrDefault(info.CounterCurrency) ?? Null.Currency;

                PriceFormat = FormatExtentions.CreateTradeFormatInfo(info.Digits);
                AmountDigits = (info.TradeVolumeStep * info.LotSize).Digits();
            }
        }

        public void Update(ISymbolInfo info)
        {
            if (info == null)
            {
                IsNull = true;
            }
            else
            {
                //IsNull = false;
                //_info = info;

                //if (info.Commission != null)
                //{
                //    CommissionType = info.Commission.ValueType.ToApiEnum();
                //}

                //Point = System.Math.Pow(10, -info.Digits);
                ////BaseCurrencyInfo = currencies.GetOrDefault(info.BaseCurrency) ?? Null.Currency;
                ////CounterCurrencyInfo = currencies.GetOrDefault(info.CounterCurrency) ?? Null.Currency;

                //PriceFormat = FormatExtentions.CreateTradeFormatInfo(info.Digits);
                //AmountDigits = (info.TradeVolumeStep * info.LotSize).Digits();
            }
        }

        public void UpdateRate(IQuoteInfo quote) => UpdateRate((Quote)quote);
    }

    public class NullSymbol : Api.Symbol
    {
        public NullSymbol(string code)
        {
            this.Name = code;
        }

        public string Name { get; private set; }
        public int Digits { get { return 3; } }
        public double ContractSize { get { return double.NaN; } }
        public double MaxTradeVolume { get { return double.NaN; } }
        public double MinTradeVolume { get { return double.NaN; } }
        public double TradeVolumeStep { get { return double.NaN; } }
        public string BaseCurrency { get { return ""; } }
        public Currency BaseCurrencyInfo => Null.Currency;
        public string CounterCurrency { get { return ""; } }
        public Currency CounterCurrencyInfo => Null.Currency;
        public bool IsNull { get { return true; } }
        public double Point { get { return double.NaN; } }
        public double Bid { get { return double.NaN; } }
        public double Ask { get { return double.NaN; } }
        public bool IsTradeAllowed { get { return false; } }
        public Api.Quote LastQuote { get { return Null.Quote; } }
        public double Commission { get { return double.NaN; } }
        public double LimitsCommission { get { return double.NaN; } }
        public CommissionChargeMethod CommissionChargeMethod { get { return CommissionChargeMethod.OneWay; } }
        public CommissionChargeType CommissionChargeType { get { return CommissionChargeType.PerTrade; } }
        public CommissionType CommissionType { get { return CommissionType.Percent; } }
        public double HedgingFactor => double.NaN;
        public double? Slippage => null;

        public void Subscribe(int depth = 1)
        {
        }

        public void Unsubscribe()
        {
        }
    }
}
