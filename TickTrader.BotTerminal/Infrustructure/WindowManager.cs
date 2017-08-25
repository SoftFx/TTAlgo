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
        private CaliburnAdapter _adapter;
        IViewAware _containerModel;
        private Dictionary<object, IScreen> _wndModels = new Dictionary<object, IScreen>();

        public WindowManager(IViewAware root)
        {
            _containerModel = root;
            _adapter = new CaliburnAdapter();
        }

        public IScreen GetWindowModel(object key)
        {
            return _wndModels.GetValueOrDefault(key);
        }

        public void OpenWindow(IScreen wndModel)
        {
            var wnd = _adapter.GetOrCreateWindow(wndModel);
            wnd.Show();
            wnd.Activate();
        }

        public void OpenMdiWindow(IScreen wndModel)
        {
            _adapter.GetOrCreateWindow(wndModel, MdiSetup).Show();
        }

        private void MdiSetup(Window wnd)
        {
            wnd.ShowInTaskbar = false;
            wnd.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            var parentWnd = _containerModel.GetView() as Window;
            if (parentWnd != null)
                wnd.Owner = parentWnd;
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
            //wndModel.Deactivated += WndModel_Deactivated;
            _wndModels[wndKey] = wndModel;
            OpenMdiWindow(wndModel);
        }

        //private void WndModel_Deactivated(object sender, DeactivationEventArgs e)
        //{
        //    if (e.WasClosed)
        //    {
        //        var wndModel = sender as IScreen;
        //        wndModel.Deactivated -= WndModel_Deactivated;

        //        var keyValue = _wndModels.FirstOrDefault(m => m.Value == wndModel);
        //        if (keyValue.Key != null)
        //            _wndModels.Remove(keyValue.Key);
        //    }
        //}

        public void CloseWindowByKey(object wndKey)
        {
            GetWindowModel(wndKey)?.TryClose();
            _wndModels.Remove(wndKey);
        }

        public bool? ShowDialog(IScreen dlgModel)
        {
            return _adapter.ShowDialog(dlgModel);
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

            public Window GetOrCreateWindow(object rootModel, Action<Window> setupAction = null)
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
        }
    }
}
