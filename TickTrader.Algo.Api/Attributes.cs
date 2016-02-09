using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Api
{
    [Serializable]
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class InputAttribute : Attribute
    {
        public string DisplayName { get; set; }
    }

    [Serializable]
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class OutputAttribute : Attribute
    {
        public string DisplayName { get; set; }
    }

    [Serializable]
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class ParameterAttribute : Attribute
    {
        public object DefaultValue { get; set; }
        public string DisplayName { get; set; }
    }

    [Serializable]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class IndicatorAttribute : Attribute
    {
        public string DisplayName { get; set; }
        public string Category { get; set; }
    }

    [Serializable]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class TradeBotAttribute : Attribute
    {
        public string DisplayName { get; set; }
        public string Category { get; set; }
    }
}
