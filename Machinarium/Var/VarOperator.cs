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

        public static Var<TOut> Convert<TIn, TOut>(this Var<TIn> src, Func<TIn, TOut> operatorDef)
        {
            return Var<TOut>.Operator<Var<TOut>>(() => operatorDef(src.Value), src);
        }

        public static BoolVar Check<T>(this Var<T> src, Func<T, bool> operatorDef)
        {
            return BoolVar.Operator<BoolVar>(() => operatorDef(src.Value), src);
        }

        public static BoolVar IsNotNull<T>(this Var<T> src)
            where T : class
        {
            return BoolVar.Operator<BoolVar>(() => src.Value != null, src);
        }

        public static BoolVar IsNull<T>(this Var<T> src)
            where T : class
        {
            return BoolVar.Operator<BoolVar>(() => src.Value == null, src);
        }

        public static BoolVar IsNotNull<T>(this Var<T?> src)
            where T : struct
        {
            return BoolVar.Operator<BoolVar>(() => src.Value.HasValue, src);
        }

        public static BoolVar IsNull<T>(this Var<T?> src)
            where T : struct
        {
            return BoolVar.Operator<BoolVar>(() => !src.Value.HasValue, src);
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

        public static BoolVar Ref<TEntity>(this IProperty<TEntity> entityProp, Func<TEntity, bool> selector)
            where TEntity : class
        {
            return BoolVar.Operator<BoolVar>(() => entityProp.Value == null ? false : selector(entityProp.Value), entityProp.Var);
        }

        public static Var<TProp> Ref<TProp, TEntity>(this IProperty<TEntity> entityProp, Func<TEntity, TProp> selector)
            where TEntity : class
        {
            return Var<TProp>.Operator<Var<TProp>>(() => entityProp.Value == null ? default(TProp) : selector(entityProp.Value), entityProp.Var);
        }

        public static Var<TProp> Ref<TProp, TEntity>(this Var<TEntity> entityVar, Func<TEntity, TProp> selector)
            where TEntity : class
        {
            return Var<TProp>.Operator<Var<TProp>>(() => entityVar.Value == null ? default(TProp) : selector(entityVar.Value), entityVar);
        }

        public static Var<TProp> Ref<TProp, TEntity>(this IProperty<TEntity> entityProp, Func<TEntity, Var<TProp>> selector)
            where TEntity : class
        {
            return Var<TProp>.SelectOperator<Var<TProp>, TEntity>(selector, entityProp.Var);
        }

        public static BoolVar Ref<TEntity>(this IProperty<TEntity> entityProp, Func<TEntity, Var<bool>> selector)
            where TEntity : class
        {
            return BoolVar.SelectOperator<BoolVar, TEntity>(selector, entityProp.Var);
        }

        public static IntVar Ref<TEntity>(this IProperty<TEntity> entityProp, Func<TEntity, Var<int>> selector)
            where TEntity : class
        {
            return IntVar.SelectOperator<IntVar, TEntity>(selector, entityProp.Var);
        }

        public static Var<TProp> Ref<TProp, TEntity>(this Var<TEntity> entityVar, Func<TEntity, Var<TProp>> selector)
            where TEntity : class
        {
            return Var<TProp>.SelectOperator<Var<TProp>, TEntity>(selector, entityVar);
        }

        public static BoolVar Ref<TEntity>(this Var<TEntity> entityVar, Func<TEntity, Var<bool>> selector)
            where TEntity : class
        {
            return BoolVar.SelectOperator<BoolVar, TEntity>(selector, entityVar);
        }

        public static BoolVar Ref<TEntity>(this Var<TEntity> entityVar, Func<TEntity, BoolVar> selector)
            where TEntity : class
        {
            return BoolVar.SelectOperator<BoolVar, TEntity>(selector, entityVar);
        }

        public static IntVar Ref<TEntity>(this Var<TEntity> entityVar, Func<TEntity, Var<int>> selector)
            where TEntity : class
        {
            return IntVar.SelectOperator<IntVar, TEntity>(selector, entityVar);
        }

        public static DoubleVar Ref<TEntity>(this Var<TEntity> entityVar, Func<TEntity, Var<double>> selector)
            where TEntity : class
        {
            return DoubleVar.SelectOperator<DoubleVar, TEntity>(selector, entityVar);
        }
    }
}
