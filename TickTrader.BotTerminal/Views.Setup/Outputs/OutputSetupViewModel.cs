using System;
using System.Windows.Media;
using TickTrader.Algo.Common.Model.Setup;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Metadata;
using TickTrader.Algo.Domain;

namespace TickTrader.BotTerminal
{
    public abstract class OutputSetupViewModel : PropertySetupViewModel
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
                NotifyOfPropertyChange(nameof(IsEnabled));
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
                NotifyOfPropertyChange(nameof(LineColor));
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
                NotifyOfPropertyChange(nameof(LineThickness));
            }
        }


        public OutputSetupViewModel(OutputDescriptor descriptor)
        {
            Descriptor = descriptor;

            SetMetadata(descriptor);
        }


        public override void Reset()
        {
            IsEnabled = !HasError && Descriptor.Visibility;
            InitColor();
            InitThickness();
        }


        protected virtual void LoadConfig(IOutputConfig output)
        {
            IsEnabled = output.IsEnabled;
            LineColor = output.LineColorArgb.ToWindowsColor();
            LineThickness = output.LineThickness;
        }

        protected virtual void SaveConfig(IOutputConfig output)
        {
            output.PropertyId = Id;
            output.IsEnabled = IsEnabled;
            output.LineColorArgb = LineColor.ToArgb();
            output.LineThickness = LineThickness;
        }


        private void InitColor()
        {
            LineColor = Descriptor.DefaultColor.ToArgb(ApiColorConverter.GreenColor).ToWindowsColor();
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


        public class Invalid : OutputSetupViewModel
        {
            public Invalid(OutputDescriptor descriptor, object error = null)
                : base(descriptor)
            {
                if (error == null)
                    Error = new ErrorMsgModel(descriptor.Error);
                else
                    Error = new ErrorMsgModel(error);
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
