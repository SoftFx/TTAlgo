using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Data;

namespace TickTrader.BotTerminal
{
    public class PascalCaseSplitConverter : IValueConverter, IMultiValueConverter
    {
        private static readonly Dictionary<string, string> _caсhedValues = new Dictionary<string, string>(5)
        {
            {"BuyStopLimit", "Buy StopLimit" },
            {"SellStopLimit", "Sell StopLimit" },
            {"SellStopLimitCanceled", "Sell StopLimit Canceled" },
            {"BuyStopLimitCanceled", "Buy StopLimit Canceled" },
            {"StopOut", "StopOut"},
        };

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;

            var key = value.ToString();

            if (string.IsNullOrEmpty(key))
                return null;

            return _caсhedValues.ContainsKey(key) ? _caсhedValues[key] : SetCorrectValue(key);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private static string SetCorrectValue(string key)
        {
            var ans = new StringBuilder(1 << 4);

            foreach (var c in key)
            {
                if (char.IsUpper(c))
                    ans.Append(' ');
                ans.Append(c);
            }

            _caсhedValues.Add(key, ans.ToString().TrimStart());

            return _caсhedValues[key];
        }

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var orderType = Convert(values[0], targetType, parameter, culture);

            return values.Length == 1 || values[1] is null ? orderType : $"{values[1]}/{orderType}";
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
