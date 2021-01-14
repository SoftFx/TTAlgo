using TickTrader.Algo.Api;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Core
{
    public sealed class CurrencyAccessor : BaseSymbolAccessor<CurrencyInfo>, Api.Currency
    {
        public CurrencyAccessor(CurrencyInfo info) : base(info) { }

        public override string ToString() => $"{Info.Name} (Digits = {Info.Digits})";

        string Currency.Name => Info.Name;

        int Currency.Digits => Info.Digits;

        string Currency.Type => Info.Type;

        bool Currency.IsNull => IsNull;
    }

    public abstract class BaseSymbolAccessor<T> : IBaseSymbolAccessor<T> where T: class
    {
        public bool IsNull { get; private set; }

        public T Info { get; private set; }


        public BaseSymbolAccessor(T info) => Update(info);

        public void Update(T info)
        {
            IsNull = info == null;
            Info = info ?? Info;
        }
    }

    public interface IBaseSymbolAccessor<T>
    {
        bool IsNull { get; }

        T Info { get; }


        void Update(T info);
    }
}