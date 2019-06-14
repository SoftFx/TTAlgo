using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TickTrader.BotAgent.Configurator.Controls
{
    /// <summary>
    /// Interaction logic for ProtocolPanel.xaml
    /// </summary>
    public partial class ProtocolPanel : UserControl
    {
        private int _countErrors = 0;

        public ProtocolPanel()
        {
            InitializeComponent();
        }

        private void TextBox_Error(object sender, ValidationErrorEventArgs e)
        {
            _countErrors += e.Action == ValidationErrorEventAction.Added ? 1 : -1;

            CheckPortButton.IsEnabled = _countErrors <= 0;

            if (Window.GetWindow(this) is IErrorCounter parentWindows)
            {
                parentWindows.CountNumberErrors(sender, e);
            }
        }

        private void CheckPort_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is ProtocolViewModel model)
            {
                try
                {
                    model.CheckPort();
                    MessageBoxManager.OKBox("Port is avaible");
                }
                catch (WarningException ex)
                {
                    MessageBoxManager.WarningBox(ex.Message);
                }
                catch (Exception ex)
                {
                    MessageBoxManager.ErrorBox(ex.Message);
                }
            }
        }
    }
}
