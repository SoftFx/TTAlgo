using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machinarium.Var
{
    public static class VarExtentions
    {
        public static IDisposable WhenTrue(this BoolVar condition, Action action)
        {
            return new Event(condition, action, null);
        }

        public static IDisposable WhenFalse(this BoolVar condition, Action action)
        {
            return new Event(condition, null, action);
        }

        public static IDisposable WhenTrueOrFalse(this BoolVar condition, Action whenTrue, Action whenFalse)
        {
            return new Event(condition, whenTrue, whenFalse);
        }

        internal class Event : IDisposable
        {
            private BoolVar _condition;
            private Action _onTrue;
            private Action _onFalse;

            public Event(BoolVar condition, Action onTrue, Action onFalse)
            {
                _condition = condition;
                _onTrue = onTrue;
                _onFalse = onFalse;
                _condition.Changed += Condition_Changed;
                VarContext.AddIfContextExist(this);

                Condition_Changed(false);
            }

            private void Condition_Changed(bool disposed)
            {
                if (disposed)
                    Dispose();
                else if (_condition.Value)
                    _onTrue?.Invoke();
                else
                    _onFalse?.Invoke();
            }

            public void Dispose()
            {
                _condition.Changed -= Condition_Changed;
            }
        }
    }
}
