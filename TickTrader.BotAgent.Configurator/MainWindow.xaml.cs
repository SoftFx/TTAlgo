using System.Windows;
using System.Windows.Controls;

namespace TickTrader.BotAgent.Configurator
{
    public partial class MainWindow : Window
    {
        private int _countErrors = 0;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = new ConfigurationViewModel();
        }

        public void CountNumberErrors(object sender, ValidationErrorEventArgs e)
        {
            _countErrors += e.Action == ValidationErrorEventAction.Added ? 1 : -1;

            StartButton.IsEnabled = _countErrors <= 0;
            CancelButton.IsEnabled = true;
            SaveButton.IsEnabled = true;
        }
    }
}
