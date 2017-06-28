using Caliburn.Micro;
using SoftFX.Extended;
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


        public ExitDialogViewModel(bool hasStartedBots)
        {
            HasStartedBots = hasStartedBots;

            DisplayName = "Exit - " + EnvService.Instance.ApplicationName;
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
}