using System.Windows;

namespace TickTrader.WpfWindowsSupportLibrary
{
    public static class MessageBoxManager
    {
        private static readonly (string title, MessageBoxImage icon) ErrorAttributes = ("Error", MessageBoxImage.Error);
        private static readonly (string title, MessageBoxImage icon) InfoAttributes = ("Info", MessageBoxImage.Information);
        private static readonly (string title, MessageBoxImage icon) WarningAttributes = ("Warning", MessageBoxImage.Warning);


        public static void OkError(string message) => OkWindow(message, ErrorAttributes);

        public static void OkInfo(string message) => OkWindow(message, InfoAttributes);

        public static void OkWarningBox(string message) => OkWindow(message, WarningAttributes);

        private static void OkWindow(string message, (string, MessageBoxImage) setting) => GetMessageBoxResult(message, MessageBoxButton.OK, setting);


        public static bool YesNoBoxError(string message) => YesNoWindow(message, ErrorAttributes);

        public static bool YesNoBoxWarning(string message) => YesNoWindow(message, WarningAttributes);

        public static bool YesNoBoxQuestion(string message, string header = "") => YesNoWindow(message, (header, MessageBoxImage.Question));

        private static bool YesNoWindow(string message, (string, MessageBoxImage) setting) => GetMessageBoxResult(message, MessageBoxButton.YesNo, setting) == MessageBoxResult.Yes;


        public static bool OkCancelBoxQuestion(string message, string header = "")
        {
            var result = GetMessageBoxResult(message, MessageBoxButton.OKCancel, (header, MessageBoxImage.Question));

            return result == MessageBoxResult.OK;
        }

        public static MessageBoxResult YesNoCancelQuestion(string message, string header = "")
        {
            return GetMessageBoxResult(message, MessageBoxButton.YesNoCancel, (header, MessageBoxImage.Question));
        }


        private static MessageBoxResult GetMessageBoxResult(string message, MessageBoxButton button, (string, MessageBoxImage) setting)
        {
            if (Application.Current?.MainWindow == null)
                return MessageBox.Show(message, setting.Item1, button, setting.Item2);
            else
                return MessageBox.Show(Application.Current.MainWindow, message, setting.Item1, button, setting.Item2);
        }
    }
}
