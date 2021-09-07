using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machinarium.Var
{
    internal sealed class ChangeEvent<T> : IDisposable
    {
        private readonly Var<T> _var;
        private readonly Action<VarChangeEventArgs<T>> _handler;
        private T _valCache;

        public ChangeEvent(Var<T> var, Action<VarChangeEventArgs<T>> handler)
        {
            _var = var;
            _handler = handler;

            _valCache = _var.Value;
            _var.Changed += VarChanged;

            VarChanged();
        }

        private void VarChanged()
        {
            var oldVal = _valCache;
            _valCache = _var.Value;

            _handler(new VarChangeEventArgs<T>(oldVal, _valCache));
        }

        public void Dispose()
        {
            _var.Changed -= VarChanged;
        }
    }
}
