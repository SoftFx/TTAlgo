using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.BotTerminal
{
    internal class OpenOrderDialogViewModel: Conductor<IOpenOrderDialogPage>.Collection.OneActive
    {
        public MarketOrderPageViewModel MarketOrderPage;
        public PendingOrderPageViewModel PendingOrderPage;

        public OpenOrderDialogViewModel()
        {
            MarketOrderPage = new MarketOrderPageViewModel();
            PendingOrderPage = new PendingOrderPageViewModel();

            Items.Add(MarketOrderPage);
            Items.Add(PendingOrderPage);
        }
    }

    internal interface IOpenOrderDialogPage { }
}
