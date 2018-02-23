using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api.Ext;

namespace TickTrader.Algo.Core.Metadata
{
    public enum ReductionType
    {
        Unknown,
        BarToDouble,
        FullBarToDouble,
        FullBarToBar,
        QuoteToDouble,
        QuoteToBar,
    }

    public class ReductionDescriptor
    {
        public static IEnumerable<ReductionDescriptor> InspectAssembly(Assembly targetAssembly)
        {
            List<ReductionDescriptor> descriptors = new List<ReductionDescriptor>();

            foreach (Type t in targetAssembly.GetTypes())
            {
                var reductionAttr = t.GetCustomAttribute<ReductionAttribute>();
                if (reductionAttr != null)
                    descriptors.Add(new ReductionDescriptor(t, reductionAttr));
            }

            return descriptors;
        }

        public string DisplayName { get; private set; }
        public Type ClassType { get; private set; }
        public ReductionType Type { get; private set; }
        public Version ApiVersion { get; private set; }

        public T CreateInstance<T>()
        {
            return (T)Activator.CreateInstance(ClassType);
        }

        public ReductionDescriptor(Type algoCustomType, ReductionAttribute reductionAttr)
        {
            this.ClassType = algoCustomType;

            var refs = algoCustomType.Assembly.GetReferencedAssemblies();
            var apiref = refs.FirstOrDefault(a => a.Name == "TickTrader.Algo.Api");
            ApiVersion = apiref?.Version;

            DisplayName = reductionAttr.DisplayName;
            if (string.IsNullOrEmpty(DisplayName))
                DisplayName = algoCustomType.Name;

            if (typeof(BarToDoubleReduction).IsAssignableFrom(algoCustomType))
                Type = ReductionType.BarToDouble;
            else if (typeof(FullBarToDoubleReduction).IsAssignableFrom(algoCustomType))
                Type = ReductionType.FullBarToDouble;
            else if (typeof(FullBarToBarReduction).IsAssignableFrom(algoCustomType))
                Type = ReductionType.FullBarToBar;
            else if (typeof(QuoteToDoubleReduction).IsAssignableFrom(algoCustomType))
                Type = ReductionType.QuoteToDouble;
            else if (typeof(QuoteToBarReduction).IsAssignableFrom(algoCustomType))
                Type = ReductionType.QuoteToBar;
            else
                Type = ReductionType.Unknown;
        }
    }
}
