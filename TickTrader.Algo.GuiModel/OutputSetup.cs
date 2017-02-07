using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using TickTrader.Algo.Api;
using Api = TickTrader.Algo.Api;
using TickTrader.Algo.Core.Metadata;

namespace TickTrader.Algo.GuiModel
{
    public abstract class OutputSetup : PropertySetupBase
    {
        public override void CopyFrom(PropertySetupBase srcProperty) { }
        public override void Reset() { }

        public OutputSetup(OutputDescriptor descriptor)
        {
            SetMetadata(descriptor);
        }
    }

    public class ErrorOutputSetup : OutputSetup
    {
        public ErrorOutputSetup(OutputDescriptor descriptor, MsgCodes? error = null)
            : base(descriptor)
        {
            if (error != null && Error != null)
                Error = new GuiModelMsg(error);
        }
    }

    public class ColoredLineOutputSetup : OutputSetup
    {
        private static int[] availableThicknesses = new int[] { 1, 2, 3, 4, 5 };
        private static LineStyles[] availableLineStyles = (LineStyles[])Enum.GetValues(typeof(LineStyles));

        private OutputDescriptor descriptor;
        private Color lineColor;
        private int lineThickness;
        private bool isEnabled;
        private LineStyles style;

        public ColoredLineOutputSetup(OutputDescriptor descriptor, MsgCodes? error = null)
            : base(descriptor)
        {
            SetMetadata(descriptor);
            if (error != null)
                Error = new GuiModelMsg(error);
            this.descriptor = descriptor;
        }

        private void InitColor()
        {
            LineColor = Convert.ToWindowsColor(descriptor.DefaultColor);
        }

        private void InitThickness()
        {
            int intThikness = (int)descriptor.DefaultThickness;
            if (intThikness < 1)
                intThikness = 1;
            if (intThikness > 5)
                intThikness = 5;
            this.LineThickness = intThikness;
        }

        public override void Reset()
        {
            InitColor();
            InitThickness();
            LineStyle = descriptor.DefaultLineStyle;
            IsEnabled = !HasError;
        }

        public new OutputDescriptor Descriptor { get { return descriptor; } }
        public int[] AvailableThicknesses { get { return availableThicknesses; } }
        public LineStyles[] AvailableLineStyles { get { return availableLineStyles; } }

        public Color LineColor
        {
            get { return lineColor; }
            set
            {
                this.lineColor = value;
                NotifyPropertyChanged("LineColor");
            }
        }

        public int LineThickness
        {
            get { return lineThickness; }
            set
            {
                this.lineThickness = value;
                NotifyPropertyChanged("LineThickness");
            }
        }

        public LineStyles LineStyle
        {
            get { return style; }
            set
            {
                this.style = value;
                NotifyPropertyChanged("LineStyle");
            }
        }

        public bool IsEnabled
        {
            get { return isEnabled; }
            set
            {
                this.isEnabled = value;
                NotifyPropertyChanged("IsEnabled");
            }
        }

        public override void CopyFrom(PropertySetupBase srcProperty)
        {
            var oldSetup = srcProperty as ColoredLineOutputSetup;
            if (oldSetup != null)
            {
                this.LineColor = oldSetup.LineColor;
                this.LineThickness = oldSetup.LineThickness;
                this.LineStyle = oldSetup.LineStyle;
                this.IsEnabled = oldSetup.IsEnabled;
            }
        }
    }

    public enum MarkerSizes { Large, Medium, Small }

    public class MarkerSeriesOutputSetup : OutputSetup
    {
        private static MarkerSizes[] availableSizes = (MarkerSizes[])Enum.GetValues(typeof(MarkerSizes));
        private static int[] availableThicknesses = new int[] { 1, 2, 3, 4, 5 };

        private OutputDescriptor descriptor;
        private Color lineColor;
        private int lineThickness;
        private MarkerSizes markerSize;
        private bool isEnabled;

        public MarkerSeriesOutputSetup(OutputDescriptor descriptor)
            : base(descriptor)
        {
            SetMetadata(descriptor);
            this.descriptor = descriptor;
        }

        public new OutputDescriptor Descriptor { get { return descriptor; } }
        public MarkerSizes[] AvailableSizes { get { return availableSizes; } }
        public int[] AvailableThicknesses { get { return availableThicknesses; } }

        public Color LineColor
        {
            get { return lineColor; }
            set
            {
                this.lineColor = value;
                NotifyPropertyChanged("LineColor");
            }
        }

        public int LineThickness
        {
            get { return lineThickness; }
            set
            {
                this.lineThickness = value;
                NotifyPropertyChanged("LineThickness");
            }
        }

        public MarkerSizes MarkerSize
        {
            get { return markerSize; }
            set
            {
                this.markerSize = value;
                NotifyPropertyChanged(nameof(MarkerSize));
            }
        }

        public bool IsEnabled
        {
            get { return isEnabled; }
            set
            {
                this.isEnabled = value;
                NotifyPropertyChanged("IsEnabled");
            }
        }

        public override void CopyFrom(PropertySetupBase srcProperty)
        {
            var oldSetup = srcProperty as MarkerSeriesOutputSetup;
            if (oldSetup != null)
            {
                this.LineColor = oldSetup.LineColor;
                this.LineThickness = oldSetup.LineThickness;
                this.IsEnabled = oldSetup.IsEnabled;
            }
        }

        public override void Reset()
        {
            InitColor();
            InitThickness();
            MarkerSize = MarkerSizes.Medium;
            IsEnabled = !HasError;
        }

        private void InitColor()
        {
            if (descriptor.DefaultColor == Api.Colors.Auto)
                this.LineColor = System.Windows.Media.Colors.Green;
            else
            {
                int colorInt = (int)descriptor.DefaultColor;
                byte r = (byte)(colorInt >> 16);
                byte g = (byte)(colorInt >> 8);
                byte b = (byte)(colorInt >> 0);
                this.LineColor = Color.FromRgb(r, g, b);
            }
        }

        private void InitThickness()
        {
            int intThikness = (int)descriptor.DefaultThickness;
            if (intThikness < 1)
                intThikness = 1;
            if (intThikness > 5)
                intThikness = 5;
            this.LineThickness = intThikness;
        }
    }
}
