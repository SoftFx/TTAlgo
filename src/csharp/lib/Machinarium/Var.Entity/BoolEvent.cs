using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machinarium.Var
{
    internal class BoolEvent : IDisposable
    {
        private BoolVar _condition;
        private Action _onTrue;
        private Action _onFalse;

        public BoolEvent(BoolVar condition, Action onTrue, Action onFalse)
        {
            _condition = condition;
            _onTrue = onTrue;
            _onFalse = onFalse;

            _condition.Changed += Condition_Changed;
            Condition_Changed();
        }

        private void Condition_Changed()
        {
            if (_condition.Value)
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
