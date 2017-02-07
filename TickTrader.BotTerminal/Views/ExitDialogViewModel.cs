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
        public ExitDialogViewModel()
        {
            DisplayName = "Exit - " + EnvService.Instance.ApplicationName;
        }
        public bool IsConfirmed { get; private set; }
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