using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Common.Business;
using TickTrader.BusinessObjects;
using TickTrader.Common.StringParsing;
using TickTrader.Server.QuoteHistory.Serialization.Binary;

namespace TickTrader.Server.QuoteHistory.Serialization
{
    public class FeedTickFormatter : IFormatter<TickValue>
    {
        private const string TickFormatString = "{0}\t{1:G}\t{2:G}\t{3:G}\t{4:G}";
        private readonly static IFormatProvider formatProvider;
        private static readonly string deserializationExceptionMessage;
        private static readonly FeedTickFormatter instance = new FeedTickFormatter();

        public string FormatString { get { return TickFormatString; } }

        static FeedTickFormatter()
        {
            formatProvider = CultureInfo.InvariantCulture;
            deserializationExceptionMessage = typeof(FeedTick).Name + " deserialization failed.";
        }

        private FeedTickFormatter()
        {
        }

        public static FeedTickFormatter Instance
        {
            get { return instance; }
        }

        private static byte[] WriteDateToDateBuffer(DateTime time)
        {
            var result = new byte[23];
            var pos = ByteArrayBuilder.WriteIntToByteBuffer(result, 0, 4, time.Year);
            result[pos++] = (byte)'.';
            pos = ByteArrayBuilder.WriteIntToByteBuffer(result, pos, 2, time.Month);
            result[pos++] = (byte)'.';
            pos = ByteArrayBuilder.WriteIntToByteBuffer(result, pos, 2, time.Day);
            result[pos++] = (byte)' ';
            pos = ByteArrayBuilder.WriteIntToByteBuffer(result, pos, 2, time.Hour);
            result[pos++] = (byte)':';
            pos = ByteArrayBuilder.WriteIntToByteBuffer(result, pos, 2, time.Minute);
            result[pos++] = (byte)':';
            pos = ByteArrayBuilder.WriteIntToByteBuffer(result, pos, 2, time.Second);
            result[pos++] = (byte)'.';
            ByteArrayBuilder.WriteIntToByteBuffer(result, pos, 3, time.Millisecond);
            return result;
        }

        public string Serialize(IEnumerable <TickValue> items)
        {
            var itemsList = items as IList<TickValue>;
            string resultStr = "";
            
            if (itemsList != null)
            {
                if (!itemsList.Any())
                    return resultStr;
                var stringItemsArray = new string[itemsList.Count];
                Parallel.ForEach(items,
                                 (item, state, seq) =>
                                     {
                                         var curResStr = new string[9];
                                         curResStr[0] = ASCIIEncoding.ASCII.GetString(WriteDateToDateBuffer(item.Id.Time));
                                         curResStr[1] = item.Id.GetIndexSuffix() + "\t";
                                         curResStr[2]= item.BestBid.Price.ToString(formatProvider);
                                         curResStr[3] = "\t";
                                         curResStr[4] = item.BestBid.Volume.ToString(formatProvider);
                                         curResStr[5] = "\t";
                                         curResStr[6] = item.BestAsk.Price.ToString(formatProvider);
                                         curResStr[7] = "\t";
                                         curResStr[8] = item.BestAsk.Volume.ToString(formatProvider);
                                         stringItemsArray[seq] = String.Concat(curResStr);
                                     });
                resultStr = string.Join("\r\n", stringItemsArray) + "\r\n";
            }
            else
            {
                var sb = new StringBuilder(1024);
                var curResStr = new string[9];
                foreach (var item in items)
                { 
                    curResStr[0] = ASCIIEncoding.ASCII.GetString(WriteDateToDateBuffer(item.Id.Time));
                    curResStr[1] = item.Id.GetIndexSuffix()+"\t";
                    curResStr[2] = item.BestBid.Price.ToString(formatProvider);
                    curResStr[3] = "\t";
                    curResStr[4] = item.BestBid.Volume.ToString(formatProvider);
                    curResStr[5] = "\t";
                    curResStr[6] = item.BestAsk.Price.ToString(formatProvider);
                    curResStr[7] = "\t";
                    curResStr[8] = item.BestAsk.Volume.ToString(formatProvider)+ "\r\n";
                    sb.Append(String.Concat(curResStr));
                }
                resultStr = sb.ToString();
            }
            return resultStr;

        }

        public void Serialize(StreamWriter stream, IEnumerable<TickValue> items)
        {

            foreach (var item in items)
            {
         

                stream.WriteLine(string.Format(formatProvider, TickFormatString, item.Id, item.BestBid.Price,
                    item.BestBid.Volume, item.BestAsk.Price, item.BestAsk.Volume));

            }
        }

        public string Serialize(TickValue item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }

            return string.Format(formatProvider, TickFormatString, item.Id, item.BestBid.Price, item.BestBid.Volume, item.BestAsk.Price, item.BestAsk.Volume);
        }

        public TickValue Deserialize(string tickStr)
        {
            int year, mon, day, hour, min, sec, msec;
            double ask_price, ask_vol, bid_price, bid_vol;
            FeedTickId id;

            // Moved from static consume 9% but allow parallel execution
            var _sp = new StringParser(tickStr);

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
            _sp.ValidateVerbatimText(".");
            _sp.ReadInt32(out msec);

            if (_sp.TryValidateVerbatimText("-"))
            {
                int num;
                _sp.ReadInt32(out num);
                id = new FeedTickId(new DateTime(year, mon, day, hour, min, sec, msec), (byte)num);
            }
            else
            {
                id = new FeedTickId(new DateTime(year, mon, day, hour, min, sec, msec));
            }


            _sp.ValidateVerbatimText("\t");
            // Read Bid Price
            _sp.ReadDouble(out bid_price);
           
            _sp.ValidateVerbatimText("\t");
            // Read Bid Volume
            _sp.ReadDouble(out bid_vol);
            _sp.ValidateVerbatimText("\t");
            // Read Ask Price
            _sp.ReadDouble(out ask_price);
            _sp.ValidateVerbatimText("\t");
            // Read Ask Volume
            _sp.ReadDouble(out ask_vol);

            return new TickValue(id, new Level2Value(new Price((decimal) bid_price), bid_vol), new Level2Value(new Price((decimal) ask_price), ask_vol));

        }

        [Obsolete]
        public TickValue DeserializeOld(string line)
        {
            if (line == null)
            {
                throw new ArgumentNullException("line");
            }

            try
            {
                int startIndex = 0;
                int foundIndex;
                if ((foundIndex = line.IndexOf('\t', startIndex)) == -1)
                    throw new FormatException(deserializationExceptionMessage);

                var dt = FeedTickId.Parse(line.Substring(startIndex, foundIndex - startIndex));
                startIndex = foundIndex + 1;
                if ((foundIndex = line.IndexOf('\t', startIndex)) == -1)
                    throw new FormatException(deserializationExceptionMessage);
                var bp = decimal.Parse(line.Substring(startIndex, foundIndex - startIndex), formatProvider);
                startIndex = foundIndex + 1;

                if ((foundIndex = line.IndexOf('\t', startIndex)) == -1)
                    throw new FormatException(deserializationExceptionMessage);
                var bv = double.Parse(line.Substring(startIndex, foundIndex - startIndex), formatProvider);
                startIndex = foundIndex + 1;

                if ((foundIndex = line.IndexOf('\t', startIndex)) == -1)
                    throw new FormatException(deserializationExceptionMessage);
                var ap = decimal.Parse(line.Substring(startIndex, foundIndex - startIndex), formatProvider);
                startIndex = foundIndex + 1;

                var av = double.Parse(line.Substring(startIndex), formatProvider);

                return new TickValue(dt, new[]
                                             {
                                                 new FeedLevel2Record {Type = FxPriceType.Bid, Price = bp, Volume = bv},
                                                 new FeedLevel2Record {Type = FxPriceType.Ask, Price = ap, Volume = av}
                                             });
            }
            catch (IndexOutOfRangeException e)
            {
                throw new FormatException(deserializationExceptionMessage, e);
            }
            catch (FormatException e)
            {
                throw new FormatException(deserializationExceptionMessage, e);
            }
            catch (OverflowException e)
            {
                throw new FormatException(deserializationExceptionMessage, e);
            }
        }
    }
}