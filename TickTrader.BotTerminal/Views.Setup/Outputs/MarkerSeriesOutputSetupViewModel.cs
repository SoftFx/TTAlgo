using System;
using TickTrader.Algo.Common.Info;
using TickTrader.Algo.Common.Model.Config;
using TickTrader.Algo.Common.Model.Setup;

namespace TickTrader.BotTerminal
{
    public class MarkerSeriesOutputSetupViewModel : OutputSetupViewModel
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
                NotifyOfPropertyChange(nameof(MarkerSize));
            }
        }

        public MarkerSeriesOutputSetupViewModel(OutputMetadataInfo metadata)
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
