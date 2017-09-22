using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machinarium.Var
{
    public static class VarOperator
    {
        public static IntVar Convert<T>(this Var<T> src, Func<T, int> operatorDef)
        {
            return IntVar.Operator<IntVar>(() => operatorDef(src.Value), src);
        }

        public static DoubleVar Convert<T>(this Var<T> src, Func<T, double> operatorDef)
        {
            return DoubleVar.Operator<DoubleVar>(() => operatorDef(src.Value), src);
        }

        public static BoolVar Check<T>(this Var<T> src, Func<T, bool> operatorDef)
        {
            return BoolVar.Operator<BoolVar>(() => operatorDef(src.Value), src);
        }

        public static IntVar Create<T1>(Var<T1> src1, Func<T1, int> operatorDef)
        {
            return IntVar.Operator<IntVar>(() => operatorDef(src1.Value), src1);
        }

        public static IntVar Create<T1, T2>(Var<T1> src1, Var<T2> src2, Func<T1, T2, int> operatorDef)
        {
            return IntVar.Operator<IntVar>(() => operatorDef(src1.Value, src2.Value), src1, src2);
        }

        public static IntVar Create<T1, T2, T3>(Var<T1> src1, Var<T2> src2, Var<T3> src3, Func<T1, T2, T3, int> operatorDef)
        {
            return IntVar.Operator<IntVar>(() => operatorDef(src1.Value, src2.Value, src3.Value), src1, src2, src3);
        }

        public static BoolVar Create<T1>(Var<T1> src1, Func<T1, bool> operatorDef)
        {
            return BoolVar.Operator<BoolVar>(() => operatorDef(src1.Value), src1);
        }

        public static BoolVar Create<T1, T2>(Var<T1> src1, Var<T2> src2, Func<T1, T2, bool> operatorDef)
        {
            return BoolVar.Operator<BoolVar>(() => operatorDef(src1.Value, src2.Value), src1, src2);
        }

        public static BoolVar Create<T1, T2, T3>(Var<T1> src1, Var<T2> src2, Var<T3> src3, Func<T1, T2, T3, bool> operatorDef)
        {
            return BoolVar.Operator<BoolVar>(() => operatorDef(src1.Value, src2.Value, src3.Value), src1, src2, src3);
        }
    }
}
