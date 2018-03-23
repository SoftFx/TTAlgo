using System;
using System.Windows;
using Xceed.Wpf.AvalonDock;
using Xceed.Wpf.AvalonDock.Layout.Serialization;
using System.IO;
using NLog;
using System.Linq;

namespace TickTrader.BotTerminal
{
    public class CustomDockManager : DockingManager
    {
        private static ILogger _logger = LogManager.GetCurrentClassLogger();


        public static readonly DependencyProperty DockManagerServiceProperty =
            DependencyProperty.Register("DockManagerService", typeof(DockManagerService), typeof(CustomDockManager), new PropertyMetadata(null, OnDockManagerServicePropertyChanged));

        public DockManagerService DockManagerService
        {
            get { return (DockManagerService)GetValue(DockManagerServiceProperty); }
            set { SetValue(DockManagerServiceProperty, value); }
        }

        private static void OnDockManagerServicePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var dockManager = d as CustomDockManager;
            var oldService = e.OldValue as DockManagerService;
            var newService = e.NewValue as DockManagerService;

            if (dockManager != null)
            {
                if (oldService != null)
                {
                    oldService.SaveLayoutEvent -= dockManager.SaveLayout;
                    oldService.LoadLayoutEvent -= dockManager.LoadLayout;
                    oldService.ShowHiddenTabEvent -= dockManager.ShowHiddenTab;
                }

                if (newService != null)
                {
                    newService.SaveLayoutEvent += dockManager.SaveLayout;
                    newService.LoadLayoutEvent += dockManager.LoadLayout;
                    newService.ShowHiddenTabEvent += dockManager.ShowHiddenTab;
                }
            }
        }


        public CustomDockManager() { }


        private void SaveLayout(Stream stream)
        {
            try
            {
                var layoutSerializer = new XmlLayoutSerializer(this);
                layoutSerializer.Serialize(stream);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to save DockManager layout");
            }
        }

        private void LoadLayout(Stream stream)
        {
            try
            {
                var layoutSerializer = new XmlLayoutSerializer(this);
                layoutSerializer.Deserialize(stream);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to load DockManager layout");
            }
        }

        private void ShowHiddenTab(string contentId)
        {
            Layout.Hidden.FirstOrDefault(a => a.ContentId == contentId)?.Show();
        }
    }
}
