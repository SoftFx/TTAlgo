using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Collections.Generic
{
    internal static class CollectionsExt
    {
        public static int IndexOf<T>(this IList<T> list, Predicate<T> predicate)
        {
            for (int i = 0; i < list.Count; i++)
                if (predicate(list[i]))
                    return i;
            return -1;
        }
    }
}
