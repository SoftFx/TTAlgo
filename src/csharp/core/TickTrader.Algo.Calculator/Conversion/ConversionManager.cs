using System.Collections.Generic;
using TickTrader.Algo.Calculator.AlgoMarket;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Calculator.Conversions
{
    public sealed class ConversionManager
    {
        private readonly AlgoMarketState _market;

        private readonly Dictionary<string, IConversionFormula> _marginConversions = new Dictionary<string, IConversionFormula>();
        private readonly Dictionary<string, IConversionFormula> _posProfitConversions = new Dictionary<string, IConversionFormula>();
        private readonly Dictionary<string, IConversionFormula> _negProfitConversions = new Dictionary<string, IConversionFormula>();

        private Dictionary<string, ISideNode> Bid => _market.Bid;

        private Dictionary<string, ISideNode> Ask => _market.Ask;


        private string _accountBalanceCurrency;

        public ConversionManager(AlgoMarketState market)
        {
            _market = market;
        }

        internal void Init(string accBalanceCurrency)
        {
            _accountBalanceCurrency = accBalanceCurrency;

            _marginConversions.Clear();
            _posProfitConversions.Clear();
            _negProfitConversions.Clear();
        }

        internal IConversionFormula GetMarginFormula(ISymbolInfo tracker)
        {
            var key = tracker.Name;

            if (!_marginConversions.ContainsKey(key))
                _marginConversions[key] = BuildMarginFormula(tracker);

            return _marginConversions[key];
        }

        public IConversionFormula GetPositiveProfitFormula(ISymbolInfo tracker)
        {
            var key = tracker.Name;

            if (!_posProfitConversions.ContainsKey(key))
                _posProfitConversions[key] = BuildProfitFormula(tracker, Bid, Ask);

            return _posProfitConversions[key];
        }

        public IConversionFormula GetNegativeProfitFormula(ISymbolInfo tracker)
        {
            var key = tracker.Name;

            if (!_negProfitConversions.ContainsKey(key))
                _negProfitConversions[key] = BuildProfitFormula(tracker, Ask, Bid);

            return _negProfitConversions[key];
        }

        private IConversionFormula BuildMarginFormula(ISymbolInfo node)
        {
            string X = node.MarginCurrency;
            string Y = node.ProfitCurrency;
            string Z = _accountBalanceCurrency;

            var XY = Exist(X + Y);

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


        private IConversionFormula BuildProfitFormula(ISymbolInfo node, Dictionary<string, ISideNode> price1, Dictionary<string, ISideNode> price2)
        {
            string X = node.MarginCurrency;
            string Y = node.ProfitCurrency;
            string Z = _accountBalanceCurrency;

            var XY = Exist(X + Y);

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
