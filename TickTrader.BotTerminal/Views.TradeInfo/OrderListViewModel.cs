using SoftFX.Extended;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace TickTrader.BotTerminal
{
    class OrderListViewModel
    {
        private AccountModel model;

        public OrderListViewModel(AccountModel model)
        {
            this.model = model;

            Orders = new ObservableSrotedList<string, OrderModel>();

            foreach (var orderModel in model.Orders.Values)
                Orders.Add(orderModel.Id, orderModel);

            model.Orders.Added += Orders_Added;
            model.Orders.Removed += Orders_Removed;
            model.Orders.Cleared += Orders_Cleared;
        }

        void Orders_Added(KeyValuePair<string, OrderModel> pair)
        {
            if (Filter(pair.Value))
            {
                Orders.Add(pair.Key, pair.Value);
                pair.Value.PropertyChanged += Order_PropertyChanged;
            }
        }

        void Order_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OrderModel order = (OrderModel)sender;

            if (!Filter(order))
                Orders_Removed(order.Id);
            else
            {
                if (Orders.GetOrDefault(order.Id) == null)
                    Orders_Added(new KeyValuePair<string, OrderModel>(order.Id, order));
            }
        }

        void Orders_Removed(string id)
        {
            OrderModel removedOrder;
            if (Orders.Remove(id, out removedOrder))
                removedOrder.PropertyChanged -= Order_PropertyChanged;
        }

        void Orders_Cleared()
        {
            foreach(OrderModel order in Orders)
                order.PropertyChanged -= Order_PropertyChanged;

            Orders.Clear();
        }

        public ObservableSrotedList<string, OrderModel> Orders { get; private set; }

        private bool Filter(OrderModel record)
        {
            return record.OrderType != TradeRecordType.Position;
        }

        public void Close()
        {
            model.Orders.Added -= Orders_Added;
            model.Orders.Removed -= Orders_Removed;
            model.Orders.Cleared -= Orders_Cleared;
        }
    }
}
