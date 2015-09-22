using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Api
{
    // Warning! This is pure black magic! Don't try it at home!
    internal static class ReflectionMagic
    {
        public static PropertyInfo[] GetAllAccessLevelProperties(this Type inspectedType)
        {
            return inspectedType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        }
    }
}
