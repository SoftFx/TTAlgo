using System;
using TickTrader.Algo.Api;
using TickTrader.Algo.Common.Info;
using TickTrader.Algo.Common.Model.Config;

namespace TickTrader.BotTerminal
{
    public class ColoredLineOutputSetupModel : OutputSetupViewModel
    {
        private static LineStyles[] _availableLineStyles = (LineStyles[])Enum.GetValues(typeof(LineStyles));


        private LineStyles _style;


        public LineStyles[] AvailableLineStyles => _availableLineStyles;

        public LineStyles LineStyle
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


        public ColoredLineOutputSetupModel(OutputMetadataInfo metadata)
            : base(metadata)
        {
        }


        public override void Reset()
        {
            base.Reset();

            LineStyle = Metadata.DefaultLineStyle;
        }

        public override void Load(Property srcProperty)
        {
            var output = srcProperty as ColoredLineOutput;
            if (output != null)
            {
                LineStyle = output.LineStyle;
                LoadConfig(output);
            }
        }

        public override Property Save()
        {
            var output = new ColoredLineOutput { LineStyle = LineStyle };
            SaveConfig(output);
            return output;
        }
    }
}
