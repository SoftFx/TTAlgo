using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace TickTrader.BotTerminal.Lib
{
    public class StateMachine<T> : IStateProvider<T>
        where T : IComparable
    {
        private IStateMachineSync _lock; // new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        private Dictionary<T, StateDescriptor<T>> descriptors = new Dictionary<T, StateDescriptor<T>>();

        [System.Diagnostics.DebuggerStepThrough]
        public StateMachine(T initialState = default(T)) : this(null, initialState)
        {
        }

        [System.Diagnostics.DebuggerStepThrough]
        public StateMachine(IStateMachineSync syncContext, T initialState = default(T))
        {
            Current = initialState;

            if (syncContext == null)
                _lock = new MonitorStateMachineSync();
            else
                _lock = syncContext;
        }

        public T Current { get; private set; }
        public event Action<T, T> StateChanged = delegate { };
        public event Action<object> EventFired = delegate { };
        public IStateMachineSync SyncContext { get { return _lock; } }

        [System.Diagnostics.DebuggerStepThrough]
        public void AddTransition(T state, object eventId, Action trAction)
        {
            _lock.Synchronized(() =>
            {
                if (trAction == null)
                    throw new ArgumentNullException("trAction");

                StateDescriptor<T> descriptor = GetOrAddDescriptor(state);
                descriptor.AddTransition(new StateEventTransition<T>(state, state, eventId, trAction));
            });
        }

        [System.Diagnostics.DebuggerStepThrough]
        public void AddTransition(T from, object eventId, T to, Action trAction = null)
        {
            _lock.Synchronized(() =>
            {
                StateDescriptor<T> descriptor = GetOrAddDescriptor(from);
                descriptor.AddTransition(new StateEventTransition<T>(from, to, eventId, trAction));
            });
        }

        [System.Diagnostics.DebuggerStepThrough]
        public void AddTransition(T from, Func<bool> condition, T to, Action trAction = null)
        {
            if (condition == null)
                throw new ArgumentNullException("condition");

            _lock.Synchronized(() =>
            {
                StateDescriptor<T> descriptor = GetOrAddDescriptor(from);
                descriptor.AddTransition(new StateConditionalTransition<T>(from, to, condition, trAction));
            });
        }

        [System.Diagnostics.DebuggerStepThrough]
        public void AddScheduledEvent(T state, object eventId, int timeInterval)
        {
            _lock.Synchronized(() =>
            {
                StateDescriptor<T> descriptor = GetOrAddDescriptor(state);
                descriptor.AddScheduledEvent(new TimeEventDescriptor(eventId, timeInterval));
            });
        }

        [System.Diagnostics.DebuggerStepThrough]
        public void OnEnter(T state, Action action)
        {
            if (action == null)
                throw new ArgumentNullException("action");

            _lock.Synchronized(() =>
            {
                StateDescriptor<T> descriptor = GetOrAddDescriptor(state);
                descriptor.AddEnterAction(action);
            });
        }

        [System.Diagnostics.DebuggerStepThrough]
        public void OnExit(T state, Action action)
        {
            if (action == null)
                throw new ArgumentNullException("action");

            _lock.Synchronized(() =>
            {
                StateDescriptor<T> descriptor = GetOrAddDescriptor(state);
                descriptor.AddExitAction(action);
            });
        }

        [System.Diagnostics.DebuggerStepThrough]
        public void ModifyConditions(Action modifyAction)
        {
            _lock.Synchronized(() =>
            {
                modifyAction();
                CheckConditions();
            });
        }

        [System.Diagnostics.DebuggerStepThrough]
        public void PushEvent(object eventId)
        {
            _lock.Synchronized(() => PushEventInternal(eventId));
        }

        [System.Diagnostics.DebuggerStepThrough]
        private void PushEventInternal(object eventId)
        {
            EventFired(eventId);

            StateDescriptor<T> currentDescriptor = FindDescriptor(Current);
            if (currentDescriptor != null)
            {
                StateTransition<T> transition = currentDescriptor.OnEvent(eventId);
                if (transition != null)
                    ChangeState(currentDescriptor, transition);
            }
        }

        [System.Diagnostics.DebuggerStepThrough]
        private StateDescriptor<T> GetOrAddDescriptor(T state)
        {
            StateDescriptor<T> descriptor;
            if (!descriptors.TryGetValue(state, out descriptor))
            {
                descriptor = new StateDescriptor<T>(state);
                descriptors.Add(state, descriptor);
            }
            return descriptor;
        }

        [System.Diagnostics.DebuggerStepThrough]
        private StateDescriptor<T> FindDescriptor(T state)
        {
            StateDescriptor<T> currentDescriptor;
            descriptors.TryGetValue(Current, out currentDescriptor);
            return currentDescriptor;
        }

        [System.Diagnostics.DebuggerStepThrough]
        private void CheckConditions()
        {
            StateDescriptor<T> currentDescriptor = FindDescriptor(Current);
            if (currentDescriptor != null)
            {
                StateTransition<T> transition = currentDescriptor.CheckConditions();
                if (transition != null)
                    ChangeState(currentDescriptor, transition);
            }
        }

        [System.Diagnostics.DebuggerStepThrough]
        private void ChangeState(StateDescriptor<T> currentDescriptor, StateTransition<T> transition)
        {
            T oldState = Current;

            if (!Current.Equals(transition.ToState))
            {
                currentDescriptor.OnExit();

                Current = transition.ToState;

                CancelScheduledEvents();

                StateDescriptor<T> newStateDesciptor = FindDescriptor(Current);
                if (newStateDesciptor != null)
                    newStateDesciptor.OnEnter();

                StateChanged(oldState, Current);

                ScheduleEventsForState(newStateDesciptor);
            }

            transition.FireAction();

            CheckConditions();
        }

        [System.Diagnostics.DebuggerStepThrough]
        public Task AsyncWait(T stateToWait)
        {
            Task waitTask = null;
            _lock.Synchronized(() => waitTask = AsyncWaitInternal(stateToWait));
            return waitTask;
        }

        [System.Diagnostics.DebuggerStepThrough]
        private Task AsyncWaitInternal(T stateToWait)
        {
            if (Current.Equals(stateToWait))
                return CompletedTask.Default;

            StateDescriptor<T> descriptor = GetOrAddDescriptor(stateToWait);
            return descriptor.AsyncWait();
        }

        [System.Diagnostics.DebuggerStepThrough]
        public Task PushEventAndAsyncWait(object eventId, T stateToWait)
        {
            Task waitTask = null;
            _lock.Synchronized(() =>
            {
                PushEventInternal(eventId);
                waitTask = AsyncWaitInternal(stateToWait);
            });
            return waitTask;
        }

        [System.Diagnostics.DebuggerStepThrough]
        public void Wait(T stateToWait)
        {
            AsyncWait(stateToWait).Wait();
        }

        #region Event Scheduler

        private List<TimeEvent> scheduledEvents;

        [System.Diagnostics.DebuggerStepThrough]
        private void ScheduleEventsForState(StateDescriptor<T> newState)
        {
            IEnumerable<TimeEventDescriptor> eventsToSchedule = newState.ListScheduledEvents();
            if (eventsToSchedule != null)
            {
                foreach (TimeEventDescriptor eventDescriptor in eventsToSchedule)
                {
                    if (scheduledEvents == null)
                        scheduledEvents = new List<TimeEvent>();
                    scheduledEvents.Add(new TimeEvent(eventDescriptor, OnEventElapsed));
                }
            }
        }

        [System.Diagnostics.DebuggerStepThrough]
        private void CancelScheduledEvents()
        {
            if (scheduledEvents != null)
            {
                foreach (TimeEvent tmrEvent in scheduledEvents)
                    tmrEvent.Dispose();

                scheduledEvents.Clear();
            }
        }

        [System.Diagnostics.DebuggerStepThrough]
        private void OnEventElapsed(TimeEvent eventObj)
        {
            _lock.Synchronized(() =>
            {
                if (scheduledEvents.Contains(eventObj)) // avoid possible concurrency
                    PushEvent(eventObj.EventId);
            });
        }

        #endregion
    }

    public interface IStateProvider<T>
    {
        T Current { get; }
        event Action<T, T> StateChanged;
    }

}
