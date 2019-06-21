using System.Windows;
using System.Windows.Controls;

namespace TickTrader.BotAgent.Configurator.Controls
{
    /// <summary>
    /// Interaction logic for ProtocolPanel.xaml
    /// </summary>
    public partial class ProtocolPanel : UserControl
    {
        public ProtocolPanel()
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
