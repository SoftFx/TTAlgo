using Caliburn.Micro;

namespace TickTrader.BotTerminal
{
    internal class PendingOrderPageViewModel : Screen, IOpenOrderDialogPage
    {
        public PendingOrderPageViewModel()
        {
            DisplayName = "Pending order";
        }
    }
}