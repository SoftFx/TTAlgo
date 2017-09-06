using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machinarium.Var
{
    public abstract class BoolVar : Var, IVar<bool>
    {
        public abstract bool Value { get; set; }

        public static implicit operator BoolVar(bool v)
        {
            return new Variable(v);
        }

        public static BoolVar operator &(BoolVar c1, BoolVar c2)
        {
            return new Operator(() => c1.Value && c2.Value, c1, c2);
        }

        public static BoolVar operator &(BoolVar c1, bool c2)
        {
            return new Operator(() => c1.Value && c2, c1);
        }

        public static BoolVar operator &(bool c1, BoolVar c2)
        {
            return new Operator(() => c1 && c2.Value, c2);
        }

        public static BoolVar operator |(BoolVar c1, BoolVar c2)
        {
            return new Operator(() => c1.Value || c2.Value, c1, c2);
        }

        public static BoolVar operator |(bool c1, BoolVar c2)
        {
            return new Operator(() => c1 || c2.Value, c2);
        }

        public static BoolVar operator |(BoolVar c1, bool c2)
        {
            return new Operator(() => c1.Value || c2, c1);
        }

        public static BoolVar operator !(BoolVar c1)
        {
            return new Operator(() => !c1.Value, c1);
        }

        public class Variable : BoolVar
        {
            private bool _val;

            public Variable(bool initialValue = false)
            {
                _val = initialValue;
            }

            public override bool Value
            {
                get => _val;
                set
                {
                    if (_val != value)
                    {
                        _val = value;
                        OnChanged();
                    }
                }
            }

            public override void Dispose()
            {
                OnDisposed();
            }
        }

        internal class Operator : BoolVar
        {
            private IVar[] _baseSources;
            private Func<bool> _operatorDef;

            public Operator(Func<bool> operatorDef, params IVar[] baseSources)
            {
                _operatorDef = operatorDef;
                _baseSources = baseSources;

                foreach (var src in baseSources)
                    src.Changed += Src_Changed;
            }

            private void Src_Changed(bool disposed)
            {
                if (disposed)
                    Dispose();
                else
                    OnChanged();
            }

            public override bool Value
            {
                get => _operatorDef();
                set => throw new Exception("Cannot set value for operator!");
            }

            public override void Dispose()
            {
                foreach (var src in _baseSources)
                    src.Changed -= Src_Changed;

                OnDisposed();
            }
        }
    }
}
