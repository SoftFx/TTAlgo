using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace TickTrader.BotTerminal
{
    public class ColorPicker : Control
    {
        #region Private Fields
        private bool isUdaptingColor;
        #endregion

        #region Constructors
        static ColorPicker()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ColorPicker), new FrameworkPropertyMetadata(typeof(ColorPicker)));
        }

        #endregion

        #region DependenciesProperty

        public static readonly DependencyProperty InsideBorderBrushProperty =
            DependencyProperty.Register(nameof(InsideBorderBrush), typeof(Brush), typeof(ColorPicker), new UIPropertyMetadata(Brushes.Black));

        public static readonly DependencyProperty InsideBorderThicknessProperty =
            DependencyProperty.Register(nameof(InsideBorderThickness), typeof(Thickness), typeof(ColorPicker), new UIPropertyMetadata(default(Thickness)));

        public static readonly DependencyProperty SelectedColorProperty =
            DependencyProperty.Register(nameof(SelectedColor), typeof(Color), typeof(ColorPicker),
                new UIPropertyMetadata(Colors.Purple, (d, e) => ((ColorPicker)d).updateHsv()));

        public static readonly DependencyProperty HueProperty =
            DependencyProperty.Register(nameof(Hue), typeof(double), typeof(ColorPicker),
                new UIPropertyMetadata((double)360, hueChanged));

        public static readonly DependencyProperty SaturationProperty =
            DependencyProperty.Register(nameof(Saturation), typeof(double), typeof(ColorPicker),
                new UIPropertyMetadata((d, e) => ((ColorPicker)d).updateColor()));

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(nameof(Value), typeof(double), typeof(ColorPicker),
                new UIPropertyMetadata((d, e) => ((ColorPicker)d).updateColor()));

        public static readonly DependencyPropertyKey OriginColorPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(OriginColor), typeof(Color), typeof(ColorPicker),
            new PropertyMetadata(Colors.Transparent));

        public static readonly DependencyProperty OriginColorProperty = OriginColorPropertyKey.DependencyProperty;

        #endregion

        #region Properties
        public Thickness InsideBorderThickness
        {
            get { return (Thickness)GetValue(InsideBorderThicknessProperty); }
            set { SetValue(InsideBorderThicknessProperty, value); }
        }

        public Brush InsideBorderBrush
        {
            get { return (Brush)GetValue(InsideBorderBrushProperty); }
            set { SetValue(InsideBorderBrushProperty, value); }
        }

        public Color SelectedColor
        {
            get { return (Color)GetValue(SelectedColorProperty); }
            set
            {
                SetValue(SelectedColorProperty, value);
            }
        }

        public Color OriginColor
        {
            get { return (Color)GetValue(OriginColorProperty); }
            protected set { SetValue(OriginColorPropertyKey, value); }
        }

        public double Hue
        {
            get { return (double)GetValue(HueProperty); }
            set
            {
                SetValue(HueProperty, value);
            }
        }

        public double Saturation
        {
            get { return (double)GetValue(SaturationProperty); }
            set
            {
                SetValue(SaturationProperty, value);
            }
        }

        public double Value
        {
            get { return (double)GetValue(ValueProperty); }
            set
            {
                SetValue(ValueProperty, value);
            }
        }
        #endregion

        #region Private Methods
        private static void hueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var cp = ((ColorPicker)d);
            cp.updateColor();
            cp.SetOriginColor();
        }

        private void SetOriginColor()
        {
            OriginColor = (new HsvColor(Hue, 1, 1)).ToRgb();
        }

        private void updateColor()
        {
            isUdaptingColor = true;
            SelectedColor = (new HsvColor(Hue, Saturation, Value)).ToRgb();
            isUdaptingColor = false;
        }

        private void updateHsv()
        {
            if (!isUdaptingColor)
            {
                var hsv = SelectedColor.ToHsv();
                Hue = hsv.H;
                Saturation = hsv.S;
                Value = hsv.V;
            }
        }
        #endregion

        public static LinearGradientBrush GetHorizontalGradientSpectrum
        {
            get
            {
                var brush = new LinearGradientBrush();
                brush.StartPoint = new Point(0, 0.5);
                brush.EndPoint = new Point(1, 0.5);
                brush.ColorInterpolationMode = ColorInterpolationMode.SRgbLinearInterpolation;

                var colorsList = GetSpectrumColors;
                double stopIncrement = (double)1 / colorsList.Count;

                int i;
                for (i = 0; i < colorsList.Count; i++)
                    brush.GradientStops.Add(new GradientStop(colorsList[i], i * stopIncrement));

                brush.GradientStops[i - 1].Offset = 1.0;
                return brush;
            }
        }

        public static LinearGradientBrush GetVerticalGradientSpectrum
        {
            get
            {
                var brush = new LinearGradientBrush();
                brush.StartPoint = new Point(0.5, 0);
                brush.EndPoint = new Point(0.5, 1);
                brush.ColorInterpolationMode = ColorInterpolationMode.SRgbLinearInterpolation;

                var colorsList = GetSpectrumColors;
                double stopIncrement = (double)1 / colorsList.Count;

                int i;
                for (i = 0; i < colorsList.Count; i++)
                    brush.GradientStops.Add(new GradientStop(colorsList[i], i * stopIncrement));

                brush.GradientStops[i - 1].Offset = 1.0;
                return brush;
            }
        }

        private static List<Color> GetSpectrumColors
        {
            get
            {
                var colorsList = new List<Color>(30);

                for (int i = 0; i < 29; i++)
                    colorsList.Add(new HsvColor(i * 12, 1, 1).ToRgb());

                colorsList.Add(new HsvColor(0, 1, 1).ToRgb());

                return colorsList;
            }
        }

        public static IEnumerable<Color> GetSystemColors
        {
            get
            {
                for (byte i = 1; i <= 5; i++)
                {
                    yield return new HsvColor(0, 0, 1 - i * 0.2).ToRgb();
                    yield return new HsvColor(36, i * 0.2, 1).ToRgb();
                    yield return new HsvColor(72, i * 0.2, 1).ToRgb();
                    yield return new HsvColor(108, i * 0.2, 1).ToRgb();
                    yield return new HsvColor(144, i * 0.2, 1).ToRgb();
                    yield return new HsvColor(180, i * 0.2, 1).ToRgb();
                    yield return new HsvColor(216, i * 0.2, 1).ToRgb();
                    yield return new HsvColor(252, i * 0.2, 1).ToRgb();
                    yield return new HsvColor(288, i * 0.2, 1).ToRgb();
                    yield return new HsvColor(360, i * 0.2, 1).ToRgb();
                }
            }
        }

    }
}
