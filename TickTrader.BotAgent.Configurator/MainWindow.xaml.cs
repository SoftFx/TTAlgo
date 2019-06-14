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

            _viewModel.RefreshManager.NewValuesEvent += EnableChangeStateButton;
            _viewModel.RefreshManager.SaveValuesEvent += DisableChangeStateButton;
            Closing += MainWindow_Closing;
        }

        public void CountNumberErrors(object sender, ValidationErrorEventArgs e)
        {
            _countErrors += e.Action == ValidationErrorEventAction.Added ? 1 : -1;

            SaveButton.IsEnabled = _countErrors <= 0;
            StartButton.IsEnabled = _countErrors <= 0;
            CancelButton.IsEnabled = true;

        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _viewModel.SaveChanges();
                SaveButton.IsEnabled = false;
                MessageBoxManager.OKBox("Saving configuration successfully!");
            }
            catch (Exception ex)
            {
                MessageBoxManager.ErrorBox(ex.Message);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBoxManager.YesNoBoxQuestion("The model has been changed. Сancel changes?"))
            {
                try
                {
                    _viewModel.CancelChanges();
                    DisableChangeStateButton();
                }
                catch (Exception ex)
                {
                    MessageBoxManager.ErrorBox(ex.Message);
                    Close();
                }
            }
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if (SaveChangesQuestion())
            {
                try
                {
                    if (_viewModel.StartAgent())
                        MessageBoxManager.OKBox("Agent has been started!");
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

        private bool SaveChangesQuestion()
        {
            if (_viewModel.WasUpdate)
            {
                var result = MessageBoxManager.YesNoBoxQuestion("The model has been changed. Save changes?");

                if (result)
                    _viewModel.SaveChanges();

                return result;
            }

            return !_viewModel.WasUpdate;
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                SaveChangesQuestion();
            }
            catch
            {
                MessageBoxManager.ErrorBox("Saving settings was failed");
            }
            finally
            {
                _viewModel.Dispose();
            }
        }

        private void EnableChangeStateButton()
        {
            CancelButton.IsEnabled = true;
            SaveButton.IsEnabled = _countErrors <= 0;
        }

        private void DisableChangeStateButton()
        {
            CancelButton.IsEnabled = false;
            SaveButton.IsEnabled = false;
        }
    }

    public interface IErrorCounter
    {
        void CountNumberErrors(object obj, ValidationErrorEventArgs e);
    }
}
