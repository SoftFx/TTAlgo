using System;
using System.CodeDom;
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
    public class FeedTickLevel2Formatter : IFormatter<TickValue>
    {
        private readonly static IFormatProvider formatProvider;
        private static readonly string deserializationExceptionMessage;
        private static readonly FeedTickLevel2Formatter instance = new FeedTickLevel2Formatter();

        static FeedTickLevel2Formatter()
        {
            formatProvider = CultureInfo.InvariantCulture;
            deserializationExceptionMessage = typeof(FeedTick).Name + " level2 deserialization failed.";
        }

        private FeedTickLevel2Formatter()
        {
        }

        public static FeedTickLevel2Formatter Instance
        {
            get { return instance; }
        }

        public string Serialize(TickValue item)
        {
            return Serialize(item, null);
        }

        public string Serialize(IEnumerable<TickValue> items)
        {
            var itemsList = items as IList<TickValue>;

            if (itemsList != null)
            {
                if (!itemsList.Any())
                    return "";
                string[] stringItemsArray = new string[itemsList.Count];
                Parallel.ForEach(items,
                                 (item, state, seq) =>
                                 {
                                     stringItemsArray[seq] = Serialize(item);
                                 });
                return string.Join("\r\n", stringItemsArray) + "\r\n";
            }
            else
            {
                var sb = new StringBuilder(1024);
                foreach (var tick in items)
                {
                    Serialize(tick, sb);
                    sb.Append("\r\n");
                }
                return sb.ToString();
            }
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
        public string Serialize(TickValue tick, StringBuilder sb, bool do_throw=false)
        {
            try
            {
                var builder = sb ?? new StringBuilder();
                var bidCnt = tick.GetBidCount();
                var askCnt = tick.GetAskCount();
                int numOfStr = 1;
                if (bidCnt > 0)
                {
                    numOfStr += 1;
                    numOfStr += bidCnt*4;
                }
                if (askCnt > 0)
                {
                    numOfStr += 1;
                    numOfStr += askCnt*4;
                }
                var resStr = new string[numOfStr];
                int ind = 0;
                resStr[ind] = ASCIIEncoding.ASCII.GetString(WriteDateToDateBuffer(tick.Id.Time)) +
                              tick.Id.GetIndexSuffix();
                ind++;

                var en = tick.Level2.GetEnumerator();

                if (bidCnt > 0)
                {
                    resStr[ind] = "\tbid";
                    ind++;
                }
                while (bidCnt > 0)
                {
                    en.MoveNext();
                    resStr[ind] = "\t";
                    ind++;
                    resStr[ind] = ((decimal) en.CurrentL2Value().Price).ToString(formatProvider);
                    ind++;
                    resStr[ind] = "\t";
                    ind++;
                    resStr[ind] = en.CurrentL2Value().Volume.ToString(formatProvider);
                    ind++;
                    bidCnt--;
                }

                if (askCnt > 0)
                {
                    resStr[ind] = "\task";
                    ind++;
                }
                while (askCnt > 0)
                {
                    en.MoveNext();
                    resStr[ind] = "\t";
                    ind++;
                    resStr[ind] = ((decimal) en.CurrentL2Value().Price).ToString(formatProvider);
                    ind++;
                    resStr[ind] = "\t";
                    ind++;
                    resStr[ind] = en.CurrentL2Value().Volume.ToString(formatProvider);
                    ind++;
                    askCnt--;
                }

                builder.Append(String.Concat(resStr));
                return sb != null ? null : builder.ToString();
            }
            catch (ArgumentOutOfRangeException ex)
            {
                if (do_throw)
                    throw ex;
                TickValue new_tick = new TickValue(tick.Id,tick.Level2);
                return Serialize(new_tick, sb, true);
            }
        }

        public void Serialize(StreamWriter stream, IEnumerable<TickValue> items)
        {
            foreach (var tick in items)
            {
                stream.Write(tick.Id.ToString());

                var bCount = tick.GetBidCount();
                if (bCount > 0)
                {
                    stream.Write("\tbid");
                    for (int i = 0; i < bCount; i++)
                    {
                        var tv = tick.Level2.GetTickLevel2BidValue(i);
                        stream.Write(string.Format(formatProvider, "\t{0:G}\t{1:G}", tv.Price, tv.Volume));
                    }
                }

                var aCount = tick.GetAskCount();
                if (aCount > 0)
                {
                    stream.Write("\task");
                    for (int i = 0; i < aCount; i++)
                    {
                        var tv = tick.Level2.GetTickLevel2AskValue(i);
                        stream.Write(string.Format(formatProvider, "\t{0:G}\t{1:G}", tv.Price, tv.Volume));
                    }
                }
                stream.Write("\r\n");
            }
        }

        public TickValue Deserialize2(string line)
        {
            try
            {

                int startIndex = 0;
                int foundIndex;

                var level2 = new List<FeedLevel2Record>();

                if ((foundIndex = line.IndexOf('\t', startIndex)) == -1)
                    throw new FormatException(deserializationExceptionMessage);

                var id = FeedTickId.Parse(line.Substring(0, foundIndex - startIndex));
                startIndex = foundIndex + 1;
                var recType = FxPriceType.Bid;

                var lLen = line.Length;
                do
                {
                    if ( startIndex+3 <= lLen && line[startIndex] == 'b' && line[startIndex + 1] == 'i' && line[startIndex + 2] == 'd' )
                    {
                        recType = FxPriceType.Bid;
                        startIndex += 4;
                    }
                    else if (startIndex + 3 <= lLen && line[startIndex] == 'a' && line[startIndex + 1] == 's' && line[startIndex + 2] == 'k')
                    {
                        recType = FxPriceType.Ask;
                        startIndex += 4;
                    }
                    else
                    {
                        if ((foundIndex = line.IndexOf('\t', startIndex)) == -1)
                            throw new FormatException(deserializationExceptionMessage);
                        var pr = decimal.Parse(line.Substring(startIndex, foundIndex - startIndex), formatProvider);
                        startIndex = foundIndex + 1;

                        double vl = double.Parse((foundIndex = line.IndexOf('\t', startIndex)) == -1 ? line.Substring(startIndex) :
                                                                                                line.Substring(startIndex, foundIndex - startIndex), formatProvider);

                        level2.Add(new FeedLevel2Record
                        {
                            Type = recType,
                            Price = pr,
                            Volume = vl
                        });

                        startIndex = foundIndex + 1;
                    }
                } while (foundIndex != -1);

                return new TickValue(id, level2);
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


        public TickValue DeserializeOld(string line)
        {
            try{

                    int startIndex = 0;
                    int foundIndex;

                    var level2 = new List<FeedLevel2Record>();

                    if ((foundIndex=line.IndexOf('\t', startIndex)) == -1)
                        throw new FormatException(deserializationExceptionMessage);

                    var id = FeedTickId.Parse(line.Substring(0, foundIndex-startIndex));
                    startIndex=foundIndex+1;
                    var recType = FxPriceType.Bid;

                    do
                    {
                        if (line.Substring(startIndex, 3) == "bid")
                        {
                            recType = FxPriceType.Bid;
                            startIndex += 4;
                        }
                        else if (line.Substring(startIndex, 3) == "ask")
                        {
                            recType = FxPriceType.Ask; 
                            startIndex += 4;
                        }
                        else
                        {
                            if ((foundIndex = line.IndexOf('\t', startIndex)) == -1)
                                throw new FormatException(deserializationExceptionMessage);
                            var pr = decimal.Parse(line.Substring(startIndex, foundIndex - startIndex), formatProvider);
                            startIndex = foundIndex + 1;

                            double vl = double.Parse((foundIndex = line.IndexOf('\t', startIndex)) == -1 ? line.Substring(startIndex) : 
                                                                                                    line.Substring(startIndex, foundIndex - startIndex), formatProvider);

                            level2.Add(new FeedLevel2Record
                                           {
                                               Type = recType,
                                               Price = pr,
                                               Volume = vl
                                           });
                            startIndex = foundIndex + 1;
                        }
                    } while ( foundIndex != -1);

                return new TickValue(id, level2);
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

        public TickValue Deserialize(string tickStr)
        {
            int year, mon, day, hour, min, sec, msec;

            // Moved from static consume 9% but allow parallel execution
            var _sp = new StringParser(tickStr);

            var level2 = new List<Level2Value>();
            byte askCount = 0;
            byte bidCount = 0;
            
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

            FeedTickId id;

            if (_sp.TryValidateVerbatimText("-"))
            {
                int num;
                _sp.ReadInt32(out num);
                id = new FeedTickId(new DateTime(year, mon, day, hour, min, sec, msec), (byte) num);
            }
            else
            {
                id = new FeedTickId(new DateTime(year, mon, day, hour, min, sec, msec));
            }

            bool PricesExist = false;
            var recType = FxPriceType.Bid;

            if (_sp.TryValidateVerbatimText("\tbid")) { 
                recType = FxPriceType.Bid;
                PricesExist = true;
            }

            do
            {
                if (_sp.TryValidateVerbatimText("\task"))
                {
                    recType = FxPriceType.Ask;
                    PricesExist = true;
                }
                else
                {
                    if (PricesExist)
                    {
                        _sp.ValidateVerbatimText("\t");

                        double pr;
                        _sp.ReadDouble(out pr);

                        _sp.ValidateVerbatimText("\t");

                        double vl;
                        _sp.ReadDouble(out vl);

                        var l2R = new Level2Value(new Price((decimal) pr), vl);

                        level2.Add(l2R);

                        if (recType == FxPriceType.Ask)
                            askCount++;
                        else
                            bidCount++;
                    }
                    else break;
                }
            } while (!_sp.IsEnd());

            return new TickValue(id, level2, 0, bidCount, askCount);
        }

    }
}