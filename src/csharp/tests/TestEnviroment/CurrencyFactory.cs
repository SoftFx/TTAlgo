using TickTrader.Algo.Domain;

namespace TestEnviroment
{
    internal static class CurrencyFactory
    {
        private static readonly CurrencyInfo _prototype = new()
        {
            Digits = 2,
        };


        public static CurrencyInfo BuildCurrency(string name)
        {
            var currency = _prototype.Clone();

            currency.Name = name;

            return currency;
        }
    }
}
