using Caliburn.Micro;
using System.Threading;

namespace TickTrader.BotTerminal
{
    internal interface IProfileLoader
    {
        void ReloadProfile(CancellationToken token);
    }

    internal interface IShell : IViewAware, IWindowModel
    {
        void OpenChart(string smb);
        void ShowChart(string smb, ChartPeriods period);
        DialogResult ShowDialog(DialogButton buttons, DialogMode mode, string title = null, string message = null, string error = null);
        void Connect(AccountAuthEntry creds);
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
