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


        private Color _lineColor;
        private int _lineThickness;
        private bool _isEnabled;


        public OutputDescriptor Descriptor { get; }

        public bool IsEnabled
        {
            get { return _isEnabled; }
            set
            {
                if (_isEnabled == value)
                    return;

                _isEnabled = value;
                NotifyPropertyChanged(nameof(IsEnabled));
            }
        }

        public Color LineColor
        {
            get { return _lineColor; }
            set
            {
                if (_lineColor == value)
                    return;

                _lineColor = value;
                NotifyPropertyChanged(nameof(LineColor));
            }
        }

        public int[] AvailableThicknesses => _availableThicknesses;

        public int LineThickness
        {
            get { return _lineThickness; }
            set
            {
                if (_lineThickness == value)
                    return;

                _lineThickness = value;
                NotifyPropertyChanged(nameof(LineThickness));
            }
        }


        public OutputSetupModel(OutputDescriptor descriptor)
        {
            Descriptor = descriptor;

            SetMetadata(descriptor);
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
            LineColor = Convert.ToWindowsColor(Descriptor.DefaultColor);
        }

        private void InitThickness()
        {
            var intThikness = (int)Descriptor.DefaultThickness;
            if (intThikness < 1)
                intThikness = 1;
            if (intThikness > 5)
                intThikness = 5;
            LineThickness = intThikness;
        }


        public class Invalid : OutputSetupModel
        {
            public Invalid(OutputDescriptor descriptor, object error = null)
                : base(descriptor)
            {
                if (error == null)
                    Error = new ErrorMsgModel(descriptor.Error.Value);
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
