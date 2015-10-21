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
                wnd.Owner = parentWnd;
            wnd.ShowInTaskbar = false;
            return wnd;
        }
    }
}
