using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.BotTerminal
{
    internal interface iOrderUi
    {
        void OpenMarkerOrder(string symbol);
        void OpenMarkerOrder(string symbol, decimal volume, OrderSide side);
    }

    internal interface ToolWindowsManager
    {
        IScreen GetWindow(object key);
        void OpenWindow(object wndKey, IScreen wndModel, bool closeExisting = false);
        void CloseWindow(object wndKey);
        bool? ShowDialog(IScreen dlgModel);
    }

    internal interface IProfileLoader
    {
        void ReloadProfile(CancellationToken token);
    }

    internal interface IShell : IWindowModel
    {
        iOrderUi OrderCommands { get; }
        UiLock ConnectionLock { get; }
        ToolWindowsManager ToolWndManager { get; }
        IProfileLoader ProfileLoader { get; }
    }
}
