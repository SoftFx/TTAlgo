﻿using System;
using System.Reflection;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.Core.Metadata
{
    [Serializable]
    public class OutputDescriptor : AlgoPropertyDescriptor
    {
        public OutputDescriptor(PropertyInfo propertyInfo, OutputAttribute attribute, bool isGlobalOverlay)
            : base(propertyInfo)
        {
            Validate(propertyInfo);

            var propertyType = propertyInfo.PropertyType;

            if (propertyType == typeof(DataSeries))
            {
                DataSeriesBaseType = typeof(double);
                IsShortDefinition = true;
            }
            else if (propertyType == typeof(MarkerSeries))
            {
                DataSeriesBaseType = typeof(Marker);
                IsShortDefinition = true;
            }
            else if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(DataSeries<>))
            {
                DataSeriesBaseType = propertyInfo.PropertyType.GetGenericArguments()[0];
            }
            else
                SetError(AlgoPropertyErrors.OutputIsNotDataSeries);

            if (DataSeriesBaseType != null && DataSeriesBaseType.IsInterface)
                IsHiddenEntity = true;

            DefaultThickness = attribute.DefaultThickness;
            DefaultColor = attribute.DefaultColor;
            DefaultLineStyle = attribute.DefaultLineStyle;
            PlotType = attribute.PlotType;
            IsOverlay = isGlobalOverlay || attribute.IsOverlay;
            PaneName = attribute.PaneName;

            InitDisplayName(attribute.DisplayName);
        }

        public Type DataSeriesBaseType { get; private set; }
        public string DataSeriesBaseTypeFullName => DataSeriesBaseType.FullName;
        public bool IsShortDefinition { get; private set; }
        public bool IsHiddenEntity { get; private set; }
        public double DefaultThickness { get; private set; }
        public Colors DefaultColor { get; private set; }
        public LineStyles DefaultLineStyle { get; private set; }
        public PlotType PlotType { get; private set; }
        public override AlgoPropertyTypes PropertyType => AlgoPropertyTypes.OutputSeries;
        public bool IsOverlay { get; private set; }
        public string PaneName { get; private set; }

        internal DataSeriesProxy<T> CreateOutput2<T>()
        {
            if (typeof(T) == typeof(double) && IsShortDefinition)
                return (DataSeriesProxy<T>)(object)new DataSeriesProxy();
            else if (typeof(T) == typeof(Marker) && IsShortDefinition)
                return (DataSeriesProxy<T>)(object)new MarkerSeriesProxy();
            else
                return new DataSeriesProxy<T>();
        }
    }
}
