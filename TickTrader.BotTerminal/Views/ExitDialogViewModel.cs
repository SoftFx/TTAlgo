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


        public ExitDialogViewModel(bool hasStartedBots, ShootMode mode)
        {
            HasStartedBots = hasStartedBots;

            switch (mode)
            {
                case ShootMode.Exit:
                    DisplayName = "Exit - " + EnvService.Instance.ApplicationName;
                    DialogTitle = "Are you sure you want to exit the application?";
                    break;
                case ShootMode.Logout:
                    DisplayName = "Log out - " + EnvService.Instance.ApplicationName;
                    DialogTitle = "Are you sure you want to log out?";
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

    enum ShootMode { Logout, Exit };
}