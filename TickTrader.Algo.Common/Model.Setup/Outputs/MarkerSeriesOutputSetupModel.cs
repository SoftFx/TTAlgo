using System;
using TickTrader.Algo.Common.Model.Config;
using TickTrader.Algo.Core.Metadata;

namespace TickTrader.Algo.Common.Model.Setup
{
    public enum MarkerSizes
    {
        Large,
        Medium,
        Small,
    }


    public class MarkerSeriesOutputSetupModel : OutputSetupModel
    {
        private static MarkerSizes[] _availableSizes = (MarkerSizes[])Enum.GetValues(typeof(MarkerSizes));


        public MarkerSizes MarkerSize { get; protected set; }

        public MarkerSeriesOutputSetupModel(OutputMetadata metadata)
            : base(metadata)
        {
        }


        public override void Reset()
        {
            MarkerSize = MarkerSizes.Medium;
        }


        public override void Load(Property srcProperty)
        {
            var output = srcProperty as MarkerSeriesOutput;
            if (output != null)
            {
                MarkerSize = output.MarkerSize;
                LoadConfig(output);
            }
        }

        public override Property Save()
        {
            var output = new MarkerSeriesOutput { MarkerSize = MarkerSize };
            SaveConfig(output);
            return output;
        }
    }
}
