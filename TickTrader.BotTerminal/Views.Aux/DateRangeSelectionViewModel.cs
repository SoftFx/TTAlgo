using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Common.Lib;

namespace TickTrader.BotTerminal
{
    internal class DateRangeSelectionViewModel : ObservableObject
    {
        private DateTime _rangeFrom;
        private DateTime _rangeTo;

        public double MaxRangeDouble => Max.GetAbsoluteDay();
        public double MinRangeDouble => Min.GetAbsoluteDay();

        public DateTime Min { get; private set; }
        public DateTime Max { get; private set; }

        public double RangeFromDouble
        {
            get => ToDouble(From) ?? 0;
            set { From = DateTimeExt.FromTotalDays((int)value); }
        }

        public double RangeToDouble
        {
            get => ToDouble(To) ?? 100;
            set { To = DateTimeExt.FromTotalDays((int)value); }
        }

        public DateTime From
        {
            get => _rangeFrom;
            set
            {
                if (_rangeFrom != value)
                {
                    _rangeFrom = value;
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
                    _rangeTo = value;
                    NotifyOfPropertyChange(nameof(To));
                    NotifyOfPropertyChange(nameof(RangeToDouble));
                }
            }
        }

        public void Reset()
        {
            Min = DateTime.MinValue;
            Max = DateTime.MaxValue;
            From = Min;
            To = Max;
        }

        public void UpdateBoundaries(DateTime min, DateTime max)
        {
            Min = min;
            Max = max;
            NotifyOfPropertyChange(nameof(Min));
            NotifyOfPropertyChange(nameof(Max));
            NotifyOfPropertyChange(nameof(MinRangeDouble));
            NotifyOfPropertyChange(nameof(MaxRangeDouble));
            From = min;
            To = max;
        }

        private double? ToDouble(DateTime? val)
        {
            if (val == null)
                return null;
            return val.Value.GetAbsoluteDay();
        }
    }
}
