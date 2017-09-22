using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machinarium.Var
{
    public class IntVar : Var<int>
    {
        public IntVar()
        {
        }

        public IntVar(int initialValue) : base(initialValue)
        {
        }

        public static IntVar operator +(IntVar c1, IntVar c2)
        {
            return Operator<IntVar>(() => c1.Value + c2.Value, c1, c2);
        }

        public static IntVar operator +(IntVar c1, int c2)
        {
            return Operator<IntVar>(() => c1.Value + c2, c1);
        }

        public static IntVar operator +(int c1, IntVar c2)
        {
            return Operator<IntVar>(() => c1 + c2.Value, c2);
        }

        public static IntVar operator -(IntVar c1, IntVar c2)
        {
            return Operator<IntVar>(() => c1.Value - c2.Value, c1, c2);
        }

        public static IntVar operator -(int c1, IntVar c2)
        {
            return Operator<IntVar>(() => c1 - c2.Value, c2);
        }

        public static IntVar operator -(IntVar c1, int c2)
        {
            return Operator<IntVar>(() => c1.Value - c2, c1);
        }

        public static IntVar operator /(IntVar c1, IntVar c2)
        {
            return Operator<IntVar>(() => c1.Value / c2.Value, c1, c2);
        }

        public static IntVar operator /(int c1, IntVar c2)
        {
            return Operator<IntVar>(() => c1 / c2.Value, c2);
        }

        public static IntVar operator /(IntVar c1, int c2)
        {
            return Operator<IntVar>(() => c1.Value / c2, c1);
        }

        public static IntVar operator *(IntVar c1, IntVar c2)
        {
            return Operator<IntVar>(() => c1.Value * c2.Value, c1, c2);
        }

        public static IntVar operator *(int c1, IntVar c2)
        {
            return Operator<IntVar>(() => c1 * c2.Value, c2);
        }

        public static IntVar operator *(IntVar c1, int c2)
        {
            return Operator<IntVar>(() => c1.Value * c2, c1);
        }

        internal override bool Equals(int val1, int val2)
        {
            return val1 == val2;
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
