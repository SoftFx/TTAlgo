using System;
using System.Windows;
using Xceed.Wpf.AvalonDock;
using Xceed.Wpf.AvalonDock.Layout.Serialization;
using System.IO;
using NLog;
using System.Linq;
using System.Collections.Generic;
using Xceed.Wpf.AvalonDock.Layout;
using System.ComponentModel;

namespace TickTrader.BotTerminal
{
    public class CustomDockManager : DockingManager
    {
        private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

        private Dictionary<string, LayoutAnchorable> _anchorableViews;

        private static CustomDockManager customDockManager;

        public static CustomDockManager GetInstance()
        {
            if (customDockManager is null)
                return new CustomDockManager();
            else
                return customDockManager;
        }


        public static readonly DependencyProperty DockManagerServiceProperty =
            DependencyProperty.Register("DockManagerService", typeof(IDockManagerServiceProvider), typeof(CustomDockManager), new PropertyMetadata(null, OnDockManagerServicePropertyChanged));

        public IDockManagerServiceProvider DockManagerService
        {
            get { return (IDockManagerServiceProvider)GetValue(DockManagerServiceProperty); }
            set { SetValue(DockManagerServiceProperty, value); }
        }

        private static void OnDockManagerServicePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var dockManager = d as CustomDockManager;
            var oldService = e.OldValue as IDockManagerServiceProvider;
            var newService = e.NewValue as IDockManagerServiceProvider;

            if (dockManager != null)
            {
                if (oldService != null)
                {
                    oldService.SaveLayoutEvent -= dockManager.SaveLayout;
                    oldService.LoadLayoutEvent -= dockManager.LoadLayout;
                    oldService.ToggleViewEvent -= dockManager.ToggleView;
                }

                if (newService != null)
                {
                    newService.SaveLayoutEvent += dockManager.SaveLayout;
                    newService.LoadLayoutEvent += dockManager.LoadLayout;
                    newService.ToggleViewEvent += dockManager.ToggleView;
                }

                dockManager.UpdateViewsVisibility();
            }
        }


        public CustomDockManager()
        {
            if (!(customDockManager is null))
                throw new Exception("Instance of class has been exist!");

            _anchorableViews = new Dictionary<string, LayoutAnchorable>();

            Loaded += (sender, args) => FindAnchorableViews();

            customDockManager = this;
        }


        private void FindAnchorableViews()
        {
            foreach (var view in _anchorableViews.Values)
            {
                view.PropertyChanged -= OnLayoutAnchorablePropertyChanged;
            }

            _anchorableViews.Clear();
            CheckLayoutContainer(Layout);

            foreach (var view in _anchorableViews.Values)
            {
                view.PropertyChanged += OnLayoutAnchorablePropertyChanged;
            }

            UpdateViewsVisibility();
        }

        private void CheckLayoutContainer(ILayoutContainer container)
        {
            foreach (var child in container.Children)
            {
                if (child is ILayoutContainer)
                {
                    CheckLayoutContainer((ILayoutContainer)child);
                }
                if (child is LayoutAnchorable)
                {
                    var view = (LayoutAnchorable)child;
                    _anchorableViews[view.ContentId] = view;
                }
            }
        }

        private void OnLayoutAnchorablePropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if (sender is LayoutAnchorable && args.PropertyName == "IsHidden")
            {
                var view = (LayoutAnchorable)sender;
                DockManagerService?.SetViewVisibility(view.ContentId, !view.IsHidden);
            }
        }

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
                FindAnchorableViews();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to load DockManager layout");
            }
        }

        private void ToggleView(string contentId)
        {
            if (_anchorableViews.ContainsKey(contentId))
            {
                var view = _anchorableViews[contentId];
                if (view.IsHidden)
                {
                    view.Show();
                }
                else
                {
                    view.Hide();
                }
            }
        }

        private void UpdateViewsVisibility()
        {
            foreach (var view in _anchorableViews.Values)
            {
                DockManagerService.SetViewVisibility(view.ContentId, !view.IsHidden);
            }
        }
    }
}
