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
using Caliburn.Micro;

namespace TickTrader.BotTerminal
{
    internal class CustomDockManager : DockingManager
    {
        private static readonly ILogger _logger = NLog.LogManager.GetCurrentClassLogger();

        private Dictionary<string, LayoutAnchorable> _anchorableViews;


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
                    oldService.ShowViewEvent -= dockManager.ShowView;
                    oldService.RemoveViewEvent -= dockManager.RemoveView;
                    oldService.RemoveViewsEvent-= dockManager.RemoveViews;
                }

                if (newService != null)
                {
                    newService.SaveLayoutEvent += dockManager.SaveLayout;
                    newService.LoadLayoutEvent += dockManager.LoadLayout;
                    newService.ToggleViewEvent += dockManager.ToggleView;
                    newService.ShowViewEvent += dockManager.ShowView;
                    newService.RemoveViewEvent += dockManager.RemoveView;
                    newService.RemoveViewsEvent += dockManager.RemoveViews;
                }

                dockManager.UpdateViewsVisibility();
            }
        }


        public CustomDockManager()
        {
            _anchorableViews = new Dictionary<string, LayoutAnchorable>();

            Loaded += (sender, args) => FindAnchorableViews();
        }


        private void FindAnchorableViews()
        {
            foreach (var view in _anchorableViews.Values)
            {
                view.PropertyChanged -= OnLayoutAnchorablePropertyChanged;
                (view.Content as IScreen)?.Deactivate(true);
            }

            _anchorableViews.Clear();
            CheckLayoutContainer(Layout);

            foreach (var view in _anchorableViews.Values)
            {
                if (view.IsVisible)
                {
                    //InitView(view);
                    (view.Content as IScreen)?.Activate();
                }
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
                DockManagerService?.ViewVisibilityChanged(view.ContentId, !view.IsHidden);
                var screen = view.Content as IScreen;
                if (screen != null)
                {
                    if (view.IsHidden)
                    {
                        screen.Deactivate(false);
                    }
                    else
                    {
                        screen.Activate();
                    }
                }
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
                layoutSerializer.LayoutSerializationCallback += LoadLayoutCallback;
                layoutSerializer.Deserialize(stream);
                layoutSerializer.LayoutSerializationCallback -= LoadLayoutCallback;
                FindAnchorableViews();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to load DockManager layout");
            }
        }

        private void LoadLayoutCallback(object senser, LayoutSerializationCallbackEventArgs args)
        {
            if (DockManagerService.ShouldClose(args.Model.ContentId))
            {
                var screen = DockManagerService.GetScreen(args.Model.ContentId);
                if (screen != null)
                {
                    args.Model.Title = screen.DisplayName;
                    args.Content = screen;
                }
                else
                {
                    args.Model.Close();
                }
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

        private void ShowView(string contentId)
        {
            if (!_anchorableViews.ContainsKey(contentId))
            {
                var view = new LayoutAnchorable { ContentId = contentId, FloatingHeight = 400, FloatingWidth = 600, FloatingTop = 100, FloatingLeft = 100 };
                InitView(view);
                _anchorableViews.Add(contentId, view);
                view.PropertyChanged += OnLayoutAnchorablePropertyChanged;
                view.AddToLayout(this, AnchorableShowStrategy.Right);
                view.Float();
            }
            else
            {
                var view = _anchorableViews[contentId];
                if (view.Content == null)
                    InitView(view);
                view.Show();
            }
        }

        private void RemoveView(string contentId)
        {
            if (_anchorableViews.ContainsKey(contentId))
            {
                var view = _anchorableViews[contentId];
                view.Parent.RemoveChild(view);
                view.PropertyChanged -= OnLayoutAnchorablePropertyChanged;
                (view.Content as IScreen)?.Deactivate(true);
                _anchorableViews.Remove(contentId);
            }
        }

        private void RemoveViews()
        {
            var viewsToClose = _anchorableViews.Values.Where(v => DockManagerService.ShouldClose(v.ContentId)).ToList();
            foreach (var view in viewsToClose)
            {
                view.PropertyChanged -= OnLayoutAnchorablePropertyChanged;
                (view.Content as IScreen)?.Deactivate(true);
                _anchorableViews.Remove(view.ContentId);
                view.Close();
            }
        }

        private void UpdateViewsVisibility()
        {
            foreach (var view in _anchorableViews.Values)
            {
                DockManagerService.ViewVisibilityChanged(view.ContentId, !view.IsHidden);
            }
        }

        private void InitView(LayoutAnchorable view)
        {
            var screen = DockManagerService.GetScreen(view.ContentId);
            if (screen != null)
            {
                view.Title = screen.DisplayName;
                view.Content = screen;
            }
        }
    }
}
