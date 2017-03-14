﻿using System;
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
            this.FromState = from;
            this.ToState = to;
            this.transitionAction = action;
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

        [System.Diagnostics.DebuggerHidden]
        public StateEventTransition(T from, T to, object eventId, Action action)
            : base(from, to, action)
        {
            this.eventId = eventId;
        }

        public object EventId { get { return eventId; } }
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