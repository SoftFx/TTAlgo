using System.Windows;
using System.Windows.Controls;

namespace TickTrader.BotAgent.Configurator.Controls
{
    public partial class SslPanel : UserControl
    {
        public SslPanel()
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
    }
}
