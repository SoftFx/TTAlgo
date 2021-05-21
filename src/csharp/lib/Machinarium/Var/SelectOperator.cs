using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machinarium.Var
{
    internal class SelectOperator<T, TRef> : IDisposable
        where TRef : class
    {
        private Var<TRef> _srcVar;
        private Func<TRef, Var<T>> _selector;
        private Var<T> _target;
        private Var<T> _srcProperty;

        public SelectOperator(Var<T> targetVar, Var<TRef> srcVar, Func<TRef, Var<T>> selector)
        {
            if (ReferenceEquals(srcVar, null))
                throw new ArgumentNullException("srcVar");

            _target = targetVar;
            _selector = selector;
            _srcVar = srcVar;
            _srcVar.Changed += OnEntityRefChanged;

            _target.SetOperator(this);

            OnEntityRefChanged();
        }

        private void OnEntityRefChanged()
        {
            var newProperty = SelectProperty();
            if (!ReferenceEquals(newProperty, _srcProperty))
            {
                if (!ReferenceEquals(_srcProperty, null))
                    _srcProperty.Changed -= OnPropertyChanged;

                _srcProperty = newProperty;

                if (!ReferenceEquals(_srcProperty, null))
                {
                    _target.SetValueInternal(_srcProperty.Value);
                    _srcProperty.Changed += OnPropertyChanged;
                }
                else
                    _target.SetValueInternal(default(T));
            }
        }

        private Var<T> SelectProperty()
        {
            var entity = _srcVar.Value;
            if (entity == null)
                return null;
            return _selector(entity);
        }

        private void OnPropertyChanged()
        {
            _target.SetValueInternal(_srcProperty.Value);
        }

        public void Dispose()
        {
            _srcVar.Changed -= OnEntityRefChanged;

            if (!ReferenceEquals(_srcProperty, null))
                _srcProperty.Changed -= OnPropertyChanged;
        }
    }
}
