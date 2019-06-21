using System.Windows;
using System.Windows.Controls;

namespace TickTrader.BotAgent.Configurator.Controls
{
    public partial class AdvancedPanel : UserControl
    {
        public AdvancedPanel()
        {
            InitializeComponent();
        }

        private void RestartApplicationButton(object sender, RoutedEventArgs e)
        {
            if (Window.GetWindow(this) is MainWindow parentWindow)
            {
                parentWindow.RestartApplication();
            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataContext is AdvancedViewModel model)
            {
                restartAppButton.IsEnabled = e.AddedItems[0] as string != model.InitialSelectedPath;
            }           
        }
    }
}
