using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using System;
using System.Globalization;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Backtester
{
    internal class CsvMapping
    {
        public class ForLogRecord : ClassMap<PluginLogRecord>
        {
            public ForLogRecord()
            {
                Map(r => r.TimeUtc).Index(0).Name("TimeUtc").TypeConverter<ProtoTimestampTypeConverter>();
                Map(r => r.Severity).Index(1).Name("Severity");
                Map(r => r.Message).Index(2).Name("Message");
                Map(r => r.Details).Index(3).Name("Details");
            }
        }

        public class ForBarData : ClassMap<BarData>
        {
            public ForBarData()
            {
                Map(b => b.OpenTime).Index(0).Name("OpenTimeUtc").TypeConverter<ProtoTimestampTypeConverter>();
                Map(b => b.Open).Index(1).Name("Open");
                Map(b => b.High).Index(2).Name("High");
                Map(b => b.Low).Index(3).Name("Low");
                Map(b => b.Close).Index(4).Name("Close");
            }
        }

        public class ForOutputPoint : ClassMap<OutputPoint>
        {
            public ForOutputPoint()
            {
                Map(p => p.Time).Index(0).Name("TimeUtc").TypeConverter<ProtoTimestampTypeConverter>();
                Map(p => p.Index).Index(1).Name("Index");
                Map(p => p.Value).Index(2).Name("Value").TypeConverter<ProtoAnyTypeConverter>();
            }
        }

        public class ForTradeReport : ClassMap<TradeReportInfo>
        {
            public ForTradeReport()
            {
                Map(t => t.TransactionTime).Index(0).Name("TrTime").TypeConverter<ProtoTimestampTypeConverter>();
                Map(t => t.OrderId).Index(1).Name("OrderId");
                Map(t => t.ActionId).Index(2).Name("ActionId");
                Map(t => t.Symbol).Index(3).Name("Symbol");
                Map(t => t.OrderSide).Index(4).Name("Side");
                Map(t => t.OrderType).Index(5).Name("Type");
                Map(t => t.OrderLastFillAmount).Index(6).Name("LastFillAmount");
                Map(t => t.OrderFillPrice).Index(7).Name("LastFillPrice");
            }
        }


        private class ProtoAnyTypeConverter : ITypeConverter
        {
            public object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
            {
                return Any.Descriptor.Parser.ParseFrom(Convert.FromBase64String(text));
            }

            public string ConvertToString(object value, IWriterRow row, MemberMapData memberMapData)
            {
                return ((Any)value).ToByteString().ToBase64();
            }
        }

        private class ProtoTimestampTypeConverter : ITypeConverter
        {
            public object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
            {
                return DateTime.Parse(text, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal).ToUniversalTime().ToTimestamp();
            }

            public string ConvertToString(object value, IWriterRow row, MemberMapData memberMapData)
            {
                return InvariantFormat.CsvFormat(((Timestamp)value).ToDateTime());
            }
        }
    }
}
