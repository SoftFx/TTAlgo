using System;
using TickTrader.Algo.Core.Metadata;
using TickTrader.Algo.Common.Model.Config;
using TickTrader.Algo.Core;
using System.Windows.Media;

namespace TickTrader.Algo.Common.Model.Setup
{
    public abstract class OutputSetupModel : PropertySetupModel
    {
        private static int[] _availableThicknesses = new int[] { 1, 2, 3, 4, 5 };


        public OutputMetadata Metadata { get; }

        public bool IsEnabled { get; protected set; }

        public Color LineColor { get; protected set; }

        public int LineThickness { get; protected set; }


        public OutputSetupModel(OutputMetadata metadata)
        {
            Metadata = metadata;

            SetMetadata(metadata);
        }


        public override void Reset()
        {
            IsEnabled = !HasError;
            InitColor();
            InitThickness();
        }


        protected virtual void LoadConfig(Output output)
        {
            IsEnabled = output.IsEnabled;
            LineColor = output.LineColor.ToWindowsColor();
            LineThickness = output.LineThickness;
        }

        protected virtual void SaveConfig(Output output)
        {
            output.Id = Id;
            output.IsEnabled = IsEnabled;
            output.LineColor = OutputColor.FromWindowsColor(LineColor);
            output.LineThickness = LineThickness;
        }


        private void InitColor()
        {
            LineColor = Convert.ToWindowsColor(Metadata.Descriptor.DefaultColor);
        }

        private void InitThickness()
        {
            var intThikness = (int)Metadata.Descriptor.DefaultThickness;
            if (intThikness < 1)
                intThikness = 1;
            if (intThikness > 5)
                intThikness = 5;
            LineThickness = intThikness;
        }


        public class Invalid : OutputSetupModel
        {
            public Invalid(OutputMetadata descriptor, object error = null)
                : base(descriptor)
            {
                if (error == null)
                    Error = new ErrorMsgModel(descriptor.Error);
                else
                    Error = new ErrorMsgModel(error);
            }


            public override void Apply(IPluginSetupTarget target)
            {
                throw new Exception("Cannot configure invalid output!");
            }

            public override void Load(Property srcProperty)
            {
            }

            public override Property Save()
            {
                throw new Exception("Cannot save error output!");
            }

            public override void Reset()
            {
            }
        }
    }
}
