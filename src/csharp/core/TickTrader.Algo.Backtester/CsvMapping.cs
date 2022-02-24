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
                Map(p => p.Time).Index(0).Name("TimeUtc").TypeConverter<TimeMsTypeConverter>();
                Map(p => p.Value).Index(1).Name("Value");
            }
        }

        public class ForTradeReport : ClassMap<TradeReportInfo>
        {
            public ForTradeReport()
            {
                Map(t => t.Id).Index(0).Name("TrId");
                Map(t => t.TransactionTime).Index(1).Name("TrTime").TypeConverter<ProtoTimestampTypeConverter>();
                Map(t => t.OrderId).Index(2).Name("OrderId");
                Map(t => t.ActionId).Index(3).Name("ActionId");
                Map(t => t.ReportType).Index(4).Name("ReportType");
                Map(t => t.TransactionReason).Index(5).Name("TrReason");
                Map(t => t.TransactionAmount).Index(6).Name("TrAmount");
                Map(t => t.TransactionCurrency).Index(7).Name("TrCurrency");

                Map(t => t.Symbol).Index(8).Name("Symbol");
                Map(t => t.OrderSide).Index(9).Name("Side");
                Map(t => t.OrderType).Index(10).Name("Type");
                Map(t => t.Price).Index(11).Name("Price");
                Map(t => t.StopPrice).Index(12).Name("StopPrice");
                Map(t => t.StopLoss).Index(13).Name("StopLoss");
                Map(t => t.TakeProfit).Index(14).Name("TakeProfit");
                Map(t => t.OpenQuantity).Index(15).Name("OpenAmount");
                Map(t => t.RemainingQuantity).Index(16).Name("RemainingAmount");
                Map(t => t.MaxVisibleQuantity).Index(17).Name("MaxVisibleAmount");
                Map(t => t.OrderOptions).Index(18).Name("OrderOptions");
                Map(t => t.Slippage).Index(19).Name("Slippage");
                Map(t => t.Expiration).Index(20).Name("Expiration").TypeConverter<ProtoTimestampTypeConverter>();
                Map(t => t.OrderOpened).Index(21).Name("OrderOpened").TypeConverter<ProtoTimestampTypeConverter>();
                Map(t => t.OrderModified).Index(22).Name("OrderModified").TypeConverter<ProtoTimestampTypeConverter>();

                Map(t => t.Swap).Index(23).Name("Swap");
                Map(t => t.Commission).Index(24).Name("Commission");
                Map(t => t.CommissionCurrency).Index(25).Name("CommissionCurrency");
                Map(t => t.OrderLastFillAmount).Index(26).Name("FillAmount");
                Map(t => t.OrderFillPrice).Index(27).Name("FillPrice");

                Map(t => t.RequestedOrderType).Index(28).Name("ReqOrderType");
                Map(t => t.RequestedOpenPrice).Index(29).Name("ReqOpenPrice");
                Map(t => t.RequestedOpenQuantity).Index(30).Name("ReqOpenAmount");
                Map(t => t.RequestedClosePrice).Index(31).Name("ReqClosePrice");
                Map(t => t.RequestedCloseQuantity).Index(32).Name("ReqCloseAmount");

                Map(t => t.PositionId).Index(33).Name("PosId");
                Map(t => t.PositionById).Index(34).Name("PosById");
                Map(t => t.PositionQuantity).Index(35).Name("PosAmount");
                Map(t => t.PositionOpenPrice).Index(36).Name("PosOpenPrice");
                Map(t => t.PositionClosePrice).Index(37).Name("PosClosePrice");
                Map(t => t.PositionCloseQuantity).Index(38).Name("PosCloseAmount");
                Map(t => t.PositionRemainingSide).Index(39).Name("PosRemainingSide");
                Map(t => t.PositionLeavesQuantity).Index(40).Name("PosRemainingAmount");
                Map(t => t.PositionRemainingPrice).Index(41).Name("PosRemainingPrice");
                Map(t => t.PositionOpened).Index(42).Name("PosOpened").TypeConverter<ProtoTimestampTypeConverter>();
                Map(t => t.PositionModified).Index(43).Name("PosModified").TypeConverter<ProtoTimestampTypeConverter>();
                Map(t => t.PositionClosed).Index(44).Name("PosClosed").TypeConverter<ProtoTimestampTypeConverter>();
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
                if (string.IsNullOrEmpty(text))
                    return default(Timestamp);

                var parts = text.Split('+');
                var time = DateTime.Parse(parts[0], CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal).ToUniversalTime().ToTimestamp();
                if (parts.Length > 1)
                    time.Nanos += int.Parse(parts[1]);
                return time;
            }

            public string ConvertToString(object value, IWriterRow row, MemberMapData memberMapData)
            {
                var time = value as Timestamp;
                if (time == null)
                    return string.Empty;

                return $"{InvariantFormat.CsvFormat(time.ToDateTime().ToUniversalTime())}+{time.Nanos % 1_000_000}";
            }
        }

        private class TimeMsTypeConverter : ITypeConverter
        {
            public object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
            {
                if (string.IsNullOrEmpty(text))
                    return 0;

                var time = DateTime.Parse(text, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);
                return TimeMs.FromDateTime(time);
            }

            public string ConvertToString(object value, IWriterRow row, MemberMapData memberMapData)
            {
                if (!(value is long))
                    return string.Empty;

                var timeMs = (long)value;
                return InvariantFormat.CsvFormat(TimeMs.ToUtc(timeMs));
            }
        }
    }
}
