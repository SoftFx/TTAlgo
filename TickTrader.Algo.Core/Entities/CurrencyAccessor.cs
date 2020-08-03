using TickTrader.Algo.Api;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Core
{
    public sealed class CurrencyAccessor : Api.Currency
    {
        public bool IsNull { get; private set; }

        public CurrencyInfo Info { get; private set; }

        public CurrencyAccessor(CurrencyInfo info) => Update(info);

        public void Update(CurrencyInfo info)
        {
            IsNull = info == null;
            Info = info ?? Info;
        }

        public override string ToString() => $"{Info.Name} (Digits = {Info.Digits})";

        string Currency.Name => Info.Name;

        int Currency.Digits => Info.Digits;

        bool Currency.IsNull => IsNull;
    }
}