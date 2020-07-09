using System.Globalization;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;
using BO = TickTrader.BusinessObjects;

namespace TickTrader.Algo.Core
{
    public class CurrencyEntity : Api.Currency, BO.ICurrencyInfo
    {
        public string Name { get; private set; }

        public int Digits { get; private set; }

        public int SortOrder { get; private set; }

        public bool IsNull { get; private set; }

        public NumberFormatInfo Format { get; private set; }

        public CurrencyInfo Info { get; private set; }


        int BO.ICurrencyInfo.Precision => Digits;
        int BO.ICurrencyInfo.SortOrder => SortOrder;


        public CurrencyEntity(CurrencyInfo info)
        {
            Update(info);
        }


        public override string ToString()
        {
            return $"{Name} (Digits = {Digits})";
        }

        private void InitFormat()
        {
            Format = FormatExtentions.CreateTradeFormatInfo(Digits);
        }


        #region FDK compatibility

        public int Precision => Digits;

        public BO.CurrencyType Type => BO.CurrencyType.Default;

        #endregion


        public void Update(CurrencyInfo info)
        {
            if (info == null)
            {
                IsNull = true;
            }
            else
            {
                Info = info;
                Name = info.Name;
                Digits = info.Digits;
                SortOrder = info.SortOrder;
                IsNull = false;
                InitFormat();
            }
        }
    }
}