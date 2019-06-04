using System;
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
            catch (Exception ex)
            {
                MessageBoxManager.ErrorBox(ex.Message);
                Close();
            }
        }

        public void CountNumberErrors(object sender, ValidationErrorEventArgs e)
        {
            _countErrors += e.Action == ValidationErrorEventAction.Added ? 1 : -1;
            SaveButton.IsEnabled = _countErrors <= 0;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _viewModel.SaveChanges();
            }
            catch (Exception ex)
            {
                MessageBoxManager.ErrorBox(ex.Message);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _viewModel.CancelChanges();
            }
            catch (Exception ex)
            {
                MessageBoxManager.ErrorBox(ex.Message);
                Close();
            }
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _viewModel.StartAgent();
            }
            catch (WarningException ex)
            {
                MessageBoxManager.WarningBox(ex.Message);
            }
            catch (Exception exx)
            {
                MessageBoxManager.ErrorBox(exx.Message);
            }
        }
    }

    public interface IErrorCounter
    {
        void CountNumberErrors(object obj, ValidationErrorEventArgs e);
    }
}
