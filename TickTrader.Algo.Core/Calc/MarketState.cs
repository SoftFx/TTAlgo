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
    public abstract class MarketStateBase
    {
        private Dictionary<Tuple<string, string>, OrderCalculator> _orderCalculators = new Dictionary<Tuple<string, string>, OrderCalculator>();
        private Dictionary<string, CurrencyEntity> _currenciesByName = new Dictionary<string, CurrencyEntity>();

        public MarketStateBase()
        {
            Conversion = new ConversionManager(this);
        }

        public IEnumerable<SymbolAccessor> Symbols { get; private set; }
        public IEnumerable<CurrencyEntity> Currencies { get; private set; }

        public ConversionManager Conversion { get; }

        public void Init(IEnumerable<SymbolAccessor> symbolList, IEnumerable<CurrencyEntity> currencyList)
        {
            Currencies = currencyList.ToList();
            _currenciesByName = currencyList.ToDictionary(c => c.Name);
            CurrenciesChanged?.Invoke();

            Symbols = symbolList.ToList();
            InitNodes();
            SymbolsChanged?.Invoke();

            Conversion.Init();
        }

        protected abstract void InitNodes();

        internal abstract SymbolMarketNode GetSymbolNodeInternal(string smb);

        public CurrencyEntity GetCurrencyOrThrow(string name)
        {
            var result = _currenciesByName.GetOrDefault(name);
            if (result == null)
                throw new BusinessLogic.MarketConfigurationException("Currency Not Found: " + name);
            return result;
        }

        public event Action SymbolsChanged;
        public event Action CurrenciesChanged;

        internal OrderCalculator GetCalculator(string symbol, string balanceCurrency)
        {
            var key = Tuple.Create(symbol, balanceCurrency);

            OrderCalculator calculator;
            if (!_orderCalculators.TryGetValue(key, out calculator))
            {
                var tracker = GetSymbolNodeInternal(symbol);
                if (tracker != null)
                {
                    calculator = new OrderCalculator(tracker, Conversion, balanceCurrency);
                    _orderCalculators.Add(key, calculator);
                }
            }
            return calculator;
        }

        internal string GetSnapshotString()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Market snapshot:");
            sb.AppendLine($"{nameof(Currencies)}");
            if (Currencies != null)
            {
                foreach (var c in Currencies)
                {
                    sb.Append($"{nameof(c.Name)} = {c.Name}, ");
                    sb.Append($"{nameof(c.IsNull)} = {c.IsNull}, ");
                    sb.Append($"{nameof(c.Digits)} = {c.Digits}, ");
                    sb.Append($"{nameof(c.SortOrder)} = {c.SortOrder}, ");
                    sb.AppendLine();
                }
            }
            else
            {
                sb.AppendLine("Empty");
            }
            sb.AppendLine($"{nameof(Symbols)}");
            if (Symbols != null)
            {
                foreach (var s in Symbols)
                {
                    sb.Append($"{nameof(s.Name)} = {s.Name}, ");
                    sb.Append($"{nameof(s.IsNull)} = {s.IsNull}, ");
                    sb.Append($"{nameof(s.IsTradeAllowed)} = {s.IsTradeAllowed}, ");
                    sb.Append($"{nameof(s.Bid)} = {s.Bid}, ");
                    sb.Append($"{nameof(s.Ask)} = {s.Ask}, ");
                    sb.Append($"{nameof(s.Digits)} = {s.Digits}, ");
                    sb.Append($"{nameof(s.ContractSize)} = {s.ContractSize}, ");
                    sb.Append($"{nameof(s.MinTradeVolume)} = {s.MinTradeVolume}, ");
                    sb.Append($"{nameof(s.MaxTradeVolume)} = {s.MaxTradeVolume}, ");
                    sb.Append($"{nameof(s.TradeVolumeStep)} = {s.TradeVolumeStep}, ");
                    sb.Append($"{nameof(s.BaseCurrency)} = {s.BaseCurrency}, ");
                    sb.Append($"{nameof(s.CounterCurrency)} = {s.CounterCurrency}, ");
                    sb.Append($"{nameof(s.SortOrder)} = {s.SortOrder}, ");
                    sb.Append($"{nameof(s.GroupSortOrder)} = {s.GroupSortOrder}, ");
                    sb.Append($"{nameof(s.Commission)} = {s.Commission}, ");
                    sb.Append($"{nameof(s.LimitsCommission)} = {s.LimitsCommission}, ");
                    sb.Append($"{nameof(s.CommissionType)} = {s.CommissionType}, ");
                    sb.Append($"{nameof(s.CommissionChargeType)} = {s.CommissionChargeType}, ");
                    sb.Append($"{nameof(s.CommissionChargeMethod)} = {s.CommissionChargeMethod}, ");
                    sb.Append($"{nameof(s.MarginFactorFractional)} = {s.MarginFactorFractional}, ");
                    sb.Append($"{nameof(s.MarginMode)} = {s.MarginMode}, ");
                    sb.Append($"{nameof(s.MarginHedged)} = {s.MarginHedged}, ");
                    sb.Append($"{nameof(s.StopOrderMarginReduction)} = {s.StopOrderMarginReduction}, ");
                    sb.Append($"{nameof(s.HiddenLimitOrderMarginReduction)} = {s.HiddenLimitOrderMarginReduction}, ");
                    sb.Append($"{nameof(s.SwapEnabled)} = {s.SwapEnabled}, ");
                    sb.Append($"{nameof(s.SwapSizeShort)} = {s.SwapSizeShort}, ");
                    sb.Append($"{nameof(s.SwapSizeLong)} = {s.SwapSizeLong}, ");
                    sb.Append($"{nameof(s.SwapType)} = {s.SwapType}, ");
                    sb.Append($"{nameof(s.TripleSwapDay)} = {s.TripleSwapDay}, ");
                    sb.Append($"{nameof(s.Security)} = {s.Security}, ");
                    sb.AppendLine();
                }
            }
            else
            {
                sb.AppendLine("Empty");
            }
            return sb.ToString();
        }
    }

    public class MarketState : MarketStateBase
    {
        private readonly Dictionary<string, SymbolMarketNode> _smbMap = new Dictionary<string, SymbolMarketNode>();

        public void Update(RateUpdate rate)
        {
            var tracker = GetSymbolNodeOrNull(rate.Symbol);
            tracker.Update(rate);
            //RateChanged?.Invoke(rate);
            //return RateUpdater.Update(rate.Symbol);
        }

        public void Update(IEnumerable<RateUpdate> rates)
        {
            if (rates == null)
                return;

            foreach (RateUpdate rate in rates)
                Update(rate);
        }

        internal SymbolMarketNode GetSymbolNodeOrNull(string symbol)
        {
            return _smbMap.GetOrDefault(symbol);
        }

        protected override void InitNodes()
        {
            _smbMap.Clear();

            foreach (var smb in Symbols)
                _smbMap.Add(smb.Name, new SymbolMarketNode(smb));
        }

        internal override SymbolMarketNode GetSymbolNodeInternal(string smb)
        {
            return GetSymbolNodeOrNull(smb);
        }
    }

    internal class AlgoMarketState : MarketStateBase
    {
        private readonly Dictionary<string, AlgoMarketNode> _smbMap = new Dictionary<string, AlgoMarketNode>();

        public AlgoMarketState()
        {
        }

        public IEnumerable<AlgoMarketNode> Nodes => _smbMap.Values;

        protected override void InitNodes()
        {
            _smbMap.Clear();

            foreach (var smb in Symbols)
                _smbMap.Add(smb.Name, new AlgoMarketNode(smb));
        }

        public AlgoMarketNode GetSymbolNodeOrNull(string symbol)
        {
            return _smbMap.GetOrDefault(symbol);
        }

        internal override SymbolMarketNode GetSymbolNodeInternal(string smb)
        {
            return GetSymbolNodeOrNull(smb);
        }
    }
}
