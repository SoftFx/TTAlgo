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

    internal interface IProfileLoader
    {
        void ReloadProfile(CancellationToken token);
    }

    internal interface IShell : IWindowModel
    {
        void OpenChart(string smb);
        void ShowChart(string smb, ChartPeriods period);
        void Connect(AccountAuthEntry creds);

        iOrderUi OrderCommands { get; }
        UiLock ConnectionLock { get; }
        WindowManager ToolWndManager { get; }
        IProfileLoader ProfileLoader { get; }
        LocalAlgoAgent Agent { get; }
        DockManagerService DockManagerService { get; }
        ConnectionManager ConnectionManager { get; }
        AlertViewModel AlertsManager { get; }
        EventJournal EventJournal { get; }
    }
}
