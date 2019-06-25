using System.Windows;

namespace TickTrader.BotAgent.Configurator
{
    public class UpdateNewProperty : Freezable
    {
        public static readonly DependencyProperty OldValueProperty = DependencyProperty.Register("OldValue", typeof(string),
            typeof(UpdateNewProperty), new PropertyMetadata(false, new PropertyChangedCallback(IsOldValueChanged)));

        public static readonly DependencyProperty NewValueProperty = DependencyProperty.Register("NewValue", typeof(string),
            typeof(UpdateNewProperty), new PropertyMetadata(false, new PropertyChangedCallback(IsNewPropertyChanged)));

        public static readonly DependencyProperty WasUpdateProperty = DependencyProperty.Register("WasUpdate", typeof(bool),
           typeof(UpdateNewProperty));

        public string OldValue
        {
            get => (string)GetValue(OldValueProperty);
            set => SetValue(OldValueProperty, value);
        }

        public string NewValue
        {
            get => (string)GetValue(NewValueProperty);
            set => SetValue(NewValueProperty, value);
        }

        public bool WasUpdate
        {
            get => (bool)GetValue(WasUpdateProperty);
            set => SetValue(WasUpdateProperty, value);
        }

        private static void IsNewPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs args)
        {
            var stateObj = (UpdateNewProperty)d;

            stateObj.WasUpdate = (string)args.NewValue == stateObj.OldValue;
        }

        private static void IsOldValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs args)
        {
            var stateObj = (UpdateNewProperty)d;

            stateObj.NewValue = (string)args.NewValue;
        }

        protected override Freezable CreateInstanceCore()
        {
            return new UpdateNewProperty();
        }
    }
}
