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
        public OutputAttribute()
        {
            DefaultColor = Colors.Auto;
        }

        public string DisplayName { get; set; }
        public Colors DefaultColor { get; set; }
        public LineStyles DefaultLineStyle { get; set; }
        public float DefaultThickness { get; set; }
        public PlotType PlotType { get; set; }
    }

    [Serializable]
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class ParameterAttribute : Attribute
    {
        public object DefaultValue { get; set; }
        public string DisplayName { get; set; }
        public bool IsRequired { get; set; }
    }

    [Serializable]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class IndicatorAttribute : Attribute
    {
        public bool IsOverlay { get; set; }
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

    [Serializable]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class CopyrightAttribute : Attribute
    {
        public CopyrightAttribute(string copyrightText)
        {
            this.Text = copyrightText;
        }

        public string Text { get; set; }
    }

    [Serializable]
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class FileFilterAttribute : Attribute
    {
        public FileFilterAttribute(string name, string mask)
        {
            this.Name = name;
            this.Mask = mask;
        }

        public string Name { get; set; }
        public string Mask { get; set; }
    }
}
