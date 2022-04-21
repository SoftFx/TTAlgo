using TickTrader.Algo.Domain;

namespace TestEnviroment
{
    public abstract class LoadSymbolBase
    {
        protected static SymbolInfoStorage _storage;
        protected static ISymbolInfoWithRate _symbol;

        public LoadSymbolBase()
        {
            _storage = new SymbolInfoStorage();
        }

        protected static void LoadSymbol(string symbol = "EURUSD")
        {
            _symbol = _storage.Symbols[symbol];

            ResetSymbolRate();
        }

        protected static void UpdateSymbolRate()
        {
            _symbol.BuildNewQuote();
        }

        protected static void ResetSymbolRate()
        {
            _symbol.UpdateRate(null);
        }

        protected static void ZeroSymbolRate()
        {
            _symbol.BuildZeroQuote();
        }

        protected static void BidSideRate()
        {
            _symbol.BuildOneSideBidQuote();
        }

        protected static void AskSideRate()
        {
            _symbol.BuildOneSideAskQuote();
        }
    }
}
