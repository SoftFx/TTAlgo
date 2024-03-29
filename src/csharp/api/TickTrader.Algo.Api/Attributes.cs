﻿using System;

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
            Target = OutputTargets.Overlay;
            Precision = -1;
            Visibility = true;
        }

        public string DisplayName { get; set; }
        public Colors DefaultColor { get; set; }
        public LineStyles DefaultLineStyle { get; set; }
        public float DefaultThickness { get; set; }
        public PlotType PlotType { get; set; }
        public OutputTargets Target { get; set; }
        public int Precision { get; set; }
        public bool Visibility { get; set; }
        /// <summary>
        /// Used to set the bottom of a column in histogram
        /// </summary>
        public double ZeroLine { get; set; }
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
    public abstract class AlgoPluginAttribute : Attribute
    {
        public string DisplayName { get; set; }
        public string Category { get; set; }
        public string Version { get; set; }
        public string Description { get; set; }
        /// <summary>
        /// Defines whether MainSymbol setup should be shown during bot setup. True by default for bots
        /// </summary>
        public bool SetupMainSymbol { get; set; }
    }

    [Serializable]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class IndicatorAttribute : AlgoPluginAttribute
    {
    }

    [Serializable]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class TradeBotAttribute : AlgoPluginAttribute
    {
        public TradeBotAttribute()
        {
            SetupMainSymbol = true;
        }
    }

    [Serializable]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class CopyrightAttribute : Attribute
    {
        public CopyrightAttribute(string copyrightText)
        {
            Text = copyrightText;
        }

        public string Text { get; set; }
    }

    [Serializable]
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class FileFilterAttribute : Attribute
    {
        public FileFilterAttribute(string name, string mask)
        {
            Name = name;
            Mask = mask;
        }

        public string Name { get; set; }
        public string Mask { get; set; }
    }
}
