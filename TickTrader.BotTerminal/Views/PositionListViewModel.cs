using Caliburn.Micro;
using SoftFX.Extended;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.BotTerminal
{
    class PositionListViewModel : PropertyChangedBase
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

            model.State.StateChanged += State_StateChanged;
        }

        public ObservableSrotedList<string, OrderModel> Positions { get; private set; }
        public bool IsBusy { get; private set; }

        private void UpdateState()
        {
            IsBusy = model.State.Current == AccountModel.States.WaitingData;
            NotifyOfPropertyChange("IsBusy");
        }

        private void State_StateChanged(AccountModel.States arg1, AccountModel.States arg2)
        {
            UpdateState();
        }

        private void Orders_Added(KeyValuePair<string, OrderModel> pair)
        {
            if (Filter(pair.Value))
            {
                Positions.Add(pair.Key, pair.Value);
                pair.Value.PropertyChanged += Order_PropertyChanged;
            }
        }

        private void Order_PropertyChanged(object sender, PropertyChangedEventArgs e)
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

        private void Orders_Removed(string id)
        {
            OrderModel removedOrder;
            if (Positions.Remove(id, out removedOrder))
                removedOrder.PropertyChanged -= Order_PropertyChanged;
        }

        private void Orders_Cleared()
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
            model.State.StateChanged -= State_StateChanged;
        }
    }


}
