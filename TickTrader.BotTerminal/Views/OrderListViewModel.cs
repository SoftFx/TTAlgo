using SoftFX.Extended;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.BotTerminal
{
    class OrderListViewModel
    {
        public OrderListViewModel(AccountModel model)
        {
            Orders = new ObservableCollection<TradeRecord>();

            foreach (var orderModel in model.Orders.Values)
                Orders.Add(orderModel);

            model.Orders.Added += p => Orders.Add(p.Value);
            model.Orders.Cleared += () => Orders.Clear();
        }

        public ObservableCollection<TradeRecord> Orders { get; private set; }
    }
}
