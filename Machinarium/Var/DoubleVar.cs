using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machinarium.Var
{
    public abstract class DoubleVar : Var, IVar<double>
    {
        public abstract double Value { get; set; }

        public static implicit operator DoubleVar(double v)
        {
            return new Variable(v);
        }

        public static DoubleVar operator +(DoubleVar c1, DoubleVar c2)
        {
            return new Operator(() => c1.Value + c2.Value, c1, c2);
        }

        public static DoubleVar operator +(DoubleVar c1, double c2)
        {
            return new Operator(() => c1.Value + c2, c1);
        }

        public static DoubleVar operator +(double c1, DoubleVar c2)
        {
            return new Operator(() => c1 + c2.Value, c2);
        }

        public static DoubleVar operator -(DoubleVar c1, DoubleVar c2)
        {
            return new Operator(() => c1.Value - c2.Value, c1, c2);
        }

        public static DoubleVar operator -(double c1, DoubleVar c2)
        {
            return new Operator(() => c1 - c2.Value, c2);
        }

        public static DoubleVar operator -(DoubleVar c1, double c2)
        {
            return new Operator(() => c1.Value - c2, c1);
        }

        public static DoubleVar operator /(DoubleVar c1, DoubleVar c2)
        {
            return new Operator(() => c1.Value / c2.Value, c1, c2);
        }

        public static DoubleVar operator /(double c1, DoubleVar c2)
        {
            return new Operator(() => c1 / c2.Value, c2);
        }

        public static DoubleVar operator /(DoubleVar c1, double c2)
        {
            return new Operator(() => c1.Value / c2, c1);
        }

        public static DoubleVar operator *(DoubleVar c1, DoubleVar c2)
        {
            return new Operator(() => c1.Value * c2.Value, c1, c2);
        }

        public static DoubleVar operator *(double c1, DoubleVar c2)
        {
            return new Operator(() => c1 * c2.Value, c2);
        }

        public static DoubleVar operator *(DoubleVar c1, double c2)
        {
            return new Operator(() => c1.Value * c2, c1);
        }

        public static BoolVar operator >(DoubleVar c1, DoubleVar c2)
        {
            return new BoolVar.Operator(() => c1.Value > c2.Value, c1);
        }

        public static BoolVar operator <(DoubleVar c1, DoubleVar c2)
        {
            return new BoolVar.Operator(() => c1.Value < c2.Value, c1);
        }

        public static BoolVar operator >=(DoubleVar c1, DoubleVar c2)
        {
            return new BoolVar.Operator(() => c1.Value >= c2.Value, c1);
        }

        public static BoolVar operator <=(DoubleVar c1, DoubleVar c2)
        {
            return new BoolVar.Operator(() => c1.Value <= c2.Value, c1);
        }

        public class Variable : DoubleVar
        {
            private double _val;

            public Variable(double initialValue = 0)
            {
                _val = initialValue;
            }

            public override double Value
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

        internal class Operator : DoubleVar
        {
            private IVar[] _baseSources;
            private Func<double> _operatorDef;
            private double _value;

            public Operator(Func<double> operatorDef, params IVar[] baseSources)
            {
                _operatorDef = operatorDef;
                _baseSources = baseSources;

                foreach (var src in baseSources)
                    src.Changed += Src_Changed;

                _value = operatorDef();
            }

            private void Src_Changed(bool disposed)
            {
                if (disposed)
                    Dispose();
                else
                {
                    var newVal = _operatorDef();
                    if (newVal != _value)
                    {
                        _value = newVal;
                        OnChanged();
                    }
                }
            }

            public override double Value
            {
                get => _value;
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
