using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Metadata;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Common.Model.Setup
{
    public class ColoredLineOutputSetupModel : OutputSetupModel
    {
        public Metadata.Types.LineStyle LineStyle { get; protected set; }


        public ColoredLineOutputSetupModel(OutputMetadata metadata)
            : base(metadata)
        {
        }


        public override void Reset()
        {
            base.Reset();

            LineStyle = Metadata.Descriptor.DefaultLineStyle.ToDomainEnum();
        }

        public override void Load(IPropertyConfig srcProperty)
        {
            var output = srcProperty as ColoredLineOutputConfig;
            if (output != null)
            {
                LineStyle = output.LineStyle;
                LoadConfig(output);
            }
        }

        public override IPropertyConfig Save()
        {
            var output = new ColoredLineOutputConfig { LineStyle = LineStyle };
            SaveConfig(output);
            return output;
        }
    }
}
