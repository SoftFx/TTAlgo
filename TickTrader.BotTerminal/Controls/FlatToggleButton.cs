﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace TickTrader.BotTerminal
{
    public class FlatToggleButton : ToggleButton
    {
        public static readonly DependencyProperty CornerRadiusProperty =
           DependencyProperty.Register(nameof(CornerRadius), typeof(CornerRadius), typeof(FlatToggleButton), new FrameworkPropertyMetadata(default(CornerRadius)));

        public static DependencyProperty HighlightBackgroundProperty =
           DependencyProperty.Register("HighlightBackground", typeof(Brush), typeof(FlatToggleButton));

        public static DependencyProperty DisabledBackgroundProperty =
            DependencyProperty.Register("DisabledBackground", typeof(Brush), typeof(FlatToggleButton));

        public static DependencyProperty CheckedBackgroundProperty =
            DependencyProperty.Register("CheckedBackground", typeof(Brush), typeof(FlatToggleButton));

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
