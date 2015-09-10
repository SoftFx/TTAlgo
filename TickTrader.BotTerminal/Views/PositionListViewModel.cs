using SoftFX.Extended;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.BotTerminal
{
    class PositionListViewModel
    {
        private AccountModel model;

        public PositionListViewModel(AccountModel model)
        {
            this.model = model;

            Positions = new ObservableSrotedList<string, OrderModel>();

            foreach (var orderModel in model.Orders.Values)
                Positions.Add(orderModel.Id, orderModel);

            model.Orders.Added += Orders_Added;
            model.Orders.Removed += Orders_Removed;
            model.Orders.Cleared += Orders_Cleared;
        }

        public ObservableSrotedList<string, OrderModel> Positions { get; private set; }

        void Orders_Added(KeyValuePair<string, OrderModel> pair)
        {
            if (Filter(pair.Value))
            {
                Positions.Add(pair.Key, pair.Value);
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
                if (Positions.GetOrDefault(order.Id) == null)
                    Orders_Added(new KeyValuePair<string, OrderModel>(order.Id, order));
            }
        }

        void Orders_Removed(string id)
        {
            OrderModel removedOrder;
            if (Positions.Remove(id, out removedOrder))
                removedOrder.PropertyChanged -= Order_PropertyChanged;
        }

        void Orders_Cleared()
        {
            foreach(OrderModel order in Positions)
                order.PropertyChanged -= Order_PropertyChanged;

            Positions.Clear();
        }

        private bool Filter(OrderModel record)
        {
            return record.OrderType == TradeRecordType.Position;
        }

        public void Close()
        {
            model.Orders.Added -= Orders_Added;
            model.Orders.Removed -= Orders_Removed;
            model.Orders.Cleared -= Orders_Cleared;
        }
    }


}
