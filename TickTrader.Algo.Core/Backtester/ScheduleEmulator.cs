using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Core
{
    internal class ScheduleEmulator : ITimeEventSeries
    {
        private List<Schedule> _schedules = new List<Schedule>();
        private Schedule _nextEventSchedule;

        public DateTime NextOccurrance => _nextEventSchedule?.NextOccurrence ?? DateTime.MaxValue;
        public bool HasJobs => _schedules.Count > 0;

        public void Init(DateTime currentTime)
        {
            foreach (var schedule in _schedules)
                schedule.Init(currentTime);

            FindNext();
        }

        public TimeEvent Take()
        {
            var ev = _nextEventSchedule.Take();
            FindNext();
            return ev;
        }

        private void FindNext()
        {
            if (_schedules.Count > 0)
                _nextEventSchedule = _schedules.MinBy(s => s.NextOccurrence);
            else
                _nextEventSchedule = null;
        }

        public void AddDailyJob(Action<PluginBuilder> job, int hour, int minute)
        {
            _schedules.Add(new DailySchedule(job, hour, minute));
        }

        private abstract class Schedule
        {
            public DateTime NextOccurrence { get; protected set; }

            public abstract void Init(DateTime currentTime);
            public abstract TimeEvent Take();
        }

        private class DailySchedule : Schedule
        {
            private Action<PluginBuilder> _job;
            private TimeSpan _timeShift;

            public DailySchedule(Action<PluginBuilder> job, int hour, int minute)
            {
                _job = job;
                _timeShift = new TimeSpan(hour, minute, 0);
            }

            public override void Init(DateTime currentTime)
            {
                var date = currentTime.Date;

                if (currentTime == date)
                    NextOccurrence = date + _timeShift;
                else
                    NextOccurrence = date + TimeSpan.FromDays(1) + _timeShift;
            }

            public override TimeEvent Take()
            {
                var occurred = NextOccurrence;
                NextOccurrence += TimeSpan.FromDays(1) + _timeShift;

                return new TimeEvent(occurred, true, _job);
            }
        }
    }
}
