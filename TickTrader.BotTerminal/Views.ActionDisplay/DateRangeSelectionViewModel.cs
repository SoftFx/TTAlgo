﻿using Caliburn.Micro;
using Machinarium.Var;
using System;
using TickTrader.Algo.Core.Lib;

namespace TickTrader.BotTerminal
{
    internal class DateRangeSelectionViewModel : PropertyChangedBase
    {
        private DateTime _rangeFrom;
        private DateTime _rangeTo;

        public DateRangeSelectionViewModel(bool dateOnly = true)
        {
            DateOnlyMode = dateOnly;

            MaxRangeDouble = new DoubleProperty();
            MinRangeDouble = new DoubleProperty();
            Min = new Property<DateTime>();
            Max = new Property<DateTime>();
        }

        public bool DateOnlyMode { get; }
        public DoubleProperty MaxRangeDouble { get; }
        public DoubleProperty MinRangeDouble { get; }
        public Property<DateTime> Min { get; }
        public Property<DateTime> Max { get; }

        public double RangeFromDouble
        {
            get => ToRangeDouble(From);
            set { From = FromRangeDouble(value); }
        }

        public double RangeToDouble
        {
            get => ToRangeDouble(To);
            set { To = FromRangeDouble(value); }
        }

        public DateTime From
        {
            get => _rangeFrom;
            set
            {
                if (_rangeFrom != value)
                {
                    if (value > Max.Value)
                        value = Max.Value;

                    if (value < Min.Value)
                        value = Min.Value;

                    if (To < value)
                        To = value;

                    _rangeFrom = DateTime.SpecifyKind(value, DateTimeKind.Utc);
                    NotifyOfPropertyChange(nameof(From));
                    NotifyOfPropertyChange(nameof(RangeFromDouble));
                }
            }
        }

        public DateTime To
        {
            get { return _rangeTo; }
            set
            {
                if (_rangeTo != value)
                {
                    if (value > Max.Value)
                        value = Max.Value;

                    if (value < Min.Value)
                        value = Min.Value;

                    if (From > value)
                        From = value;

                    _rangeTo = DateTime.SpecifyKind(value, DateTimeKind.Utc);
                    NotifyOfPropertyChange(nameof(To));
                    NotifyOfPropertyChange(nameof(RangeToDouble));
                }
            }
        }

        public void Reset()
        {
            UpdateBoundaries(DateTime.MinValue, DateTime.MaxValue);
            ResetSelectedRange();
        }

        public void ResetSelectedRange()
        {
            From = Min.Value;
            To = Max.Value;
        }

        public void UpdateBoundaries(DateTime min, DateTime max)
        {
            Min.Value = DateTime.MinValue;
            Max.Value = DateTime.MaxValue;

            if (From < min)
                From = min;

            if (To > max)
                To = max;

            Min.Value = min;
            Max.Value = max;

            MinRangeDouble.Value = ToRangeDouble(min);
            MaxRangeDouble.Value = ToRangeDouble(max);
        }

        private double ToRangeDouble(DateTime val)
        {
            return val.GetAbsoluteDay();
        }

        private DateTime FromRangeDouble(double val)
        {
            return DateTimeExtensions.FromTotalDays((int)val);
        }
    }
}
