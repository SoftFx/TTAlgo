using TickTrader.Algo.Core.Metadata;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Common.Model.Setup
{
    public class MarkerSeriesOutputSetupModel : OutputSetupModel
    {
        public Metadata.Types.MarkerSize MarkerSize { get; protected set; }

        public MarkerSeriesOutputSetupModel(OutputMetadata metadata)
            : base(metadata)
        {
        }


        public override void Reset()
        {
            base.Reset();

            MarkerSize = Domain.Metadata.Types.MarkerSize.Medium;
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
