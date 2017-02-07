using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace TickTrader.BotTerminal
{
    public class MdiWindowManager : WindowManager
    {
        private IViewAware containerModel;

        public MdiWindowManager(IViewAware containerModel)
        {
            this.containerModel = containerModel;
        }

        protected override Window CreateWindow(object rootModel, bool isDialog, object context, IDictionary<string, object> settings)
        {
            var wnd = base.CreateWindow(rootModel, isDialog, context, settings);
            var parentWnd = containerModel.GetView() as Window;
            if (parentWnd != null)
            {
                wnd.Owner = parentWnd;
                wnd.Closing += (s, e) =>
                {
                    if (!System.Windows.Interop.ComponentDispatcher.IsThreadModal)
                    {
                        var window = s as Window;
                        window.Owner?.Activate();
                        window.Owner = null;
                    }
                };
            }

            wnd.ShowInTaskbar = false;
            wnd.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            return wnd;
        }
    }
}
