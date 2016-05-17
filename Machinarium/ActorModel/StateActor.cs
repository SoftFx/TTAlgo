using Machinarium.State;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Machinarium.ActorModel
{
    public class StateActor<T> : LameActorCore
        where T : IComparable
    {
        private static readonly Task CompletedTask = Task.FromResult<object>(null);

        private LinkedList<StateAwaiter> stateWaiters = new LinkedList<StateAwaiter>();
        private Dictionary<T, StateDescriptor<T>> descriptors = new Dictionary<T, StateDescriptor<T>>();

        [System.Diagnostics.DebuggerHidden]
        public StateActor(T initialState = default(T))
        {
        }

        public T State { get; private set; }
        public Action<T, T> StateChanged { get; set; }
        public Action<object> EventFired { get; set; }

        [System.Diagnostics.DebuggerHidden]
        public void AddTransition(T state, object eventId, Action trAction)
        {
            if (trAction == null)
                throw new ArgumentNullException("trAction");

            StateDescriptor<T> descriptor = GetOrAddDescriptor(state);
            descriptor.AddTransition(new StateEventTransition<T>(state, state, eventId, trAction));
        }

        [System.Diagnostics.DebuggerHidden]
        public void AddTransition(T from, object eventId, T to, Action trAction = null)
        {
            StateDescriptor<T> descriptor = GetOrAddDescriptor(from);
            descriptor.AddTransition(new StateEventTransition<T>(from, to, eventId, trAction));
        }

        [System.Diagnostics.DebuggerHidden]
        public void AddTransition(T from, Func<bool> condition, T to, Action trAction = null)
        {
            if (condition == null)
                throw new ArgumentNullException("condition");

            StateDescriptor<T> descriptor = GetOrAddDescriptor(from);
            descriptor.AddTransition(new StateConditionalTransition<T>(from, to, condition, trAction));
        }

        //[System.Diagnostics.DebuggerHidden]
        //public void AddScheduledEvent(T state, object eventId, int timeInterval)
        //{
        //    _lock.Synchronized(() =>
        //    {
        //        StateDescriptor<T> descriptor = GetOrAddDescriptor(state);
        //        descriptor.AddScheduledEvent(new TimeEventDescriptor(eventId, timeInterval));
        //    });
        //}

        [System.Diagnostics.DebuggerHidden]
        public void OnEnter(T state, Action action)
        {
            if (action == null)
                throw new ArgumentNullException("action");

            StateDescriptor<T> descriptor = GetOrAddDescriptor(state);
            descriptor.AddEnterAction(action);
        }

        [System.Diagnostics.DebuggerHidden]
        public void OnExit(T state, Action action)
        {
            if (action == null)
                throw new ArgumentNullException("action");

            StateDescriptor<T> descriptor = GetOrAddDescriptor(state);
            descriptor.AddExitAction(action);
        }

        //[System.Diagnostics.DebuggerHidden]
        //public void ModifyConditions(Action modifyAction)
        //{
        //    modifyAction();
        //    CheckConditions();
        //}

        //[System.Diagnostics.DebuggerHidden]
        //public Task ModifyConditionsAndWait(Action modifyAction, Predicate<T> stateCondition)
        //{
        //    Task waitTask = null;
        //    _lock.Synchronized(() =>
        //    {
        //        modifyAction();
        //        CheckConditions();
        //        waitTask = AsyncWaitInternal(stateCondition);
        //    });
        //    return waitTask;
        //}

        //[System.Diagnostics.DebuggerHidden]
        //public Task ModifyConditionsAndWait(Action modifyAction, T state)
        //{
        //    return ModifyConditionsAndWait(modifyAction, s => state.Equals(s));
        //}

        [System.Diagnostics.DebuggerHidden]
        public void PushEvent(object eventId)
        {
            EventFired(eventId);

            StateDescriptor<T> currentDescriptor = FindDescriptor(State);
            if (currentDescriptor != null)
            {
                StateTransition<T> transition = currentDescriptor.OnEvent(eventId);
                if (transition != null)
                    ChangeState(currentDescriptor, transition);
            }
        }

        [System.Diagnostics.DebuggerHidden]
        protected override void AfterHandler()
        {
            CheckConditions();
        }

        [System.Diagnostics.DebuggerHidden]
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

        [System.Diagnostics.DebuggerHidden]
        private StateDescriptor<T> FindDescriptor(T state)
        {
            StateDescriptor<T> currentDescriptor;
            descriptors.TryGetValue(State, out currentDescriptor);
            return currentDescriptor;
        }

        [System.Diagnostics.DebuggerHidden]
        private void CheckConditions()
        {
            StateDescriptor<T> currentDescriptor = FindDescriptor(State);
            if (currentDescriptor != null)
            {
                StateTransition<T> transition = currentDescriptor.CheckConditions();
                if (transition != null)
                    ChangeState(currentDescriptor, transition);
            }
        }

        [System.Diagnostics.DebuggerHidden]
        private void ChangeState(StateDescriptor<T> currentDescriptor, StateTransition<T> transition)
        {
            T oldState = State;

            if (!State.Equals(transition.ToState))
            {
                currentDescriptor.OnExit();

                State = transition.ToState;

                //CancelScheduledEvents();

                StateDescriptor<T> newStateDesciptor = FindDescriptor(State);
                if (newStateDesciptor != null)
                    newStateDesciptor.OnEnter();

                ReleaseAwaiters();

                StateChanged(oldState, State);

                //ScheduleEventsForState(newStateDesciptor);
            }

            transition.FireAction();

            CheckConditions();
        }

        [System.Diagnostics.DebuggerHidden]
        public Task When(Predicate<T> condition)
        {
            AssertActor("AsyncWait");
            return AsyncWaitInternal(condition);
        }

        [System.Diagnostics.DebuggerHidden]
        public Task When(T stateToWait)
        {
            AssertActor("AsyncWait");
            return When(s => stateToWait.Equals(s));
        }

        [System.Diagnostics.DebuggerHidden]
        private Task AsyncWaitInternal(Predicate<T> stateCondition)
        {
            if (stateCondition(State))
                return CompletedTask;

            StateAwaiter waiter = new StateAwaiter(stateCondition);
            stateWaiters.AddLast(waiter);
            return waiter.Task;
        }

        //[System.Diagnostics.DebuggerHidden]
        //public Task PushEventAndWait(object eventId, Predicate<T> stateCondition)
        //{
        //    Task waitTask = null;
        //    _lock.Synchronized(() =>
        //    {
        //        PushEventInternal(eventId);
        //        waitTask = AsyncWaitInternal(stateCondition);
        //    });
        //    return waitTask;
        //}

        //[System.Diagnostics.DebuggerHidden]
        //public Task PushEventAndWait(object eventId, T stateToWait)
        //{
        //    return PushEventAndWait(eventId, s => stateToWait.Equals(s));
        //}

        //[System.Diagnostics.DebuggerHidden]
        //public void Wait(T stateToWait)
        //{
        //    AsyncWait(stateToWait).Wait();
        //}

        [System.Diagnostics.DebuggerHidden]
        private void ReleaseAwaiters()
        {
            var node = stateWaiters.First;
            while (node != null)
            {
                var nextNode = node.Next;
                if (node.Value.Condition(State))
                {
                    stateWaiters.Remove(node);
                    node.Value.Fire();
                }

                node = nextNode;
            }
        }

        [Conditional("DEBUG")]
        private void AssertActor(string methodName)
        {
            Debug.Assert(SynchronizationContext.Current == Context, "Method " + methodName + "() can be called only inside Actor context. Use Enqueue() or AsyncCall() methods to get to Actor context.");
        }

        //#region Event Scheduler

        //private List<TimeEvent> scheduledEvents;

        //[System.Diagnostics.DebuggerHidden]
        //private void ScheduleEventsForState(StateDescriptor<T> newState)
        //{
        //    IEnumerable<TimeEventDescriptor> eventsToSchedule = newState.ListScheduledEvents();
        //    if (eventsToSchedule != null)
        //    {
        //        foreach (TimeEventDescriptor eventDescriptor in eventsToSchedule)
        //        {
        //            if (scheduledEvents == null)
        //                scheduledEvents = new List<TimeEvent>();
        //            scheduledEvents.Add(new TimeEvent(eventDescriptor, OnEventElapsed));
        //        }
        //    }
        //}

        //[System.Diagnostics.DebuggerHidden]
        //private void CancelScheduledEvents()
        //{
        //    if (scheduledEvents != null)
        //    {
        //        foreach (TimeEvent tmrEvent in scheduledEvents)
        //            tmrEvent.Dispose();

        //        scheduledEvents.Clear();
        //    }
        //}

        //[System.Diagnostics.DebuggerHidden]
        //private void OnEventElapsed(TimeEvent eventObj)
        //{
        //    _lock.Synchronized(() =>
        //    {
        //        if (scheduledEvents.Contains(eventObj)) // avoid possible concurrency
        //            PushEvent(eventObj.EventId);
        //    });
        //}

        //#endregion

        private class StateAwaiter
        {
            public StateAwaiter(Predicate<T> stateCondition)
            {
                this.Condition = stateCondition;
            }

            private TaskCompletionSource<object> scr = new TaskCompletionSource<object>();
            public Task Task { get { return scr.Task; } }
            public Predicate<T> Condition { get; private set; }

            public void Fire()
            {
                scr.SetResult(null);
            }
        }
    }
}
