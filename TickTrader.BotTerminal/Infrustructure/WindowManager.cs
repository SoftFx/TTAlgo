using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace TickTrader.BotTerminal
{
    public class WindowManager
    {
        private static Caliburn.Micro.WindowManager staticManager = new Caliburn.Micro.WindowManager();

        private CaliburnAdapter _adapter;
        IViewAware _containerModel;
        private Dictionary<object, IScreen> _wndModels = new Dictionary<object, IScreen>();

        public WindowManager(IViewAware root)
        {
            _containerModel = root;
            _adapter = new CaliburnAdapter();
        }

        public void OpenWindow(IScreen wndModel)
        {
            var wnd = _adapter.GetOrCreateMdiWindow(wndModel);
            wnd.Show();
            wnd.Activate();
        }

        public void OpenMdiWindow(IScreen wndModel)
        {
            _adapter.GetOrCreateMdiWindow(wndModel, MdiSetup).Show();
        }

        public static void OpenWindow(IScreen wndModel, IViewAware parent)
        {
            staticManager.ShowWindow(wndModel);
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
                existing.Activate();
            else
                OpenMdiWindow(wndKey, wndModelfactory());
        }

        public void OpenMdiWindow(object wndKey, IScreen wndModel)
        {
            IScreen existing = GetWindowModel(wndKey);
            if (existing != null)
            {
                if (existing != wndModel)
                    existing.TryClose();
                else
                    existing.Activate();
            }
            wndModel.Deactivated += WndModel_Deactivated;
            _wndModels[wndKey] = wndModel;
            OpenMdiWindow(wndModel);
        }

        private IScreen GetWindowModel(object key)
        {
            return _wndModels.GetValueOrDefault(key);
        }

        private void WndModel_Deactivated(object sender, DeactivationEventArgs e)
        {
            if (e.WasClosed)
            {
                var wndModel = sender as IScreen;
                wndModel.Deactivated -= WndModel_Deactivated;

                var keyValue = _wndModels.FirstOrDefault(m => m.Value == wndModel);
                if (keyValue.Key != null)
                    _wndModels.Remove(keyValue.Key);
            }
        }

        public void CloseWindowByKey(object wndKey)
        {
            GetWindowModel(wndKey)?.TryClose();
            _wndModels.Remove(wndKey);
        }

        public bool? ShowDialog(IScreen dlgModel, IViewAware parent = null)
        {
            var wnd = _adapter.CreateWindow(dlgModel);
            if (parent != null)
            {
                wnd.ShowInTaskbar = false;
                wnd.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                wnd.Owner = (Window)parent.GetView();
            }
            return wnd.ShowDialog();
        }

        private class CaliburnAdapter : Caliburn.Micro.WindowManager
        {
            //private IViewAware _containerModel;
            private Dictionary<object, Window> _windows = new Dictionary<object, Window>();

            public CaliburnAdapter()
            {
            }

            public Window GetWindow(object rootModel)
            {
                return _windows.GetOrDefault(rootModel);
            }

            public Window GetOrCreateMdiWindow(object rootModel, Action<Window> setupAction = null)
            {
                var wnd = _windows.GetOrDefault(rootModel);
                if (wnd == null)
                {
                    wnd = base.CreateWindow(rootModel, false, null, null);
                    setupAction?.Invoke(wnd);
                    _windows.Add(rootModel, wnd);

                    wnd.Closing += (s, e) =>
                    {
                        var window = (Window)s;
                        window.Owner?.Activate();
                        window.Owner = null;
                        _windows.Remove(rootModel);
                    };
                }
                return wnd;
            }

            public Window CreateWindow(object rootModel)
            {
                return base.CreateWindow(rootModel, false, null, null);
            }

            protected override Window CreateWindow(object rootModel, bool isDialog, object context, IDictionary<string, object> settings)
            {
                return base.CreateWindow(rootModel, isDialog, context, settings);
            }
        }
    }
}
