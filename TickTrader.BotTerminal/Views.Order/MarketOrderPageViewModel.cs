using Caliburn.Micro;

namespace TickTrader.BotTerminal
{
    internal class MarketOrderPageViewModel : Screen, IOpenOrderDialogPage 
    {
        public MarketOrderPageViewModel()
        {
            DisplayName = "Market order";
        }
    }
}