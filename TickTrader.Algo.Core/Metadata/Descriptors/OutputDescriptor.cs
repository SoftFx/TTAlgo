using System;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.Core.Metadata
{
    [Serializable]
    public class OutputDescriptor : PropertyDescriptor
    {
        public override AlgoPropertyTypes PropertyType => AlgoPropertyTypes.OutputSeries;

        public string DataSeriesBaseTypeFullName { get; set; }

        public double DefaultThickness { get; set; }

        public Colors DefaultColor { get; set; }

        public LineStyles DefaultLineStyle { get; set; }

        public PlotType PlotType { get; set; }

        public OutputTargets Target { get; set; }

        public int Precision { get; set; }

        public double ZeroLine { get; set; }
    }
}
