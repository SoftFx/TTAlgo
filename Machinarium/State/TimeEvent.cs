using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Machinarium.State
{
    internal class TimeEvent : IDisposable
    {
        private System.Threading.Timer timer;
        private Action<TimeEvent> handler;

        public TimeEvent(TimeEventDescriptor descriptor, Action<TimeEvent> elapsedHandler)
        {
            this.EventId = descriptor.EventId;
            this.handler = elapsedHandler;
            this.timer = new Timer(state => elapsedHandler(this), null, descriptor.TimeInterval, descriptor.TimeInterval);
        }

        public object EventId { get; private set; }

        public void Dispose()
        {
            timer.Dispose();
        }
    }

    internal class TimeEventDescriptor
    {
        public TimeEventDescriptor(object eventId, int timeInterval)
        {
            this.EventId = eventId;
            this.TimeInterval = timeInterval;
        }

        public object EventId { get; private set; }
        public int TimeInterval { get; private set; }
    }
}
