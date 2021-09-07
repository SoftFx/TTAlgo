using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machinarium.Var
{
    internal class Operator<T> : IDisposable
    {
        private Var[] _baseSources;
        private Func<T> _operatorDef;
        private Var<T> _target;

        public Operator(Var<T> targetVar, Func<T> operatorDef, params Var[] baseSources)
        {
            _target = targetVar;
            _operatorDef = operatorDef;
            _baseSources = baseSources;

            foreach (var src in baseSources)
                src.Changed += Src_Changed;

            _target.SetOperator(this);
            _target.SetValueInternal(operatorDef());
        }

        private void Src_Changed()
        {
            _target.SetValueInternal(_operatorDef());
        }

        public void Dispose()
        {
            if (_baseSources != null)
            {
                foreach (var src in _baseSources)
                {
                    src.Changed -= Src_Changed;
                    src.DisposeIfNoContext(); // propogate dispose down
                }

                _baseSources = null;
            }
        }
    }
}
