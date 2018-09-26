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
            Digits = 2;
        }

        public string Name { get; private set; }
        public int Digits { get; set; }
        public int SortOrder { get; set; }
        public bool IsNull { get; set; }

        int BO.ICurrencyInfo.Precision => Digits;
        int BO.ICurrencyInfo.SortOrder => SortOrder;

        #region FDK compatibility

        public int Precision => Digits;

        #endregion

        public override string ToString() { return $"{Name} (Digits = {Digits})"; }
    }
}