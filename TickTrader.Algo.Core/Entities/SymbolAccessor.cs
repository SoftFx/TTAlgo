using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.Core
{
    internal class SymbolAccessor : Api.Symbol
    {
        private SymbolEntity entity;
        private IPluginSubscriptionHandler handler;

        public SymbolAccessor(SymbolEntity entity, IPluginSubscriptionHandler handler, Dictionary<string, CurrencyEntity> currencies)
        {
            this.entity = entity;
            this.handler = handler;

            this.Point = System.Math.Pow(10, -entity.Digits);

            this.Ask = double.NaN;
            this.Bid = double.NaN;
            LastQuote = Null.Quote;
            BaseCurrencyInfo = currencies.ContainsKey(BaseCurrency) ? currencies[BaseCurrency] : Null.Currency; 
            CounterCurrencyInfo = currencies.ContainsKey(CounterCurrency) ? currencies[CounterCurrency] : Null.Currency; 
        }

        public string Name { get { return entity.Name; } }
        public int Digits { get { return entity.Digits; } }
        public double ContractSize { get { return entity.LotSize; } }
        public double MaxTradeVolume { get { return entity.MaxAmount; } }
        public double MinTradeVolume { get { return entity.MinAmount; } }
        public double TradeVolumeStep { get { return entity.AmountStep; } }
        public string BaseCurrency { get { return entity.BaseCurrencyCode; } }
        public Currency BaseCurrencyInfo { get; private set; }
        public string CounterCurrency { get { return entity.CounterCurrencyCode; } }
        public Currency CounterCurrencyInfo { get; private set; }
        public bool IsNull { get { return false; } }
        public double Point { get; private set; }
        public double Bid { get; set; }
        public double Ask { get; set; }
        public Api.Quote LastQuote {get; set;}
        public bool IsTradeAllowed { get { return entity.IsTradeAllowed; } }
        public double Commission { get { return entity.Commission; } }
        public double LimitsCommission { get { return entity.LimitsCommission; } }
        public CommissionChargeMethod CommissionChargeMethod { get { return entity.CommissionChargeMethod; } }
        public CommissionChargeType CommissionChargeType { get { return entity.CommissionChargeType; } }
        public CommissionType CommissionType { get { return entity.CommissionType; } }

        public void Subscribe(int depth = 1)
        {
            handler.Subscribe(Name, depth);
        }

        public void Unsubscribe()
        {
            handler.Unsubscribe(Name);
        }

        public void UpdateRate(Api.Quote quote)
        {
            Ask = quote.Ask;
            Bid = quote.Bid;
            LastQuote = quote;
        }
    }

    public class NullSymbol : Api.Symbol
    {
        public NullSymbol(string code)
        {
            this.Name = code;
        }

        public string Name { get; private set; }
        public int Digits { get { return -1; } }
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

        public void Subscribe(int depth = 1)
        {
        }

        public void Unsubscribe()
        {
        }
    }
}
