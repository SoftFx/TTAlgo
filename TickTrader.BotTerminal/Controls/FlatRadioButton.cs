using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace TickTrader.BotTerminal
{
    public class FlatRadioButton: RadioButton
    {
        public static readonly DependencyProperty CornerRadiusProperty =
           DependencyProperty.Register(nameof(CornerRadius), typeof(CornerRadius), typeof(FlatRadioButton), new FrameworkPropertyMetadata(default(CornerRadius)));

        public static DependencyProperty HighlightBackgroundProperty =
           DependencyProperty.Register(nameof(HighlightBackground), typeof(Brush), typeof(FlatRadioButton));

        public static DependencyProperty DisabledBackgroundProperty =
            DependencyProperty.Register(nameof(DisabledBackground), typeof(Brush), typeof(FlatRadioButton));

        public static DependencyProperty CheckedBackgroundProperty =
            DependencyProperty.Register(nameof(CheckedBackground), typeof(Brush), typeof(FlatRadioButton));

        public CornerRadius CornerRadius
        {
            get { return (CornerRadius)GetValue(CornerRadiusProperty); }
            set { SetValue(CornerRadiusProperty, value); }
        }

        public Brush HighlightBackground
        {
            get { return (Brush)GetValue(HighlightBackgroundProperty); }
            set { SetValue(HighlightBackgroundProperty, value); }
        }

        public Brush DisabledBackground
        {
            get { return (Brush)GetValue(DisabledBackgroundProperty); }
            set { SetValue(DisabledBackgroundProperty, value); }
        }

        public Brush CheckedBackground
        {
            get { return (Brush)GetValue(CheckedBackgroundProperty); }
            set { SetValue(CheckedBackgroundProperty, value); }
        }
    }
}
