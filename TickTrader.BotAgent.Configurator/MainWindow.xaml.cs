using System.Windows;
using System.Windows.Controls;

namespace TickTrader.BotAgent.Configurator
{
    public partial class MainWindow : Window, IErrorCounter
    {
        private ConfigurationViewModel _viewModel;
        private int _countErrors = 0;

        public MainWindow()
        {
            InitializeComponent();

            try
            {
                _viewModel = new ConfigurationViewModel();
                DataContext = _viewModel;
            }
            catch
            {
                Close();
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.SaveChanges();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _viewModel.CancelChanges();
            }
            catch
            {
                Close();
            }
        }

        public void CountNumberErrors(object sender, ValidationErrorEventArgs e)
        {
            _countErrors += e.Action == ValidationErrorEventAction.Added ? 1 : -1;
            SaveButton.IsEnabled = _countErrors <= 0;
        }
    }

    public interface IErrorCounter
    {
        void CountNumberErrors(object obj, ValidationErrorEventArgs e);
    }
}
