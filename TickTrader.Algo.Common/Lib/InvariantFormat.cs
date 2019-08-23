using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Common.Lib
{
    public static class InvariantFormat
    {
        public const string DateFormat = "yyyy.MM.dd HH:mm:ss.fff";
        public const int DateFormatFixedLength = 23;

        public const string CsvDateFormat = "yyyy-MM-dd HH:mm:ss.fff";

        public static string Format(DateTime time)
        {
            return time.ToString(DateFormat, CultureInfo.InvariantCulture);
        }

        public static string CsvFormat(DateTime time)
        {
            return time.ToString(CsvDateFormat, CultureInfo.InvariantCulture);
        }
    }
}
