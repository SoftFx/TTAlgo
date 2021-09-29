using Microsoft.Xaml.Behaviors;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Data;

namespace TickTrader.BotAgent.Configurator
{
    public class Show2FACodeBehavior : Behavior<ButtonBase>
    {
        private readonly DelegateCommand _cmd;


        public static readonly DependencyProperty OtpSecretProperty = DependencyProperty.Register("OtpSecret", typeof(string), typeof(Show2FACodeBehavior),
            new FrameworkPropertyMetadata()
            {
                DefaultUpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
            });

        public static readonly DependencyProperty LoginProperty = DependencyProperty.Register("Login", typeof(string), typeof(Show2FACodeBehavior),
            new FrameworkPropertyMetadata()
            {
                DefaultUpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
            });


        public string OtpSecret
        {
            get => (string)GetValue(OtpSecretProperty);
            set => SetValue(OtpSecretProperty, value);
        }

        public string Login
        {
            get => (string)GetValue(LoginProperty);
            set => SetValue(LoginProperty, value);
        }


        public Show2FACodeBehavior()
        {
            _cmd = new DelegateCommand(_ => ShowDialog());
        }


        protected override void OnAttached()
        {
            base.OnAttached();

            if (AssociatedObject is ToggleButton toggleBtn)
            {
                toggleBtn.Click += ToggleBtnOnClick;
            }
            else
            {
                AssociatedObject.Command = _cmd;
            }
        }


        private void ShowDialog()
        {
            if (string.IsNullOrEmpty(OtpSecret))
                return;

            var dlg = new Show2FACodeDialog(OtpSecret, Login)
            {
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = Window.GetWindow(AssociatedObject)
            };
            dlg.ShowDialog();
        }

        private void ToggleBtnOnClick(object sender, RoutedEventArgs e)
        {
            // ToggleButton.Command is executed after this method
            // But to work properly we need to execute after OtpSecret generation
            Dispatcher.BeginInvoke((System.Action)ShowDialog);
        }
    }
}
