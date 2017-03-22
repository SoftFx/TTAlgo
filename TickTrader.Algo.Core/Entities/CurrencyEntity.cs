using System;

namespace TickTrader.Algo.Core
{
    [Serializable]
    public class CurrencyEntity : Api.Currency
    {
        public CurrencyEntity(string code)
        {
            Name = code;
        }

        public string Name { get; private set; }
        public int Digits { get; set; }
        public bool IsNull => false;

        public override string ToString() { return $"{Name} (Digits = {Digits})"; }
    }
}