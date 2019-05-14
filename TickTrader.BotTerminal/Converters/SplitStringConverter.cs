using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace TickTrader.BotTerminal
{
    public class SplitStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var str = value.ToString();

            if (string.IsNullOrEmpty(str))
                return null;

            string ans = string.Empty;

            foreach(var c in str)
            {
                if (char.IsUpper(c))
                    ans += " ";
                ans += c;
            }

            return ans.TrimStart();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
