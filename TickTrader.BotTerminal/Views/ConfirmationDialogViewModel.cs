using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace TickTrader.BotTerminal
{
    enum DialogMode { OK, YesNo, OKCancel }

    enum DialogResult { None, OK, No, Cancel }

    static class DialogMessages
    {
        public const string FailedTitle = "Failed";

        public static string ExitTitle = $"Exit - {EnvService.Instance.ApplicationName}";
        public static string LogoutTitle = $"Log out - {EnvService.Instance.ApplicationName}";

        public const string ExitMessage = "Are you sure you want to exit the application?";
        public const string LogoutMessage = "Are you sure you want to log out?";

        public const string RemoveBotSourceWarning = "All related bots will be deleted!";
        public const string BotsWorkError = "All running bots will be stopped!";
        public const string RemovePackageError = "Cannot remove package: one or more trade bots from this package is being executed! Please stop all bots and try again!";

        public static string GetRemoveTitle(string item) => $"Delete {item}";
        public static string GetRemoveMessage(string item) => $"Do you want to delete the {item}?";
    }

    class ConfirmationDialogViewModel : Screen
    {
        public DialogResult DialogResult = DialogResult.None;

        public DialogMode Mode { get; }

        public string DialogMessage { get; }

        public string DialogError { get; }


        public ConfirmationDialogViewModel(DialogMode mode, string displayName = null, string message = null, string error = null)
        {
            Mode = mode;
            DisplayName = displayName;
            DialogMessage = message;
            DialogError = error;
        }

        public void OK()
        {
            DialogResult = DialogResult.OK;
            TryClose();
        }

        public void Cancel()
        {
            DialogResult = DialogResult.Cancel;
            TryClose();
        }
    }
}