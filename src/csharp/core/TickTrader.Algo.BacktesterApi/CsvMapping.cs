﻿using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using System;
using System.Globalization;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.BacktesterApi
{
    public class CsvMapping
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
                Map(b => b.OpenTime).Index(0).Name("OpenTimeUtc").TypeConverter<UtcTicksTypeConverter>();
                Map(b => b.Open).Index(1).Name("Open");
                Map(b => b.High).Index(2).Name("High");
                Map(b => b.Low).Index(3).Name("Low");
                Map(b => b.Close).Index(4).Name("Close");
            }
        }


        public abstract class RoundClassMap<T> : ClassMap<T> where T : IOutputPoint
        {
            private readonly MemberMap<T, double> _valueMap;


            protected abstract string TimeHeader { get; }


            public RoundClassMap()
            {
                Map(p => p.Time).Index(0).Name(TimeHeader).TypeConverter<UtcTicksTypeConverter>();

                _valueMap = Map(p => p.Value).Index(1).Name("Value");
            }


            public RoundClassMap<T> SetPricision(int precision)
            {
                _valueMap.TypeConverter(new RoundedDoubleConverter(precision));

                return this;
            }
        }

        public class ForDoublePoint : RoundClassMap<OutputPoint>
        {
            public const string Header = "TimeUtc_Double";

            protected override string TimeHeader => Header;


            public static OutputPoint Read(IReaderRow reader)
            {
                if (!UtcTicksTypeConverter.TryReadTime(reader[0], out var time))
                    throw new ArgumentException($"Can't read time at row {reader.CurrentIndex}");
                if (!reader.TryGetField<double>(1, out var value))
                    throw new ArgumentException($"Can't read y value at row {reader.CurrentIndex}");
                return new OutputPoint(time, value);
            }
        }

        public readonly struct MarkerPointWrapper : IOutputPoint
        {
            private readonly OutputPoint _point;
            private readonly MarkerInfo _marker;


            public UtcTicks Time => _point.Time;

            public double Value => _point.Value;

            public MarkerInfo.Types.IconType Icon => _marker.Icon;

            public uint? ColorArgb => _marker.ColorArgb;

            public string DisplayText => _marker.DisplayText;

            public ushort IconCode => (ushort)_marker.IconCode;


            public MarkerPointWrapper(OutputPoint point)
            {
                _point = point;
                _marker = (MarkerInfo)point.Metadata;
            }
        }

        // Old converter for reading legacy markers
        public class ForMarkerPoint : RoundClassMap<MarkerPointWrapper>
        {
            public const string Header = "TimeUtc_Marker";

            protected override string TimeHeader => Header;


            public ForMarkerPoint() : base()
            {
                Map(p => p.Icon).Index(2).Name("Icon");
                Map(p => p.ColorArgb).Index(3).Name("ColorArgb");
                Map(p => p.DisplayText).Index(4).Name("DisplayText");
            }


            public static OutputPoint Read(IReaderRow reader)
            {
                if (!UtcTicksTypeConverter.TryReadTime(reader[0], out var time))
                    throw new ArgumentException($"Can't read time at row {reader.CurrentIndex}");
                if (!reader.TryGetField<double>(1, out var value))
                    throw new ArgumentException($"Can't read y value at row {reader.CurrentIndex}");
                if (!reader.TryGetField<MarkerInfo.Types.IconType>(2, out var icon))
                    throw new ArgumentException($"Can't read icon type at row {reader.CurrentIndex}");
                if (!reader.TryGetField<uint>(3, out var color))
                    throw new ArgumentException($"Can't read marker color at row {reader.CurrentIndex}");
                if (!reader.TryGetField<string>(4, out var text))
                    throw new ArgumentException($"Can't read marker text at row {reader.CurrentIndex}");
                var marker = new MarkerInfo { Icon = icon, ColorArgb = color, DisplayText = text };
                return new OutputPoint(time, value, marker);
            }
        }

        public class ForMarkerPoint2 : RoundClassMap<MarkerPointWrapper>
        {
            public const string Header = "TimeUtc_Marker2";

            protected override string TimeHeader => Header;


            public ForMarkerPoint2() : base()
            {
                Map(p => p.Icon).Index(2).Name("Icon");
                Map(p => p.ColorArgb).Index(3).Name("ColorArgb").TypeConverter<NullUInt32HexConverter>();
                Map(p => p.DisplayText).Index(4).Name("DisplayText");
                Map(p => p.IconCode).Index(5).Name("SymbolCode");
            }


            public static OutputPoint Read(IReaderRow reader)
            {
                if (!UtcTicksTypeConverter.TryReadTime(reader[0], out var time))
                    throw new ArgumentException($"Can't read time at row {reader.CurrentIndex}");
                if (!reader.TryGetField<double>(1, out var value))
                    throw new ArgumentException($"Can't read y value at row {reader.CurrentIndex}");
                if (!reader.TryGetField<MarkerInfo.Types.IconType>(2, out var type))
                    type = MarkerInfo.Types.IconType.UnknownIconType;
                if (!reader.TryGetField<uint?>(3, out var color))
                    color = null;
                if (!reader.TryGetField<string>(4, out var text))
                    throw new ArgumentException($"Can't read marker text at row {reader.CurrentIndex}");
                if (!reader.TryGetField<ushort>(5, out var code) && type == MarkerInfo.Types.IconType.Wingdings)
                    throw new ArgumentException($"Can't read marker code at row {reader.CurrentIndex}");

                var marker = new MarkerInfo { Icon = type, ColorArgb = color, DisplayText = text, IconCode = code };
                return new OutputPoint(time, value, marker);
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

                Map(t => t.Comment).Index(45).Name("Comment");
                Map(t => t.Tag).Index(46).Name("Tag");

                Map(t => t.OcoRelatedOrderId).Index(47).Name("OcoRelatedOrderId").Optional();

                Map(t => t.MarginToBalanceConversionRate).Index(48).Name("MarginToBalanceConversionRate").Optional();
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

        private sealed class ProtoTimestampTypeConverter : ITypeConverter
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

        private sealed class UtcTicksTypeConverter : ITypeConverter
        {
            public static bool TryReadTime(string text, out UtcTicks timeTicks)
            {
                timeTicks = UtcTicks.Default;
                if (!DateTime.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var time))
                    return false;

                timeTicks = new UtcTicks(time);
                return true;
            }

            public object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
            {
                if (string.IsNullOrEmpty(text))
                    return 0;

                var time = DateTime.Parse(text, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);
                return new UtcTicks(time);
            }

            public string ConvertToString(object value, IWriterRow row, MemberMapData memberMapData)
            {
                return value is UtcTicks time ? InvariantFormat.CsvFormat(time.ToUtcDateTime()) : string.Empty;
            }
        }

        private sealed class RoundedDoubleConverter : ITypeConverter
        {
            private readonly string _doubleFormat;
            private readonly double _bigValue;

            public RoundedDoubleConverter(int precision)
            {
                if (precision > 0 && precision <= 14)
                {
                    _doubleFormat = $"0.{new string('#', precision)}";
                    // values can be too big for requested precision after decimal point
                    // better switch to general format at this point
                    _bigValue = Math.Pow(10, 14 - precision);
                }
                else
                {
                    _doubleFormat = "G";
                    _bigValue = double.MaxValue;
                }
            }


            public object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
            {
                return double.TryParse(text, out var result) ? result : 0.0;
            }

            public string ConvertToString(object value, IWriterRow row, MemberMapData memberMapData)
            {
                if (value is not double dValue)
                    return string.Empty;

                return dValue < _bigValue ? dValue.ToString(_doubleFormat) : dValue.ToString("G");
            }
        }

        private sealed class NullUInt32HexConverter : ITypeConverter
        {
            public object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
            {
                return uint.TryParse(text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var result) ? result : default(uint?);
            }

            public string ConvertToString(object value, IWriterRow row, MemberMapData memberMapData)
            {
                return value is uint uintValue ? uintValue.ToString("x8") : string.Empty;
            }
        }
    }
}