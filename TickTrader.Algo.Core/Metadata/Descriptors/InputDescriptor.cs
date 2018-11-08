using System;

namespace TickTrader.Algo.Core.Metadata
{
    [Serializable]
    public class InputDescriptor : PropertyDescriptor
    {
        public override AlgoPropertyTypes PropertyType => AlgoPropertyTypes.InputSeries;

        public string DataSeriesBaseTypeFullName { get; set; }
    }
}
