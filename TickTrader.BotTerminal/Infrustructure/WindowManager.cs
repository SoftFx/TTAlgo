﻿using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using TickTrader.Algo.Core.Lib;

namespace TickTrader.BotTerminal
{
    public class WindowManager
    {
        private readonly Dictionary<object, IScreen> _wndModels = new();
        private readonly CaliburnAdapter _adapter = new();
        private readonly IViewAware _containerModel;

        public WindowManager(IViewAware root)
        {
            _containerModel = root;
        }

        public async void OpenWindow(IScreen wndModel)
        {
            var window = await _adapter.GetOrCreateMdiWindow(wndModel);
            window.Show();
            window.Activate();
        }

        public async void OpenMdiWindow(IScreen wndModel)
        {
            var window = await _adapter.GetOrCreateMdiWindow(wndModel, MdiSetup);
            window.Show();
        }

        public static void ShowError(string message, IViewAware contextModel = null, string caption = null)
        {
            var view = contextModel?.GetView() as DependencyObject;
            var window = view != null ? Window.GetWindow(view) : null;
            if (window != null)
                System.Windows.MessageBox.Show(window, message, caption, MessageBoxButton.OK, MessageBoxImage.Error);
            else
                System.Windows.MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void MdiSetup(Window wnd)
        {
            wnd.ShowInTaskbar = false;
            wnd.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            var parentWnd = _containerModel.GetView() as Window;
            if (parentWnd != null)
                wnd.Owner = parentWnd;
        }

        public void OpenOrActivateWindow(object wndKey, Func<IScreen> wndModelfactory)
        {
            IScreen existing = GetWindowModel(wndKey);
            if (existing != null)
                _adapter.ActivateWindow(existing);
            else
                OpenMdiWindow(wndKey, wndModelfactory());
        }

        public void OpenMdiWindow(object wndKey, IScreen wndModel)
        {
            IScreen existing = GetWindowModel(wndKey);
            if (existing != null)
            {
                if (existing != wndModel)
                    existing.TryCloseAsync();
                else
                    existing.ActivateAsync();
            }
            wndModel.Deactivated += WndModel_Deactivated;
            _wndModels[wndKey] = wndModel;
            OpenMdiWindow(wndModel);
        }

        private IScreen GetWindowModel(object key)
        {
            return _wndModels.GetOrDefault(key);
        }

        private Task WndModel_Deactivated(object sender, DeactivationEventArgs e)
        {
            if (e.WasClosed)
            {
                var wndModel = sender as IScreen;
                wndModel.Deactivated -= WndModel_Deactivated;

                var keyValue = _wndModels.FirstOrDefault(m => m.Value == wndModel);
                if (keyValue.Key != null)
                    _wndModels.Remove(keyValue.Key);
            }

            return Task.CompletedTask;
        }

        public void CloseWindowByKey(object wndKey)
        {
            GetWindowModel(wndKey)?.TryCloseAsync();
            _wndModels.Remove(wndKey);
        }

        public async Task<bool?> ShowDialog(IScreen dlgModel, IViewAware parent = null)
        {
            var wnd = await _adapter.CreateWindow(dlgModel);
            if (parent != null)
            {
                wnd.ShowInTaskbar = false;
                wnd.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                wnd.Owner = (Window)parent.GetView();
            }
            else
                wnd.WindowStartupLocation = WindowStartupLocation.CenterScreen;

            return wnd.ShowDialog();
        }

        private class CaliburnAdapter : Caliburn.Micro.WindowManager
        {
            private readonly Dictionary<object, Window> _windows = new();

            public CaliburnAdapter()
            {
            }

            public void ActivateWindow(object rootModel)
            {
                var wnd = _windows.GetOrDefault(rootModel);
                wnd.Activate();
            }

            public Window GetWindow(object rootModel)
            {
                return _windows.GetOrDefault(rootModel);
            }

            public async Task<Window> GetOrCreateMdiWindow(object rootModel, Action<Window> setupAction = null)
            {
                var wnd = _windows.GetOrDefault(rootModel);
                if (wnd == null)
                {
                    wnd = await base.CreateWindowAsync(rootModel, false, null, null);
                    setupAction?.Invoke(wnd);
                    _windows.Add(rootModel, wnd);

                    wnd.Closed += (s, e) =>
                    {
                        var window = (Window)s;
                        window.Owner?.Activate();
                        window.Owner = null;
                        _windows.Remove(rootModel);
                    };
                }

                return wnd;
            }

            public async Task<Window> CreateWindow(object rootModel)
            {
                var window = await base.CreateWindowAsync(rootModel, false, null, null);
                return window;
            }
        }
    }
}
