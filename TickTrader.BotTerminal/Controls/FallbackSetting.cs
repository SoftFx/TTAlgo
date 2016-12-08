using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace TickTrader.BotTerminal
{
    public class FallbackSetting : DependencyObject
    {
        private bool isSet;
        private bool isDefaultBeingSet;

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(nameof(Value), typeof(object), typeof(FallbackSetting),
                 new PropertyMetadata(ValueChanged));

        public static readonly DependencyProperty DefaultValueProperty =
                DependencyProperty.Register(nameof(DefaultValue), typeof(object), typeof(FallbackSetting),
                new PropertyMetadata(DefaultValueChanged));

        public object Value
        {
            get { return GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public object DefaultValue
        {
            get { return GetValue(DefaultValueProperty); }
            set { SetValue(DefaultValueProperty, value); }
        }

        private static void ValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var setting = (FallbackSetting)d;

            if (setting.isDefaultBeingSet)
                return;

            if (e.NewValue != null && e.NewValue != DependencyProperty.UnsetValue)
                setting.isSet = true;
            else
                setting.Clear();
        }

        private static void DefaultValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var setting = (FallbackSetting)d;
            if (!setting.isSet)
            {
                try
                {
                    setting.isDefaultBeingSet = true;
                    setting.Value = e.NewValue;
                }
                finally
                {
                    setting.isDefaultBeingSet = false;
                }
            }
        }

        public void Clear()
        {
            isSet = false;
            Value = DefaultValue;
        }
    }
}
