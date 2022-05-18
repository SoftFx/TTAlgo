//using System;
//using SciChart.Charting.Visuals.Axes.LabelProviders;
//using TickTrader.Algo.Domain;

//namespace TickTrader.BotTerminal
//{
//    public class StockLabelProvider : LabelProviderBase
//    {
//        private bool _firstLabel;
//        private DateTime _prevValue;
//        //private LabelLevels _level;

//        public Feed.Types.Timeframe Timeframe { get; set; } = Feed.Types.Timeframe.M1;

//        public override string FormatCursorLabel(IComparable dataValue)
//        {
//            return ((DateTime)dataValue).ToString(FullDateTimeFormat);
//        }

//        public override void OnBeginAxisDraw()
//        {
//            _firstLabel = true;

//            base.OnBeginAxisDraw();
//        }

//        public override string FormatLabel(IComparable dataValue)
//        {
//            if (!(dataValue is DateTime))
//                return string.Empty;

//            var dateTime = (DateTime)dataValue;
//            var format = _firstLabel ? FormatFirstLabel() : FormarNextLabel(dateTime);
//            var label = dateTime.ToString(format);
//            _firstLabel = false;
//            _prevValue = dateTime;
//            return label;
//        }

//        private string FormatFirstLabel()
//        {
//            switch (Timeframe)
//            {
//                case Feed.Types.Timeframe.MN: return MonthWithYearFormat;
//                case Feed.Types.Timeframe.W:
//                case Feed.Types.Timeframe.D: return DayWithYearFormat;
//                case Feed.Types.Timeframe.H4:
//                case Feed.Types.Timeframe.H1:
//                case Feed.Types.Timeframe.M30:
//                case Feed.Types.Timeframe.M15:
//                case Feed.Types.Timeframe.M5:
//                case Feed.Types.Timeframe.M1: //return DayWithTimeFormat;
//                case Feed.Types.Timeframe.S10:
//                case Feed.Types.Timeframe.S1:
//                case Feed.Types.Timeframe.Ticks:
//                case Feed.Types.Timeframe.TicksLevel2: return DayWithTimeFormat;
//                default: return FullDateTimeFormat;
//            }
//        }

//        private string FormarNextLabel(DateTime newVal)
//        {
//            switch (Timeframe)
//            {
//                case Feed.Types.Timeframe.MN:
//                    {
//                        if (newVal.Year != _prevValue.Year)
//                            return MonthWithYearFormat;
//                        else
//                            return MonthFormat;
//                    }
//                case Feed.Types.Timeframe.W:
//                case Feed.Types.Timeframe.D:
//                    {
//                        if (newVal.Year != _prevValue.Year)
//                            return DayWithYearFormat;
//                        else
//                            return DayOnlyFormat;
//                    }
//                case Feed.Types.Timeframe.H4:
//                case Feed.Types.Timeframe.H1:
//                case Feed.Types.Timeframe.M30:
//                case Feed.Types.Timeframe.M15:
//                case Feed.Types.Timeframe.M5:
//                case Feed.Types.Timeframe.M1:
//                case Feed.Types.Timeframe.S10:
//                case Feed.Types.Timeframe.S1:
//                case Feed.Types.Timeframe.Ticks:
//                case Feed.Types.Timeframe.TicksLevel2:
//                    {
//                        if (newVal.Day != _prevValue.Day)
//                            return DayWithTimeFormat;
//                        else
//                            return TimeOnlyFormat;
//                    }
//                //case TimeFrames.S10:
//                //case TimeFrames.S1:
//                //case TimeFrames.Ticks:
//                //case TimeFrames.TicksLevel2:
//                //{
//                //    if (newVal.Day != _prevValue.Day)
//                //        return DayWithTimeWithSecondsFormat;
//                //    else if (newVal.Minute != _prevValue.Minute)
//                //    {
//                //        if (newVal.Second == 0)
//                //            return TimeOnlyFormat;
//                //        else
//                //            return TimeWithSecondsFormat;
//                //    }
//                //    else
//                //        return SecondsOnlyFormat;
//                //}
//                default: return FullDateTimeFormat;
//            }
//        }

//        #region Formats

//        private const string FullDateTimeFormat = "d MMM yyyy, HH:mm:ss";
//        private const string DateWithTimeFormat = "d MMM yyyy, HH:mm";
//        private const string TimeWithSecondsFormat = "HH:mm:ss";
//        private const string TimeOnlyFormat = "HH:mm";
//        private const string DayWithTimeFormat = "d MMM, HH:mm";
//        private const string DayWithTimeWithSecondsFormat = "d MMM, HH:mm:ss";
//        private const string DayOnlyFormat = "d MMM";
//        private const string DayWithYearFormat = "d MMM yyyy";
//        private const string MonthFormat = "MMM";
//        private const string MonthWithYearFormat = "MMM yyyy";
//        private const string YearOnlyFormat = "yyyy";
//        //private const string MinutesWithSecondsFormat = "mm\\m ss\\s";
//        private const string SecondsOnlyFormat = "ss\\s";

//        #endregion
//    }
//}
