using TickTrader.Algo.Api;

namespace TickTrader.Algo.Common.Info
{
    public class OutputMetadataInfo : PropertyMetadataInfo
    {
        public string DataSeriesBaseTypeFullName { get; set; }

        public double DefaultThickness { get; set; }

        public Colors DefaultColor { get; set; }

        public LineStyles DefaultLineStyle { get; set; }
    }
}
