namespace Machinarium.Var
{
    public class DoubleVar : Var<double>
    {
        public DoubleVar()
        {
        }

        public DoubleVar(double initialValue)
        {
        }

        public static DoubleVar operator +(DoubleVar c1, DoubleVar c2)
        {
            return Operator<DoubleVar>(() => c1.Value + c2.Value, c1, c2);
        }

        public static DoubleVar operator +(DoubleVar c1, double c2)
        {
            return Operator<DoubleVar>(() => c1.Value + c2, c1);
        }

        public static DoubleVar operator +(double c1, DoubleVar c2)
        {
            return Operator<DoubleVar>(() => c1 + c2.Value, c2);
        }

        public static DoubleVar operator -(DoubleVar c1, DoubleVar c2)
        {
            return Operator<DoubleVar>(() => c1.Value - c2.Value, c1, c2);
        }

        public static DoubleVar operator -(double c1, DoubleVar c2)
        {
            return Operator<DoubleVar>(() => c1 - c2.Value, c2);
        }

        public static DoubleVar operator -(DoubleVar c1, double c2)
        {
            return Operator<DoubleVar>(() => c1.Value - c2, c1);
        }

        public static DoubleVar operator /(DoubleVar c1, DoubleVar c2)
        {
            return Operator<DoubleVar>(() => c1.Value / c2.Value, c1, c2);
        }

        public static DoubleVar operator /(double c1, DoubleVar c2)
        {
            return Operator<DoubleVar>(() => c1 / c2.Value, c2);
        }

        public static DoubleVar operator /(DoubleVar c1, double c2)
        {
            return Operator<DoubleVar>(() => c1.Value / c2, c1);
        }

        public static DoubleVar operator *(DoubleVar c1, DoubleVar c2)
        {
            return Operator<DoubleVar>(() => c1.Value * c2.Value, c1, c2);
        }

        public static DoubleVar operator *(double c1, DoubleVar c2)
        {
            return Operator<DoubleVar>(() => c1 * c2.Value, c2);
        }

        public static DoubleVar operator *(DoubleVar c1, double c2)
        {
            return Operator<DoubleVar>(() => c1.Value * c2, c1);
        }

        public static BoolVar operator >(DoubleVar c1, DoubleVar c2)
        {
            return BoolVar.Operator<BoolVar>(() => c1.Value > c2.Value, c1);
        }

        public static BoolVar operator <(DoubleVar c1, DoubleVar c2)
        {
            return BoolVar.Operator<BoolVar>(() => c1.Value < c2.Value, c1);
        }

        public static BoolVar operator >=(DoubleVar c1, DoubleVar c2)
        {
            return BoolVar.Operator<BoolVar>(() => c1.Value >= c2.Value, c1);
        }

        public static BoolVar operator <=(DoubleVar c1, DoubleVar c2)
        {
            return BoolVar.Operator<BoolVar>(() => c1.Value <= c2.Value, c1);
        }

        internal override bool Equals(double val1, double val2)
        {
            return val1 == val2;
        }
    }
}
