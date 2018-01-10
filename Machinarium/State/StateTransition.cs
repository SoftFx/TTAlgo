using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Machinarium.State
{
    internal class StateTransition<T>
    {
        private Action transitionAction;

        [System.Diagnostics.DebuggerHidden]
        public StateTransition(T from, T to, Action action)
        {
            FromState = from;
            ToState = to;
            transitionAction = action;
        }

        public T FromState { get; private set; }
        public T ToState { get; private set; }

        [System.Diagnostics.DebuggerHidden]
        public void FireAction()
        {
            if (transitionAction != null)
                transitionAction();
        }
    }

    internal class StateEventTransition<T> : StateTransition<T>
    {
        private object eventId;
        private Func<bool> condition;

        [System.Diagnostics.DebuggerHidden]
        public StateEventTransition(T from, T to, object eventId, Func<bool> condition,  Action action)
            : base(from, to, action)
        {
            this.eventId = eventId;
            this.condition = condition;
        }

        public object EventId { get { return eventId; } }
        public bool IsConditional => condition != null;

        [System.Diagnostics.DebuggerHidden]
        public bool CheckCondition()
        {
            return condition?.Invoke() ?? true;
        }
    }

    internal class StateConditionalTransition<T> : StateTransition<T>
    {
        private Func<bool> condition;

        [System.Diagnostics.DebuggerHidden]
        public StateConditionalTransition(T from, T to, Func<bool> condition, Action action)
            : base(from, to, action)
        {
            this.condition = condition;
        }

        [System.Diagnostics.DebuggerHidden]
        public bool CheckCondition()
        {
            return condition();
        }
    }
}
