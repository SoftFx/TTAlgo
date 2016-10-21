using EnvDTE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.VS.Package
{
    internal static class EnvDTEHelper
    {
        public static object Get(this Properties props, string name)
        {
            foreach (Property p in props)
            {
                if (p.Name == name)
                    return p.Value;
            }

            return null;
        }

        public static string GetString(this Properties props, string name)
        {
            return props.Get(name)?.ToString();
        }
    }
}
