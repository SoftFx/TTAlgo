using System;
using TickTrader.Algo.Core.Metadata;
using TickTrader.Algo.Core;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Common.Model.Setup
{
    public abstract class OutputSetupModel : PropertySetupModel
    {
        public OutputMetadata Metadata { get; }

        public bool IsEnabled { get; protected set; }

        public uint LineColorArgb { get; protected set; }

        public int LineThickness { get; protected set; }

        public OutputSetupModel(OutputMetadata metadata)
        {
            Metadata = metadata;

            SetMetadata(metadata);
        }


        public override void Reset()
        {
            IsEnabled = !HasError && Metadata.Descriptor.Visibility;
            InitColor();
            InitThickness();
        }


        protected virtual void LoadConfig(IOutputConfig output)
        {
            IsEnabled = output.IsEnabled;
            LineColorArgb = output.LineColorArgb;
            LineThickness = output.LineThickness;
        }

        protected virtual void SaveConfig(IOutputConfig output)
        {
            output.PropertyId = Id;
            output.IsEnabled = IsEnabled;
            output.LineColorArgb = output.LineColorArgb;
            output.LineThickness = LineThickness;
        }


        private void InitColor()
        {
            LineColorArgb = Metadata.Descriptor.DefaultColor.ToArgb() ?? ApiColorConverter.GreenColor;
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

            public override void Load(IPropertyConfig srcProperty)
            {
            }

            public override IPropertyConfig Save()
            {
                throw new Exception("Cannot save error output!");
            }

            public override void Reset()
            {
            }
        }
    }
}
