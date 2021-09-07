using Caliburn.Micro;
using FontAwesome.WPF;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace TickTrader.BotTerminal
{
    enum DialogButton { OK, YesNo, OKCancel }

    enum DialogResult { None, OK, No, Cancel }

    enum DialogMode { Info, Question, Warning, Error }

    static class DialogMessages
    {
        public const string FailedTitle = "Failed";

        public static string ExitTitle = $"Exit - {EnvService.Instance.ApplicationName}";
        public static string LogoutTitle = $"Log out - {EnvService.Instance.ApplicationName}";

        public const string ExitMessage = "Are you sure you want to exit the application?";
        public const string LogoutMessage = "Are you sure you want to log out?";

        public const string RemoveBotSourceWarning = "All related bots will be deleted!";
        public const string BotsWorkError = "All running bots will be stopped!";
        public const string RemovePackageError = "Cannot remove Algo package: one or more trade bots from this package is being executed! Please stop all bots and try again!";

        public static string GetRemoveTitle(string item) => $"Delete {item}";
        public static string GetRemoveMessage(string item) => $"Do you want to delete the {item}?";
    }

    class ConfirmationDialogViewModel : Screen
    {
        public DialogResult DialogResult = DialogResult.None;

        public DialogButton Buttons { get; }

        public DialogMode Mode { get; }

        public string DialogMessage { get; }

        public string DialogError { get; }

        public ImageSource Icon { get; }

        public ConfirmationDialogViewModel(DialogButton buttons, DialogMode mode, string displayName = null, string message = null, string error = null)
        {
            Buttons = buttons;
            DisplayName = displayName;
            DialogMessage = message;
            DialogError = error;
            Mode = mode;
            Icon = (ImageSource)Application.Current.TryFindResource($"{Mode}Icon"); //Key icons are located in Resx/Images/Paths
        }

        public void OK()
        {
            DialogResult = DialogResult.OK;
            TryCloseAsync();
        }

        public void Cancel()
        {
            DialogResult = DialogResult.Cancel;
            TryCloseAsync();
        }
    }
}