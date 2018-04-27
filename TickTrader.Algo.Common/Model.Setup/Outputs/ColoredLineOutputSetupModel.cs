using System;
using TickTrader.Algo.Api;
using TickTrader.Algo.Common.Model.Config;
using TickTrader.Algo.Core.Metadata;

namespace TickTrader.Algo.Common.Model.Setup
{
    public class ColoredLineOutputSetupModel : OutputSetupModel
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
                NotifyPropertyChanged(nameof(LineStyle));
            }
        }


        public ColoredLineOutputSetupModel(OutputMetadata descriptor)
            : base(descriptor)
        {
        }


        public override void Reset()
        {
            base.Reset();

            LineStyle = Descriptor.Descriptor.DefaultLineStyle;
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
