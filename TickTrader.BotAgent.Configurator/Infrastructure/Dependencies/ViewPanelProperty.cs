using System.Windows;

namespace TickTrader.BotAgent.Configurator
{
    public class HiddenPanelProperty : Freezable
    {
        public static readonly DependencyProperty IsShowPanel = DependencyProperty.Register("IsShow", typeof(bool),
           typeof(HiddenPanelProperty), new PropertyMetadata(false, new PropertyChangedCallback(IsShownPropertyChanged)));

        public static readonly DependencyProperty VisibilityProperty = DependencyProperty.Register("Visibility", typeof(Visibility),
           typeof(HiddenPanelProperty), new PropertyMetadata(Visibility.Collapsed));

        public bool IsShow
        {
            get => (bool)GetValue(IsShowPanel);
            set => SetValue(IsShowPanel, value);
        }

        public Visibility Visibility
        {
            get => (Visibility)GetValue(VisibilityProperty);
            set => SetValue(VisibilityProperty, value);
        }

        private static void IsShownPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs args)
        {
            var stateObj = (HiddenPanelProperty)d;

            stateObj.Visibility = (bool)args.NewValue ? Visibility.Visible : Visibility.Collapsed;
        }

        protected override Freezable CreateInstanceCore()
        {
            return new HiddenPanelProperty();
        }
    }
}
