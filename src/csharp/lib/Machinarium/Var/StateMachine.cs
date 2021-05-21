using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machinarium.Var
{
    public class StateMachine<T> : StateMachineBase<T>
        where T : IComparable<T>, IEquatable<T>
    {
        private StateDescriptor _currentStateDescriptor;
        private Dictionary<T, StateDescriptor> _descriptors = new Dictionary<T, StateDescriptor>();

        public StateMachine(T initialState = default(T))
            : base(initialState)
        {
            _currentStateDescriptor = GetDescriptor(initialState);
        }

        public void AddTransition(T fromState, object eventId, T toState)
        {
            var fromDescriptor = GetDescriptor(fromState);
            var ToDescriptor = GetDescriptor(toState);
            fromDescriptor.Add(eventId, ToDescriptor);
        }

        protected override T OnEvent(T currentState, object eventId)
        {
            var newDescriptor = _currentStateDescriptor.GetTransition(eventId);
            if (newDescriptor != null)
            {
                _currentStateDescriptor = newDescriptor;
                return newDescriptor.State;
            }
            return currentState;
        }

        private StateDescriptor GetDescriptor(T state)
        {
            StateDescriptor d;
            if (!_descriptors.TryGetValue(state, out d))
            {
                d = new StateDescriptor(state);
                _descriptors.Add(state, d);
            }
            return d;
        }

        private class StateDescriptor : Dictionary<object, StateDescriptor>
        {
            public StateDescriptor(T state)
            {
                State = state;
            }

            public T State { get; }

            public StateDescriptor GetTransition(object eventId)
            {
                StateDescriptor d;
                TryGetValue(eventId, out d);
                return d;
            }
        }
    }
}
