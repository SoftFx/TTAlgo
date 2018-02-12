using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machinarium.Var
{
    public abstract class StateMachineBase<T> : EntityBase
        where T : IComparable<T>, IEquatable<T>
    {
        private Property<T> _stateProperty;

        public StateMachineBase(T initialState = default(T))
        {
            _stateProperty = AddProperty(initialState);
        }

        public Var<T> State => _stateProperty.Var;

        public virtual void OnEnter(T state, Action handler)
        {
            TriggerOn(State == state, handler);
        }

        public virtual void OnExit(T state, Action handler)
        {
            TriggerOn(State != state, handler);
        }

        protected abstract T OnEvent(T currentState, object eventId);

        public void PushEvent(object eventId)
        {
            State.Value = OnEvent(State.Value, eventId);
        }

        public void AddInput(BoolVar varInput, object trueEvent)
        {
            TriggerOn(varInput, () => PushEvent(trueEvent));
        }

        public void AddInput(BoolVar varInput, object trueEvent, object falseEvent)
        {
            TriggerOn(varInput, () => PushEvent(trueEvent), () => PushEvent(falseEvent));
        }
    }
}
