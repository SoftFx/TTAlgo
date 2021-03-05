using System.Windows;

namespace TickTrader.BotAgent.Configurator
{
    static public class MessageBoxManager
    {
        public static void OkError(string message)
        {
            MessageBox.Show(Application.Current.MainWindow, message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public static void OkInfo(string message)
        {
            MessageBox.Show(Application.Current.MainWindow, message, "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public static bool YesNoBoxError(string message)
        {
            var result = MessageBox.Show(Application.Current.MainWindow, message, "Error", MessageBoxButton.YesNo, MessageBoxImage.Error);

            return result == MessageBoxResult.Yes;
        }

        public static bool YesNoBoxQuestion(string message, string header = "")
        {
            var result = MessageBox.Show(Application.Current.MainWindow, message, header, MessageBoxButton.YesNo, MessageBoxImage.Question);

            return result == MessageBoxResult.Yes;
        }

        public static bool OkCancelBoxQuestion(string message, string header = "")
        {
            var result = MessageBox.Show(Application.Current.MainWindow, message, header, MessageBoxButton.OKCancel, MessageBoxImage.Question);

            return result == MessageBoxResult.OK;
        }

        public static MessageBoxResult YesNoCancelQuestion(string message, string header = "")
        {
            return MessageBox.Show(Application.Current.MainWindow, message, header, MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
        }

        public static void WarningBox(string message)
        {
            MessageBox.Show(Application.Current.MainWindow, message, "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }
}
