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

        public OpenOrderDialogViewModel()
        {
            MarketOrderPage = new MarketOrderPageViewModel();

            Items.Add(MarketOrderPage);
        }
    }

    internal interface IOpenOrderDialogPage { }
}
