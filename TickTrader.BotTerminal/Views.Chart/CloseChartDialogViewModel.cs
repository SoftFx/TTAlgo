using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.BotTerminal
{
    class CloseChartDialogViewModel : Screen
    {
        public bool IsConfirmed { get; private set; }

        public bool HasStartedBots { get; }

        public string Message { get; }

        
        public CloseChartDialogViewModel(ChartViewModel chart)
        {
            DisplayName = $"Close chart '{chart.DisplayName}'";
            Message = $"Are you sure you want to close chart '{chart.DisplayName}'?";
        }


        public void Ok()
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
