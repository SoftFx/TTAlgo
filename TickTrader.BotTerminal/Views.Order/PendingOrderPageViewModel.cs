using Caliburn.Micro;

namespace TickTrader.BotTerminal
{
    internal class PendingOrderPageViewModel : Screen, IOpenOrderDialogPage
    {
        public PendingOrderPageViewModel(OpenOrderDialogViewModel openOrderViewModel)
        {
            this.Order = openOrderViewModel;

            DisplayName = "Pending order";
        }

        public OpenOrderDialogViewModel Order { get; private set; }
    }
}