using System;
using BO = TickTrader.BusinessObjects;

namespace TickTrader.Algo.Core
{
    [Serializable]
    public class CurrencyEntity : Api.Currency, BO.ICurrencyInfo
    {
        public CurrencyEntity(string code)
        {
            Name = code;
        }

        public string Name { get; private set; }
        public int Digits { get; set; }
        public int SortOrder { get; set; }
        public bool IsNull => false;

        int BO.ICurrencyInfo.Precision => Digits;
        int BO.ICurrencyInfo.SortOrder => SortOrder;

        public override string ToString() { return $"{Name} (Digits = {Digits})"; }
    }
}