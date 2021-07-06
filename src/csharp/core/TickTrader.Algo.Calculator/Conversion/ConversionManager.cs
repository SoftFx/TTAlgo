using System.Collections.Generic;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Calculator
{
    internal sealed class ConversionManager
    {
        private readonly MarketStateBase _market;

        private readonly Dictionary<string, ISideNode> Bid = new Dictionary<string, ISideNode>();
        private readonly Dictionary<string, ISideNode> Ask = new Dictionary<string, ISideNode>();

        private readonly Dictionary<(string, string), IConversionFormula> _marginConversions = new Dictionary<(string, string), IConversionFormula>();
        private readonly Dictionary<(string, string), IConversionFormula> _posProfitConversions = new Dictionary<(string, string), IConversionFormula>();
        private readonly Dictionary<(string, string), IConversionFormula> _negProfitConversions = new Dictionary<(string, string), IConversionFormula>();

        public ConversionManager(MarketStateBase market)
        {
            _market = market;
        }

        internal void Init()
        {
            Ask.Clear();
            Bid.Clear();

            _marginConversions.Clear();
            _posProfitConversions.Clear();
            _negProfitConversions.Clear();

            FillConversionSet(_market.Symbols);
        }

        private void FillConversionSet(IEnumerable<ISymbolInfo> symbols)
        {
            foreach (var symbol in symbols)
                if (!Exist(symbol.NodeKey))
                {
                    Bid[symbol.NodeKey] = new BidSideNode(symbol);
                    Ask[symbol.NodeKey] = new AskSideNode(symbol);
                }
        }

        internal IConversionFormula GetMarginFormula(ISymbolInfo tracker, string depositCurr)
        {
            var key = (tracker.Name, depositCurr);

            if (!_marginConversions.ContainsKey(key))
                _marginConversions[key] = BuildMarginFormula(tracker, depositCurr);

            return _marginConversions[key];
        }

        public IConversionFormula GetPositiveProfitFormula(ISymbolInfo tracker, string depositCurr)
        {
            var key = (tracker.Name, depositCurr);

            if (!_posProfitConversions.ContainsKey(key))
                _posProfitConversions[key] = BuildProfitFormula(tracker, depositCurr, Bid, Ask);

            return _posProfitConversions[key];
        }

        public IConversionFormula GetNegativeProfitFormula(ISymbolInfo tracker, string depositCurr)
        {
            var key = (tracker.Name, depositCurr);

            if (!_negProfitConversions.ContainsKey(key))
                _negProfitConversions[key] = BuildProfitFormula(tracker, depositCurr, Ask, Bid);

            return _negProfitConversions[key];
        }

        private IConversionFormula BuildMarginFormula(ISymbolInfo node, string depositCurr)
        {
            string X = node.MarginCurrency;
            string Y = node.ProfitCurrency;
            string Z = depositCurr;

            var XY = Exist(X + Y);

            if (X == Z)
                return Formula.Direct; // N 1

            if (Y == Z && XY)
                return Formula.Get(Ask[X + Y]); // N 2

            if (Exist(X + Z))
                return Formula.Get(Ask[X + Z]); // N 3

            if (Exist(Z + X))
                return Formula.Inv(Bid[Z + X]); // N 4

            if (Exist(Y + Z) && XY)
                return Formula.Get(Ask[X + Y]).Mul(Ask[Y + Z]); // N 5

            if (Exist(Z + Y) && XY)
                return Formula.Get(Ask[X + Y]).Div(Bid[Z + Y]); // N 6


            foreach (var curr in _market.Currencies)
            {
                var C = curr.Name;
                var XC = Exist(X + C);
                var ZC = Exist(Z + C);


                if (XC && ZC)
                    return Formula.Get(Ask[X + C]).Div(Bid[Z + C]); // N 7


                var CX = Exist(C + X);

                if (Exist(C + X) && ZC)
                    return Formula.Inv(Bid[C + X]).Div(Bid[Z + C]); // N 8


                var CZ = Exist(C + Z);

                if (XC && CZ)
                    return Formula.Get(Ask[X + C]).Mul(Ask[C + Z]); // N 9

                if (CX && CZ)
                    return Formula.Inv(Bid[C + X]).Mul(Ask[C + Z]); // N 10


                var YC = Exist(Y + C);

                if (YC && ZC && XY)
                    return Formula.Get(Ask[Y + C]).Div(Bid[Z + C]).Mul(Ask[X + Y]); // N 11


                var CY = Exist(C + Y);

                if (CY && ZC && XY)
                    return Formula.Inv(Bid[C + Y]).Div(Bid[Z + C]).Mul(Ask[X + Y]); // N 12

                if (YC && CZ && XY)
                    return Formula.Get(Ask[Y + C]).Mul(Ask[C + Z]).Mul(Ask[X + Y]); // N 13

                if (CY && CZ && XY)
                    return Formula.Inv(Bid[C + Y]).Mul(Ask[C + Z]).Mul(Ask[X + Y]); // N 14
            }

            return Formula.ErrorBuild;
        }


        private IConversionFormula BuildProfitFormula(ISymbolInfo node, string toCurrency, Dictionary<string, ISideNode> price1, Dictionary<string, ISideNode> price2)
        {
            string X = node.MarginCurrency;
            string Y = node.ProfitCurrency;
            string Z = toCurrency;

            var XY = Exist(X + Y);

            if (Y == Z)
                return Formula.Direct; // N 1

            if (X == Z && XY)
                return Formula.Inv(price2[X + Y]); // N 2

            if (Exist(Y + Z))
                return Formula.Get(price1[Y + Z]); // N 3

            if (Exist(Z + Y))
                return Formula.Inv(price2[Z + Y]); // N 4

            if (Exist(Z + X) && XY)
                return Formula.Inv(price2[X + Y]).Div(price2[Z + X]); // N 5

            if (Exist(X + Z) && XY)
                return Formula.Inv(price2[X + Y]).Mul(price1[X + Z]); // N 6


            foreach (var curr in _market.Currencies)
            {
                var C = curr.Name;
                var YC = Exist(Y + C);
                var ZC = Exist(Z + C);

                if (YC && ZC)
                    return Formula.Get(price1[Y + C]).Div(price2[Z + C]); // N 7


                var CY = Exist(C + Y);

                if (CY && ZC)
                    return Formula.Inv(price2[C + Y]).Div(price2[Z + C]); // N 8


                var CZ = Exist(C + Z);

                if (YC && CZ)
                    return Formula.Get(price1[Y + C]).Mul(price1[C + Z]); // N 9

                if (CY && CZ)
                    return Formula.Inv(price2[C + Y]).Mul(price1[C + Z]); // N 10
            }

            return Formula.ErrorBuild;
        }

        private bool Exist(string key) => Bid.ContainsKey(key);
    }
}
