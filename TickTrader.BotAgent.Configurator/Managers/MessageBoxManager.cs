using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace TickTrader.BotAgent.Configurator
{
    static public class MessageBoxManager
    {
        public static void ErrorBox(string message)
        {
            MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public static void OKBox(string message)
        {
            MessageBox.Show(message, "", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public static bool YesNoBox(string message)
        {
            var result = MessageBox.Show(message, "Error", MessageBoxButton.YesNo, MessageBoxImage.Error);

            return result == MessageBoxResult.Yes;
        }
    }
}
