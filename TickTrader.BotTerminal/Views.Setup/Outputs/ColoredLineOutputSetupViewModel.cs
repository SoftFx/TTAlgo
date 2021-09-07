using System;
using TickTrader.Algo.Domain;

namespace TickTrader.BotTerminal
{
    public class ColoredLineOutputSetupViewModel : OutputSetupViewModel
    {
        private static Metadata.Types.LineStyle[] _availableLineStyles = (Metadata.Types.LineStyle[])Enum.GetValues(typeof(Metadata.Types.LineStyle));


        private Metadata.Types.LineStyle _style;


        public Metadata.Types.LineStyle[] AvailableLineStyles => _availableLineStyles;

        public Metadata.Types.LineStyle LineStyle
        {
            get { return _style; }
            set
            {
                if (_style == value)
                    return;

                _style = value;
                NotifyOfPropertyChange(nameof(LineStyle));
            }
        }


        public ColoredLineOutputSetupViewModel(OutputDescriptor descriptor)
            : base(descriptor)
        {
        }


        public override void Reset()
        {
            base.Reset();

            LineStyle = Descriptor.DefaultLineStyle;
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
