using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateMachinarium
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

        public StateDescriptor(T state)
        {
            this.state = state;
        }

        public void AddEnterAction(Action action)
        {
            if (eneterAction != null)
                throw new InvalidOperationException("Exit action has been already assigned for state " + state);
            eneterAction = action;
        }

        public void AddExitAction(Action action)
        {
            if (exitAction != null)
                throw new InvalidOperationException("Exit action has been already assigned for state " + state);
            exitAction = action;
        }

        public void AddTransition(StateConditionalTransition<T> transition)
        {
            //if (cdTransition != null)
            //    throw new InvalidOperationException("Conditional transition has been already assigned for state " + state
            //        + ". Only one conditional transition per state is allowed.");
            cdTransitions.Add(transition);
        }

        public void AddTransition(StateEventTransition<T> transition)
        {
            if (eventTransitions.Any(t => t.EventId.Equals(transition.EventId)))
                throw new InvalidOperationException("Duplicate transition for event " + transition.EventId);

            eventTransitions.Add(transition);
        }

        public void AddScheduledEvent(TimeEventDescriptor eventDescriptor)
        {
            if (timeEvents == null)
                timeEvents = new List<TimeEventDescriptor>();

            timeEvents.Add(eventDescriptor);
        }

        public IEnumerable<TimeEventDescriptor> ListScheduledEvents()
        {
            return timeEvents;
        }

        public StateTransition<T> OnEvent(object eventId)
        {
            return eventTransitions.FirstOrDefault(t => t.EventId.Equals(eventId));
        }

        public StateTransition<T> CheckConditions()
        {
            return cdTransitions.FirstOrDefault(t => t.CheckCondition());
        }

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

        public void OnExit()
        {
            if (exitAction != null)
                exitAction();
        }

        public Task AsyncWait()
        {
            TaskCompletionSource<object> src = new TaskCompletionSource<object>();
            stateWaiters.AddLast(src);
            return src.Task;
        }
    }
}
