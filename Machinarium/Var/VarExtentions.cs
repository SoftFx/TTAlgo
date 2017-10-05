using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machinarium.Var
{
    public static class VarExtentions
    {
        public static void Set(this BoolProperty property)
        {
            property.Value = true;
        }

        public static void Clear(this BoolProperty property)
        {
            property.Value = false;
        }
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
