using SoftFX.Extended;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using Machinarium.Qnil;

namespace TickTrader.BotTerminal
{
    class OrderListViewModel
    {
        private AccountModel model;

        public OrderListViewModel(AccountModel model)
        {
            this.model = model;

            Orders = new ObservableSrotedList<string, OrderModel>();

            foreach (var pair in model.Orders.Snapshot)
                Orders.Add(pair.Key, pair.Value);

            model.Orders.Updated += OrderUpdated;
        }

        private void OrderUpdated(DictionaryUpdateArgs<string, OrderModel> args)
        {
            switch(args.Action)
            {
                case DLinqAction.Insert:
                    AddOrder(args.NewItem);
                    break;
                case DLinqAction.Remove:
                    RemoveOrder(args.Key);
                    break;
            }
        }

        void OrderPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OrderModel order = (OrderModel)sender;

            if (!Filter(order))
                RemoveOrder(order.Id);
            else
            {
                if (Orders.GetOrDefault(order.Id) == null)
                    AddOrder(order);
            }
        }

        private void AddOrder(OrderModel order)
        {
            if (Filter(order))
            {
                Orders.Add(order.Id, order);
                order.PropertyChanged += OrderPropertyChanged;
            }
        }

        void RemoveOrder(string id)
        {
            OrderModel removedOrder;
            if (Orders.Remove(id, out removedOrder))
                removedOrder.PropertyChanged -= OrderPropertyChanged;
        }

        public ObservableSrotedList<string, OrderModel> Orders { get; private set; }

        private bool Filter(OrderModel record)
        {
            return record.OrderType != TradeRecordType.Position;
        }

        public void Close()
        {
            model.Orders.Updated -= OrderUpdated;

            foreach (OrderModel order in Orders)
                order.PropertyChanged -= OrderPropertyChanged;

            Orders.Clear();
        }
    }
}
