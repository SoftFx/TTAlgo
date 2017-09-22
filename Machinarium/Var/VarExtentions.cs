using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machinarium.Var
{
    public static class VarExtentions
    {
        //public static BoolVar Check<T>(this Var<T> srcVar, Predicate<T> condition)
        //{
        //    return BoolVar.Operator<BoolVar>(() => condition(srcVar.Value), srcVar);
        //}
    }

    public struct VarChangeEventArgs<T>
    {
        internal VarChangeEventArgs(T oldValue, T newValue)
        {
            Old = oldValue;
            New = newValue;
        }

        public T Old { get; }
        public T New { get; }
    }
}
