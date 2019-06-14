using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace TickTrader.BotTerminal
{
    public class ErrorIndicator : Control
    {
        public static readonly DependencyProperty ErrorProperty =
            DependencyProperty.Register(nameof(Error), typeof(string), typeof(ErrorIndicator),
                new FrameworkPropertyMetadata(null, OnErrorChanged));

        public static readonly DependencyPropertyKey ErrorIconVisibilityPropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(ErrorIconVisibility), typeof(Visibility), typeof(ErrorIndicator),
                new FrameworkPropertyMetadata(Visibility.Hidden));

        public static readonly DependencyProperty ErrorIconVisibilityProperty = ErrorIconVisibilityPropertyKey.DependencyProperty;

        public string Error
        {
            get => (string)GetValue(ErrorProperty);
            set => SetValue(ErrorProperty, value);
        }

        public Visibility ErrorIconVisibility
        {
            get => (Visibility)GetValue(ErrorIconVisibilityProperty);
            private set => SetValue(ErrorIconVisibilityPropertyKey, value);
        }

        private static void OnErrorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var self = (ErrorIndicator)d;
            self.ErrorIconVisibility = string.IsNullOrEmpty(self.Error) ? Visibility.Hidden : Visibility.Visible;
        }
    }
}
