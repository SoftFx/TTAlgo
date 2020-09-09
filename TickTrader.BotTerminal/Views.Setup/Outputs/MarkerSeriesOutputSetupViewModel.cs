using System;
using TickTrader.Algo.Common.Model.Setup;
using TickTrader.Algo.Core.Metadata;
using TickTrader.Algo.Domain;

namespace TickTrader.BotTerminal
{
    public class MarkerSeriesOutputSetupViewModel : OutputSetupViewModel
    {
        private static Metadata.Types.MarkerSize[] _availableSizes = (Metadata.Types.MarkerSize[])Enum.GetValues(typeof(Metadata.Types.MarkerSize));


        private Metadata.Types.MarkerSize _markerSize;


        public Metadata.Types.MarkerSize[] AvailableSizes => _availableSizes;

        public Metadata.Types.MarkerSize MarkerSize
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

        public MarkerSeriesOutputSetupViewModel(OutputDescriptor descriptor)
            : base(descriptor)
        {
        }


        public override void Reset()
        {
            base.Reset();

            MarkerSize = Metadata.Types.MarkerSize.Medium;
        }


        public override void Load(IPropertyConfig srcProperty)
        {
            var output = srcProperty as MarkerSeriesOutputConfig;
            if (output != null)
            {
                MarkerSize = output.MarkerSize;
                LoadConfig(output);
            }
        }

        public override IPropertyConfig Save()
        {
            var output = new MarkerSeriesOutputConfig { MarkerSize = MarkerSize };
            SaveConfig(output);
            return output;
        }
    }
}
