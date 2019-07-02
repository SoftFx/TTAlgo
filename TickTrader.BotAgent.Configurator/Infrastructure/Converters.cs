using System;
using System.Globalization;
using System.ServiceProcess;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace TickTrader.BotAgent.Configurator
{
    public class ServiceStateToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ServiceControllerStatus status)
            {
                Brush color;

                switch (status)
                {
                    case ServiceControllerStatus.Running:
                        color = Brushes.Green;
                        break;
                    case ServiceControllerStatus.Stopped:
                        color = Brushes.Red;
                        break;
                    default:
                        color = Brushes.Yellow;
                        break;
                }

                return color;
            }
            else
                return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Visibility vis = Visibility.Collapsed;

            if (value is bool state && state)
                vis = Visibility.Visible;

            return vis;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
