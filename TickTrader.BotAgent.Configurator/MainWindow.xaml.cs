using System.Windows;

namespace TickTrader.BotAgent.Configurator
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            DataContext = new ConfigurationViewModel();
            InitializeComponent();
        }
    }
}
