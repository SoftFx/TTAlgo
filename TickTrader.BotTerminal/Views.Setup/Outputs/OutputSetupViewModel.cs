using System;
using TickTrader.Algo.Common.Model.Config;
using TickTrader.Algo.Common.Info;
using System.Windows.Media;
using TickTrader.Algo.Common.Model.Setup;

namespace TickTrader.BotTerminal
{
    public abstract class OutputSetupViewModel : PropertySetupViewModel
    {
        private static int[] _availableThicknesses = new int[] { 1, 2, 3, 4, 5 };


        private Color _lineColor;
        private int _lineThickness;
        private bool _isEnabled;


        public OutputMetadataInfo Metadata { get; }

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


        public OutputSetupViewModel(OutputMetadataInfo metadata)
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
            LineColor = Algo.Common.Model.Setup.Convert.ToWindowsColor(Metadata.DefaultColor);
        }

        private void InitThickness()
        {
            var intThikness = (int)Metadata.DefaultThickness;
            if (intThikness < 1)
                intThikness = 1;
            if (intThikness > 5)
                intThikness = 5;
            LineThickness = intThikness;
        }


        public class Invalid : OutputSetupViewModel
        {
            public Invalid(OutputMetadataInfo metadata, object error = null)
                : base(metadata)
            {
                if (error == null)
                    Error = new ErrorMsgModel(metadata.Error.Value);
                else
                    Error = new ErrorMsgModel(error);
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
