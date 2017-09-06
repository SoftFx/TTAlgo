using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machinarium.Var
{
    public abstract class StateMachineBase<T>
        where T : IComparable<T>, IEquatable<T>
    {
        public StateMachineBase(T initialState = default(T))
        {
            State = CmpVar.New(initialState);
        }

        public CmpVar<T> State { get; }

        public virtual void OnEnter(T state, Action handler)
        {
            (State == state).WhenTrue(handler);
        }

        public virtual void OnExit(T state, Action handler)
        {
            (State == state).WhenFalse(handler);
        }

        protected abstract T OnEvent(T currentState, object eventId);

        public void PushEvent(object eventId)
        {
            State.Value = OnEvent(State.Value, eventId);
        }

        public void AddInput(BoolVar varInput, object trueEvent)
        {
            varInput.WhenTrue(() => PushEvent(trueEvent));
        }

        public void AddInput(BoolVar varInput, object trueEvent, object falseEvent)
        {
            varInput.WhenTrueOrFalse(() => PushEvent(trueEvent), ()=> PushEvent(falseEvent));
        }
    }
}
