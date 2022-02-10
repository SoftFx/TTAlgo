using System.Collections.Generic;
using System.Linq;
using TestEnviroment;
using TickTrader.Algo.Calculator.AlgoMarket;
using TickTrader.Algo.Calculator.Conversions;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Calculator.Tests
{
    public abstract class AlgoMarketEmulator
    {
        protected readonly SymbolInfoStorage _symbolStorage;
        protected readonly CurrencyInfoStorage _currencyStorage;

        protected ConversionManager _conversion;
        protected AlgoMarketState _algoMarket;
        protected AccountEmulator _account;


        internal Dictionary<string, ISymbolInfoWithRate> Symbol => _symbolStorage.Symbols;

        internal Dictionary<string, CurrencyInfo> Currency => _currencyStorage.Currency;


        public AlgoMarketEmulator()
        {
            _symbolStorage = new SymbolInfoStorage();
            _currencyStorage = new CurrencyInfoStorage();
        }

        protected void CreateAlgoMarket()
        {
            _symbolStorage.AllSymbolsRateUpdate();

            _algoMarket = new AlgoMarketState();
            _conversion = new ConversionManager(_algoMarket);

            _account = new AccountEmulator(this)
            {
                Leverage = 1,
            };
        }

        protected void InitAlgoMarket(params string[] load)
        {
            _algoMarket.Init(_account, load.Where(u => Symbol.ContainsKey(u)).Select(u => Symbol[u]), _currencyStorage.Currency?.Values);
            _algoMarket.StartCalculators();
            _conversion.Init(_account.BalanceCurrencyName);
        }
    }


    public sealed class AccountEmulator : IMarketStateAccountInfo
    {
        private readonly AlgoMarketEmulator _market;
        private string _balanceCurrencyName;

        public CurrencyInfo BalanceCurrency { get; set; }

        public int Leverage { get; set; }


        public string BalanceCurrencyName
        {
            get => _balanceCurrencyName;

            set
            {
                _balanceCurrencyName = value;

                BalanceCurrency = _market.Currency[value];
            }
        }


        internal AccountEmulator(AlgoMarketEmulator market)
        {
            _market = market;
        }
    }
}
