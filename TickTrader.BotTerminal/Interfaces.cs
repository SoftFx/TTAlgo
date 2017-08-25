using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.BotTerminal
{
    internal interface iOrderUi
    {
        void OpenMarkerOrder(string symbol);
        void OpenMarkerOrder(string symbol, decimal volume, OrderSide side);
    }

    internal interface IShell : IWindowModel
    {
        iOrderUi OrderCommands { get; }
        UiLock ConnectionLock { get; }
        WindowManager ToolWndManager { get; }
    }
}
