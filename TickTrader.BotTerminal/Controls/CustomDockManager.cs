using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Threading.Tasks;
using Xceed.Wpf.AvalonDock;
using Xceed.Wpf.AvalonDock.Layout.Serialization;
using System.IO;

namespace TickTrader.BotTerminal.Controls
{
    public class CustomDockManager : DockingManager
    {
        public static readonly DependencyProperty LoadSaveNotificationProperty =
            DependencyProperty.Register("LoadSaveNotification", typeof(DockManagerNotification), typeof(CustomDockManager), new PropertyMetadata(null, OnLoadSaveNotificationPropertyChanged));

        public DockManagerNotification LoadSaveNotification
        {
            get { return (DockManagerNotification)GetValue(LoadSaveNotificationProperty); }            
            set { SetValue(LoadSaveNotificationProperty, value); }
        }

        private static void OnLoadSaveNotificationPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var value = e.NewValue as DockManagerNotification;
            value.SaveLayoutEvent += () =>
            {
                XmlLayoutSerializer layoutSerializer = new XmlLayoutSerializer((CustomDockManager)d);
                using (var writer = new StreamWriter("test"))
                {
                    layoutSerializer.Serialize(writer);
                }
            };

            value.LoadLayoutEvent += () =>
            {
                XmlLayoutSerializer layoutSerializer = new XmlLayoutSerializer((CustomDockManager)d);
                using (var reader = new StreamReader("test"))
                {
                    layoutSerializer.Deserialize(reader);
                }
            };
        }

        public CustomDockManager()
        {
            
        }
    }

    public class DockManagerNotification
    {
        public event Action SaveLayoutEvent = delegate { };
        public event Action LoadLayoutEvent = delegate { };

        public void SaveLayout()
        {
            SaveLayoutEvent();
        }

        public void LoadLayout()
        {
            LoadLayoutEvent();
        }
    }
}
