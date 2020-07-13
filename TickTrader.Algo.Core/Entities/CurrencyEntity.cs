using System.Globalization;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Core
{
    public interface ICurrencyInfo
    {
        string Name { get; }

        int Digits { get; }

        int SortOrder { get; }
    }

    public class CurrencyEntity : Api.Currency, ICurrencyInfo
    {
        public string Name { get; private set; }

        public int Digits { get; private set; }

        public int SortOrder { get; private set; }

        public bool IsNull { get; private set; }

        public NumberFormatInfo Format { get; private set; }

        public CurrencyInfo Info { get; private set; }


        public CurrencyEntity(CurrencyInfo info)
        {
            Update(info);
        }


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
                Format = FormatExtentions.CreateTradeFormatInfo(Digits);
            }
        }

        public override string ToString() => $"{Name} (Digits = {Digits})";
    }
}