using SciChart.Charting.Visuals.Axes.LabelProviders;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.BotTerminal
{
    public class DayNameLabelProvider : LabelProviderBase
    {
        private static readonly string[] DayNames = new string[] { "Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat" };

        public override string FormatLabel(IComparable dataValue)
        {
            var dayNo = (int)(double)dataValue;
            if (dayNo >= 0 && dayNo <= 6)
            {
                //var dayOfWeek = (DayOfWeek)dayNo;
                //return DateTimeFormatInfo.CurrentInfo.GetDayName(dayOfWeek);
                return DayNames[dayNo];
            }
            return dayNo.ToString();
        }

        public override string FormatCursorLabel(IComparable dataValue)
        {
            return FormatLabel(dataValue);
        }
    }
}
