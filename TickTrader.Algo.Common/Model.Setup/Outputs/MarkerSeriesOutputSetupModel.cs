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


    public class MarkerSeriesOutputSetup : OutputSetupModel
    {
        private static MarkerSizes[] _availableSizes = (MarkerSizes[])Enum.GetValues(typeof(MarkerSizes));


        private MarkerSizes _markerSize;


        public MarkerSizes[] AvailableSizes => _availableSizes;

        public MarkerSizes MarkerSize
        {
            get { return _markerSize; }
            set
            {
                if (_markerSize == value)
                    return;

                _markerSize = value;
                NotifyPropertyChanged(nameof(MarkerSize));
            }
        }

        public MarkerSeriesOutputSetup(OutputDescriptor descriptor)
            : base(descriptor)
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
