using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Common.Business;
using TickTrader.Common.StringParsing;

namespace TickTrader.Server.QuoteHistory.Serialization
{
    public class BarFormatter : IFormatter<HistoryBar>
    {
        private const string DateTimeFormat = @"yyyy.MM.dd HH:mm:ss";
        private string BarFormatString = "{0:" + DateTimeFormat + "}";

        private static readonly string deserializationExceptionMessage;
        private static readonly IFormatProvider formatProvider;

        private readonly int? _pricePrecision;
        private readonly int? _volumePrecision;

        private BarFormatter()
        {
            BarFormatString += "\t{1}\t{2}\t{3}\t{4}\t{5}";
        }

        public BarFormatter(int? volumePrecision, int? pricePrecision)
        {
            _volumePrecision = volumePrecision;
            _pricePrecision = pricePrecision;

            if (_pricePrecision.HasValue)
                BarFormatString += $"\t{{1:F{_pricePrecision}}}\t{{2:F{_pricePrecision}}}\t{{3:F{_pricePrecision}}}\t{{4:F{_pricePrecision}}}";
            else
                BarFormatString += "\t{1}\t{2}\t{3}\t{4}";

            if (_volumePrecision.HasValue)
                BarFormatString += $"\t{{5:F{_volumePrecision}}}";
            else
                BarFormatString += "\t{5}";
        }

        static BarFormatter()
        {
            formatProvider = CultureInfo.InvariantCulture;
            Default = new BarFormatter();
            deserializationExceptionMessage = typeof(HistoryBar).Name + " deserialization failed.";
        }

        public static BarFormatter Default { get; }

        public string Serialize(HistoryBar item)
        {
            return string.Format(formatProvider, BarFormatString, item.Time,  item.Open, item.Hi, item.Low, item.Close, item.Volume);
        }


        public string Serialize(IEnumerable<HistoryBar> items)
        {
            var itemsList = items as IList<HistoryBar>;

            if (itemsList != null){
                if (!itemsList.Any())
                    return "";
                string[] stringItemsArray = new string[itemsList.Count];
                Parallel.ForEach(items,
                                 (item, state, seq) =>
                                 {
                                     stringItemsArray[seq] = string.Format(formatProvider, BarFormatString, item.Time, item.Open, item.Hi, item.Low, item.Close, item.Volume);
                                 });
                return string.Join("\r\n", stringItemsArray) + "\r\n";
            }else{
                var sb = new StringBuilder(1024);
                foreach (var item in items){
                    sb.AppendFormat(formatProvider, BarFormatString + "\r\n", item.Time, item.Open, item.Hi, item.Low, item.Close, item.Volume);
                }
                return sb.ToString();
            }
        }

        public void Serialize(StreamWriter stream, IEnumerable<HistoryBar> items)
        {
            foreach (var item in items)
            {
                stream.WriteLine(string.Format(formatProvider, BarFormatString, item.Time, item.Open, item.Hi, item.Low, item.Close, item.Volume));
            }
        }

        [Obsolete]
        public HistoryBar DeserializeOld(string line)
        {
            try
            {
                var hb = new HistoryBar();

                int startIndex = 0;
                int foundIndex;
                if ((foundIndex = line.IndexOf('\t', startIndex)) == -1)
                    throw new FormatException(deserializationExceptionMessage);
                hb.Time = DateTime.ParseExact(line.Substring(startIndex, foundIndex - startIndex), DateTimeFormat, formatProvider,
                                              DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);
                startIndex = foundIndex + 1;

                if ((foundIndex = line.IndexOf('\t', startIndex)) == -1)
                    throw new FormatException(deserializationExceptionMessage);
                hb.Open = decimal.Parse(line.Substring(startIndex, foundIndex - startIndex), formatProvider);
                startIndex = foundIndex + 1;

                if ((foundIndex = line.IndexOf('\t', startIndex)) == -1)
                    throw new FormatException(deserializationExceptionMessage);
                hb.Hi = decimal.Parse(line.Substring(startIndex, foundIndex - startIndex), formatProvider);
                startIndex = foundIndex + 1;

                if ((foundIndex = line.IndexOf('\t', startIndex)) == -1)
                    throw new FormatException(deserializationExceptionMessage);
                hb.Low = decimal.Parse(line.Substring(startIndex, foundIndex - startIndex), formatProvider);
                startIndex = foundIndex + 1;

                if ((foundIndex = line.IndexOf('\t', startIndex)) == -1)
                    throw new FormatException(deserializationExceptionMessage);
                hb.Close = decimal.Parse(line.Substring(startIndex, foundIndex - startIndex), formatProvider);
                startIndex = foundIndex + 1;

                hb.Volume = uint.Parse(line.Substring(startIndex), formatProvider);

                return hb;
            }
            catch (Exception ex)
            {
                throw new FormatException(deserializationExceptionMessage, ex);
            }
        }

        public HistoryBar Deserialize(string line)
        {
            int year, mon, day, hour, min, sec;
            
            var _sp = new StringParser(line);

            // Read Date
            _sp.ReadInt32(out year);
            _sp.ValidateVerbatimText(".");
            _sp.ReadInt32(out mon);
            _sp.ValidateVerbatimText(".");
            _sp.ReadInt32(out day);
            _sp.ValidateVerbatimText(" ");
            _sp.ReadInt32(out hour);
            _sp.ValidateVerbatimText(":");
            _sp.ReadInt32(out min);
            _sp.ValidateVerbatimText(":");
            _sp.ReadInt32(out sec);

            _sp.ValidateVerbatimText("\t");

            var dt = new DateTime(year, mon, day, hour, min, sec);
            double lo, hi, op, cl;
            double vol;

            _sp.ReadDouble(out op);
            _sp.ValidateVerbatimText("\t");
            _sp.ReadDouble(out hi);
            _sp.ValidateVerbatimText("\t");
            _sp.ReadDouble(out lo);
            _sp.ValidateVerbatimText("\t");
            _sp.ReadDouble(out cl);
            _sp.ValidateVerbatimText("\t");
            _sp.ReadDouble(out vol);

            var hb = new HistoryBar
            {
                Time = dt,
                Open = (decimal)op,
                Hi = (decimal)hi,
                Low = (decimal)lo,
                Close = (decimal)cl,
                Volume = vol
            };

            return hb;
        }

    }
}