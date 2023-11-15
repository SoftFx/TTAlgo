﻿using Caliburn.Micro;
using System.Threading;
using TickTrader.Algo.Domain;

namespace TickTrader.BotTerminal
{
    internal interface IProfileLoader
    {
        void ReloadProfile(CancellationToken token);
    }

    internal interface IShell : IViewAware, IWindowModel
    {
        UiLock ConnectionLock { get; }
        WindowManager ToolWndManager { get; }
        IProfileLoader ProfileLoader { get; }
        LocalAlgoAgent2 Agent { get; }
        DockManagerService DockManagerService { get; }
        ConnectionManager ConnectionManager { get; }
        AlertViewModel AlertsManager { get; }
        EventJournal EventJournal { get; }

        void OpenChart(string smb);
        void ShowChart(string smb, Feed.Types.Timeframe period);
        DialogResult ShowDialog(DialogButton buttons, DialogMode mode, string title = null, string message = null, string error = null);
        void Connect(AccountAuthEntry creds);
        void OpenUpdate(BotAgentViewModel agent);
    }
}
