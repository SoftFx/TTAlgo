using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machinarium.Var
{
    internal class ChangeEvent<T> : IDisposable
    {
        private Var<T> _var;
        private Action<VarChangeEventArgs<T>> _handler;
        private T _valCahche;

        public ChangeEvent(Var<T> var, Action<VarChangeEventArgs<T>> handler)
        {
            _var = var;
            _handler = handler;

            _valCahche = _var.Value;
            _var.Changed += VarChanged;
            VarChanged();
        }

        private void VarChanged()
        {
            var oldVal = _valCahche;
            _valCahche = _var.Value;
            _handler(new VarChangeEventArgs<T>(oldVal, _valCahche));
        }

        public void Dispose()
        {
            _var.Changed -= VarChanged;
        }
    }
}
