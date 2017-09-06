using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machinarium.Var
{
    public static class Operator
    {
        public static IntVar Convert<T>(this IVar<T> src, Func<T, int> operatorDef)
        {
            return new IntVar.Operator(() => operatorDef(src.Value), src);
        }

        public static DoubleVar Convert<T>(this IVar<T> src, Func<T, double> operatorDef)
        {
            return new DoubleVar.Operator(() => operatorDef(src.Value), src);
        }

        public static BoolVar If<T>(this IVar<T> src, Func<T, bool> operatorDef)
        {
            return new BoolVar.Operator(() => operatorDef(src.Value), src);
        }

        public static IntVar Create<T1>(IVar<T1> src1, Func<T1, int> operatorDef)
        {
            return new IntVar.Operator(() => operatorDef(src1.Value), src1);
        }

        public static IntVar Create<T1, T2>(IVar<T1> src1, IVar<T2> src2, Func<T1, T2, int> operatorDef)
        {
            return new IntVar.Operator(() => operatorDef(src1.Value, src2.Value), src1, src2);
        }

        public static IntVar Create<T1, T2, T3>(IVar<T1> src1, IVar<T2> src2, IVar<T3> src3, Func<T1, T2, T3, int> operatorDef)
        {
            return new IntVar.Operator(() => operatorDef(src1.Value, src2.Value, src3.Value), src1, src2, src3);
        }

        public static BoolVar Create<T1>(IVar<T1> src1, Func<T1, bool> operatorDef)
        {
            return new BoolVar.Operator(() => operatorDef(src1.Value), src1);
        }

        public static BoolVar Create<T1, T2>(IVar<T1> src1, IVar<T2> src2, Func<T1, T2, bool> operatorDef)
        {
            return new BoolVar.Operator(() => operatorDef(src1.Value, src2.Value), src1, src2);
        }

        public static BoolVar Create<T1, T2, T3>(IVar<T1> src1, IVar<T2> src2, IVar<T3> src3, Func<T1, T2, T3, bool> operatorDef)
        {
            return new BoolVar.Operator(() => operatorDef(src1.Value, src2.Value, src3.Value), src1, src2, src3);
        }
    }
}
