using System.Windows;
using System.Windows.Controls;

namespace TickTrader.BotAgent.Configurator.Controls
{
    /// <summary>
    /// Interaction logic for CredentialPanel.xaml
    /// </summary>
    public partial class CredentialPanel : UserControl
    {
        public CredentialPanel()
        {
            InitializeComponent();
        }

        private void TextBox_Error(object sender, ValidationErrorEventArgs e)
        {
            if (Window.GetWindow(this) is IErrorCounter parentWindows)
            {
                parentWindows.CountNumberErrors(sender, e);
            }
        }

        private void Generate_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is CredentialViewModel model)
            {
                if ((sender as Button).Name.Contains("Login"))
                    model.GeneratenewLogin();
                else
                    model.GenerateNewPassword();
            }
        }
    }
}
