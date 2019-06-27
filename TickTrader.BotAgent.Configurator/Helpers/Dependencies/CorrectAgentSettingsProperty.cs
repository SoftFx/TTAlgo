using System.Windows;

namespace TickTrader.BotAgent.Configurator
{
    public class CorrectAgentSettingsProperty : Freezable
    {
        private static int _countErrors;

        public static readonly DependencyProperty ErrorProperty = DependencyProperty.Register("Error", typeof(bool),
            typeof(CorrectAgentSettingsProperty), new PropertyMetadata(false, new PropertyChangedCallback(SetNewError)));

        public static readonly DependencyProperty EnableStartProperty = DependencyProperty.Register("EnableStart", typeof(bool),
            typeof(CorrectAgentSettingsProperty), new PropertyMetadata(true));

        public bool Error
        {
            get => (bool)GetValue(ErrorProperty);
            set => SetValue(ErrorProperty, value);
        }

        public bool EnableStart
        {
            get => (bool)GetValue(ErrorProperty);
            set => SetValue(ErrorProperty, value);
        }

        private static void SetNewError(DependencyObject d, DependencyPropertyChangedEventArgs args)
        {
            var stateObj = (CorrectAgentSettingsProperty)d;

            _countErrors += (bool)args.NewValue ? 1 : -1;

            stateObj.EnableStart = _countErrors <= 0;
        }

        protected override Freezable CreateInstanceCore()
        {
            return new CorrectAgentSettingsProperty();
        }
    }
}
