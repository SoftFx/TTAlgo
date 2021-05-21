using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machinarium.State
{
    internal class StateDescriptor<T>
    {
        private List<StateEventTransition<T>> eventTransitions = new List<StateEventTransition<T>>();
        private List<StateConditionalTransition<T>> cdTransitions = new List<StateConditionalTransition<T>>();
        private List<TimeEventDescriptor> timeEvents;
        private Action eneterAction;
        private Action exitAction;
        private T state;
        private LinkedList<TaskCompletionSource<object>> stateWaiters = new LinkedList<TaskCompletionSource<object>>();

        [System.Diagnostics.DebuggerHidden]
        public StateDescriptor(T state)
        {
            this.state = state;
        }

        [System.Diagnostics.DebuggerHidden]
        public void AddEnterAction(Action action)
        {
            if (eneterAction != null)
                throw new InvalidOperationException("Exit action has been already assigned for state " + state);
            eneterAction = action;
        }

        [System.Diagnostics.DebuggerHidden]
        public void AddExitAction(Action action)
        {
            if (exitAction != null)
                throw new InvalidOperationException("Exit action has been already assigned for state " + state);
            exitAction = action;
        }

        [System.Diagnostics.DebuggerHidden]
        public void AddTransition(StateConditionalTransition<T> transition)
        {
            cdTransitions.Add(transition);
        }

        [System.Diagnostics.DebuggerHidden]
        public void AddTransition(StateEventTransition<T> transition)
        {
            if (!transition.IsConditional && eventTransitions.Any(t => t.EventId.Equals(transition.EventId) && !t.IsConditional))
                throw new InvalidOperationException("Duplicate non-conditional transition for event " + transition.EventId);

            eventTransitions.Add(transition);
        }

        [System.Diagnostics.DebuggerHidden]
        public void AddScheduledEvent(TimeEventDescriptor eventDescriptor)
        {
            if (timeEvents == null)
                timeEvents = new List<TimeEventDescriptor>();

            timeEvents.Add(eventDescriptor);
        }

        [System.Diagnostics.DebuggerHidden]
        public IEnumerable<TimeEventDescriptor> ListScheduledEvents()
        {
            return timeEvents;
        }

        [System.Diagnostics.DebuggerHidden]
        public StateTransition<T> OnEvent(object eventId)
        {
            return eventTransitions.FirstOrDefault(t => t.EventId.Equals(eventId) && t.CheckCondition());
        }

        [System.Diagnostics.DebuggerHidden]
        public StateTransition<T> CheckConditions()
        {
            return cdTransitions.FirstOrDefault(t => t.CheckCondition());
        }

        [System.Diagnostics.DebuggerHidden]
        public void OnEnter()
        {
            try
            {
                if (eneterAction != null)
                    eneterAction();
            }
            finally
            {
                // release all waiters
                while (stateWaiters.Count > 0)
                {
                    TaskCompletionSource<object> eventCopy = stateWaiters.First.Value;
                    stateWaiters.RemoveFirst();
                    Task.Factory.StartNew(() => eventCopy.TrySetResult(null));
                }
            }
        }

        [System.Diagnostics.DebuggerHidden]
        public void OnExit()
        {
            if (exitAction != null)
                exitAction();
        }

        [System.Diagnostics.DebuggerHidden]
        public Task AsyncWait()
        {
            TaskCompletionSource<object> src = new TaskCompletionSource<object>();
            stateWaiters.AddLast(src);
            return src.Task;
        }
    }
}
