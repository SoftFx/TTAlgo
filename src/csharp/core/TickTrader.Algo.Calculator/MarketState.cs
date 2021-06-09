using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Calculator
{
    public abstract class MarketStateBase
    {
        private Dictionary<Tuple<string, string>, OrderCalculator> _orderCalculators = new Dictionary<Tuple<string, string>, OrderCalculator>();
        private Dictionary<string, CurrencyInfo> _currenciesByName = new Dictionary<string, CurrencyInfo>();

        public IFeedSubscription Subscriptions { get; set; }

        public MarketStateBase()
        {
            Conversion = new ConversionManager(this);
        }

        public IEnumerable<SymbolInfo> Symbols { get; private set; }

        public IEnumerable<ICurrencyInfo> Currencies { get; private set; }

        internal ConversionManager Conversion { get; }

        public void Init(IEnumerable<SymbolInfo> symbolList, IEnumerable<CurrencyInfo> currencyList)
        {
            Currencies = currencyList?.ToList();
            _currenciesByName = currencyList?.ToDictionary(c => c.Name);
            CurrenciesChanged?.Invoke();

            Symbols = symbolList?.ToList();
            InitNodes();
            SymbolsChanged?.Invoke();

            Conversion.Init();
        }

        protected abstract void InitNodes();

        internal abstract SymbolMarketNode GetSymbolNodeInternal(string smb);

        public ICurrencyInfo GetCurrencyOrThrow(string name)
        {
            var result = _currenciesByName.GetOrDefault(name);
            if (result == null)
                throw new MarketConfigurationException("Currency Not Found: " + name);
            return result;
        }

        public event Action SymbolsChanged;
        public event Action CurrenciesChanged;

        public OrderCalculator GetCalculator(string symbol, IMarginAccountInfo2 account)
        {
            var key = Tuple.Create(symbol, account.BalanceCurrency);

            OrderCalculator calculator;
            if (!_orderCalculators.TryGetValue(key, out calculator))
            {
                var tracker = GetSymbolNodeInternal(symbol);
                if (tracker != null)
                {
                    calculator = new OrderCalculator(tracker, Conversion, account);
                    _orderCalculators.Add(key, calculator);
                }
            }
            return calculator;
        }

        public string GetSnapshotString()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Market snapshot:");
            sb.AppendLine($"{nameof(Currencies)}");
            if (Currencies != null)
            {
                foreach (var c in Currencies)
                {
                    sb.AppendLine(c.ToString());
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
                    sb.AppendLine(s.ToString());
                }
            }
            else
            {
                sb.AppendLine("Empty");
            }
            return sb.ToString();
        }
    }

    //public class MarketState : MarketStateBase
    //{
    //    private readonly Dictionary<string, SymbolMarketNode> _smbMap = new Dictionary<string, SymbolMarketNode>();

    //    public void Update(IRateInfo rate)
    //    {
    //        var tracker = GetSymbolNodeOrNull(rate.Symbol);
    //        tracker.Update(rate);
    //    }

    //    public void Update(IEnumerable<IRateInfo> rates)
    //    {
    //        if (rates == null)
    //            return;

    //        foreach (IRateInfo rate in rates)
    //            Update(rate);
    //    }

    //    internal SymbolMarketNode GetSymbolNodeOrNull(string symbol)
    //    {
    //        return _smbMap.GetOrDefault(symbol);
    //    }

    //    protected override void InitNodes()
    //    {
    //        _smbMap.Clear();

    //        foreach (var smb in Symbols)
    //            _smbMap.Add(smb.Name, new SymbolMarketNode(smb));
    //    }

    //    internal override SymbolMarketNode GetSymbolNodeInternal(string smb)
    //    {
    //        return GetSymbolNodeOrNull(smb);
    //    }
    //}

    public class AlgoMarketState : MarketStateBase
    {
        private readonly Dictionary<string, AlgoMarketNode> _smbMap = new Dictionary<string, AlgoMarketNode>();

        public AlgoMarketState()
        {
        }

        public IEnumerable<AlgoMarketNode> Nodes => _smbMap.Values;

        protected override void InitNodes()
        {
            // We have to leave deleted symbols, MarketNodes contain subscription info
            // In case they will come back after reconnect we need to re-enable their subscription

            foreach (var node in _smbMap.Values)
            {
                node.Update((SymbolInfo)null);
            }

            foreach (var smb in Symbols)

            {
                if (_smbMap.TryGetValue(smb.Name, out var node))
                {
                    node.Update(smb);
                }
                else
                {
                    _smbMap.Add(smb.Name, new AlgoMarketNode(smb));
                }
            }
        }

        public AlgoMarketNode GetSymbolNodeOrNull(string symbol)
        {
            if (_smbMap.TryGetValue(symbol, out var node))
            {
                return node.IsShadowCopy ? null : node;
            }
            return null;
        }

        internal override SymbolMarketNode GetSymbolNodeInternal(string smb)
        {
            return GetSymbolNodeOrNull(smb);
        }

        public void UpdateRate(IRateInfo newRate, out AlgoMarketNode node)
        {
            node = GetSymbolNodeOrNull(newRate.Symbol);
            if (node != null)
            {
                node.SymbolInfo.UpdateRate(newRate.LastQuote);
                //node.Update(newRate);
            }
        }
    }
}
