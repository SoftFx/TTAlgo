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
        public static readonly DependencyProperty ManagerNotificationProperty =
            DependencyProperty.Register("ManagerNotification", typeof(DockManagerNotification), typeof(CustomDockManager), new PropertyMetadata(null, OnManagerNotificationPropertyChanged));

        public DockManagerNotification ManagerNotification
        {
            get { return (DockManagerNotification)GetValue(ManagerNotificationProperty); }            
            set { SetValue(ManagerNotificationProperty, value); }
        }

        private static void OnManagerNotificationPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
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

            value.ShowHiddenTabEvent += (o) =>
            {
                var hiddenTabs = ((CustomDockManager)d).Layout.Hidden.Where(obj => { return obj.ContentId.Equals(o); });
                if(hiddenTabs.Count() != 0)
                    hiddenTabs.First().Show();
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
        public event Action<string> ShowHiddenTabEvent = delegate { };

        public void SaveLayout()
        {
            SaveLayoutEvent();
        }

        public void LoadLayout()
        {
            LoadLayoutEvent();
        }

        public void ShowHiddenTab(string tab)
        {
            ShowHiddenTabEvent(tab);
        }
    }
}
