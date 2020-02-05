using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace TickTrader.BotTerminal
{
    class ExitDialogViewModel : Screen
    {
        public bool IsConfirmed { get; private set; }

        public bool HasStartedBots { get; }

        public string DialogTitle { get; set; }

        public bool OnlyOkButton { get; }

        public bool OnAndCancelButtons => !OnlyOkButton;

        public bool HasError { get; }

        public ExitDialogViewModel(bool hasStartedBots, DialogMode mode)
        {
            HasStartedBots = hasStartedBots;
            OnlyOkButton = false;

            switch (mode)
            {
                case DialogMode.Exit:
                    DisplayName = "Exit - " + EnvService.Instance.ApplicationName;
                    DialogTitle = "Are you sure you want to exit the application?";
                    break;
                case DialogMode.Logout:
                    DisplayName = "Log out - " + EnvService.Instance.ApplicationName;
                    DialogTitle = "Are you sure you want to log out?";
                    break;
                case DialogMode.FailRemovePackage:
                    DisplayName = "Failed";
                    DialogTitle = "Cannot remove package: one or more trade bots from this package is being executed! Please stop all bots and try again!";
                    HasError = true;
                    OnlyOkButton = true;
                    break;
            }
        }


        public void OK()
        {
            IsConfirmed = true;
            TryClose();
        }
        public void Cancel()
        {
            IsConfirmed = false;
            TryClose();
        }
    }

    enum DialogMode { Logout, Exit, FailRemovePackage};
}