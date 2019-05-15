using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;
using TickTrader.Algo.Core.Calc.Conversion;
using TickTrader.Algo.Core.Lib;

namespace TickTrader.Algo.Core.Calc
{
    public class MarketState
    {
        private Dictionary<string, SymbolMarketInfo> _smbMap = new Dictionary<string, SymbolMarketInfo>();
        private Dictionary<Tuple<string, string>, OrderCalculator> _orderCalculators = new Dictionary<Tuple<string, string>, OrderCalculator>();
        private Dictionary<string, CurrencyEntity> _currenciesByName = new Dictionary<string, CurrencyEntity>();

        public MarketState()
        {
            Conversion = new ConversionManager(this);
        }

        public IEnumerable<SymbolAccessor> Symbols { get; private set; }
        public IEnumerable<CurrencyEntity> Currencies { get; private set; }

        public ConversionManager Conversion { get; }

        internal IEnumerable<SymbolMarketInfo> Trackers => _smbMap.Values;

        public void Update(IEnumerable<RateUpdate> rates)
        {
            if (rates == null)
                return;

            foreach (RateUpdate rate in rates)
                Update(rate);
        }

        public void Update(RateUpdate rate)
        {
            var tracker = GetSymbolNodeOrNull(rate.Symbol);
            tracker.Update(rate);
            RateChanged?.Invoke(rate);
            //return RateUpdater.Update(rate.Symbol);
        }

        public void Init(IEnumerable<SymbolAccessor> symbolList, IEnumerable<CurrencyEntity> currencyList)
        {
            Currencies = currencyList.ToList();
            _currenciesByName = currencyList.ToDictionary(c => c.Name);
            CurrenciesChanged?.Invoke();

            Symbols = symbolList.ToList();
            _smbMap = symbolList.Select(s => new SymbolMarketInfo(s)).ToDictionary(t => t.SymbolInfo.Name);
            SymbolsChanged?.Invoke();

            Conversion.Init();
        }

        public CurrencyEntity GetCurrencyOrThrow(string name)
        {
            var result = _currenciesByName.GetOrDefault(name);
            if (result == null)
                throw new BusinessLogic.MarketConfigurationException("Currency Not Found: " + name);
            return result;
        }

        public event Action SymbolsChanged;
        public event Action CurrenciesChanged;
        public event Action<RateUpdate> RateChanged;

        internal SymbolMarketInfo GetSymbolNodeOrNull(string symbol)
        {
            return _smbMap.GetOrDefault(symbol);
        }

        internal OrderCalculator GetCalculator(string symbol, string balanceCurrency)
        {
            var key = Tuple.Create(symbol, balanceCurrency);

            OrderCalculator calculator;
            if (!_orderCalculators.TryGetValue(key, out calculator))
            {
                var tracker = GetSymbolNodeOrNull(symbol);
                if (tracker != null)
                {
                    calculator = new OrderCalculator(tracker, Conversion, balanceCurrency);
                    _orderCalculators.Add(key, calculator);
                }
            }
            return calculator;
        }
    }
}
