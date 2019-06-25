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

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            restartAppButton.IsEnabled = e.AddedItems[0] as string != (DataContext as ConfigurationViewModel).AdvancedModel.InitialSelectedPath;
        }
    }
}
