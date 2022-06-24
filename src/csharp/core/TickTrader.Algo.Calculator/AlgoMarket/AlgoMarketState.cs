using System.Collections.Generic;
using System.Linq;
using System.Text;
using TickTrader.Algo.Calculator.Conversions;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Core.Lib.Extensions;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Domain.CalculatorInterfaces;

namespace TickTrader.Algo.Calculator.AlgoMarket
{
    public interface IMarketStateAccountInfo
    {
        CurrencyInfo BalanceCurrency { get; }

        int Leverage { get; }
    }

    public class AlgoMarketState
    {
        private readonly Dictionary<string, SymbolMarketNode> _marketNodes = new Dictionary<string, SymbolMarketNode>();
        private readonly ConversionManager _conversionManager;

        public IMarketStateAccountInfo Account { get; private set; }

        public IEnumerable<ISymbolInfoWithRate> Symbols { get; private set; }

        public IEnumerable<ICurrencyInfo> Currencies { get; private set; }


        public Dictionary<string, ISideNode> Bid { get; }

        public Dictionary<string, ISideNode> Ask { get; }


        public AlgoMarketState()
        {
            _conversionManager = new ConversionManager(this);

            Bid = new Dictionary<string, ISideNode>();
            Ask = new Dictionary<string, ISideNode>();
        }

        public void Init(IMarketStateAccountInfo account, IEnumerable<ISymbolInfoWithRate> symbolList, IEnumerable<CurrencyInfo> currencyList)
        {
            Account = account;
            Symbols = symbolList;
            Currencies = currencyList;

            InitNodes();
        }

        private void InitNodes()
        {
            // We have to leave deleted symbols, MarketNodes contain subscription info
            // In case they will come back after reconnect we need to re-enable their subscription
            _marketNodes.Values.ForEach(u => u.Update(null));

            foreach (var smb in Symbols)
            {
                if (_marketNodes.TryGetValue(smb.Name, out var node))
                    node.Update(smb);
                else
                    AddNewMarketNode(smb);
            }
        }

        public void StartCalculators()
        {
            if (Account.BalanceCurrency != null)
            {
                _conversionManager.Init(Account.BalanceCurrency.Name);
                _marketNodes.Values.ForEach(u => u.InitCalculators(_conversionManager));
            }
        }

        public ISymbolCalculator GetCalculator(ISymbolInfoWithRate symbol)
        {
            if (!_marketNodes.TryGetValue(symbol.Name, out var calculator))
            {
                calculator = AddNewMarketNode(symbol);
                calculator.InitCalculators(_conversionManager);
            }

            return calculator;
        }

        private SymbolMarketNode AddNewMarketNode(ISymbolInfoWithRate symbol)
        {
            var node = new SymbolMarketNode(Account, symbol);

            _marketNodes[symbol.Name] = node;
            // Lasts symbol have same currencies pair, which breaks calculator logic
            // Also if there are multiple symbols for same currency pair we should use first
            if (!symbol.Name.EndsWith("_L") && !Bid.ContainsKey(symbol.TradePair))
            {
                Bid[symbol.TradePair] = node.Bid;
                Ask[symbol.TradePair] = node.Ask;
            }

            return node;
        }

        public SymbolMarketNode GetSymbolNodeOrNull(string symbol)
        {
            if (_marketNodes.TryGetValue(symbol, out var node))
                return node.IsShadowCopy ? null : node;

            return null;
        }

        public SymbolMarketNode GetUpdateNode(IRateInfo newRate)
        {
            var node = GetSymbolNodeOrNull(newRate.Symbol);
            node?.SymbolInfo.UpdateRate(newRate.LastQuote);
            return node;
        }

        public void ClearUserSubscriptions()
        {
            foreach (var node in _marketNodes.Values)
            {
                node.UserSubscriptionInfo = null;
            }
        }

        public string GetSnapshotString()
        {
            var sb = new StringBuilder(1 << 10);

            sb.AppendLine($"Market snapshot:")
              .AppendList(Currencies.ToList(), nameof(Currencies))
              .AppendList(Symbols.ToList(), nameof(Symbols));

            return sb.ToString();
        }
    }
}
