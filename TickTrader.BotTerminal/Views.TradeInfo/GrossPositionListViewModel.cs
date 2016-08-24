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
    class GrossPositionListViewModel : PropertyChangedBase
    {
        private AccountModel model;

        public GrossPositionListViewModel(AccountModel model)
        {
            this.model = model;

            Positions = new ObservableSrotedList<string, OrderModel>();

            foreach (var orderModel in model.Orders.Values)
                Positions.Add(orderModel.Id, orderModel);

            model.Orders.Added += OrderAdded;
            model.Orders.Removed += OrderRemoved;
            model.Orders.Cleared += OrdersCleared;

            model.State.StateChanged += StateChanged;
            model.AccountTypeChanged += AccountTypeChanged;
        }

        public ObservableSrotedList<string, OrderModel> Positions { get; private set; }
        public bool IsBusy { get; private set; }
        public bool IsEnabled { get; private set; }

        private void UpdateState()
        {
            IsBusy = model.State.Current == AccountModel.States.WaitingData;
            NotifyOfPropertyChange(nameof(IsBusy));
        }

        private void AccountTypeChanged()
        {
            IsEnabled = model.Type == AccountType.Gross;
            NotifyOfPropertyChange(nameof(IsEnabled));
        }

        private void StateChanged(AccountModel.States arg1, AccountModel.States arg2)
        {
            UpdateState();
        }

        private void OrderAdded(KeyValuePair<string, OrderModel> pair)
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
                OrderRemoved(order.Id);
            else
            {
                if (Positions.GetOrDefault(order.Id) == null)
                    OrderAdded(new KeyValuePair<string, OrderModel>(order.Id, order));
            }
        }

        private void OrderRemoved(string id)
        {
            OrderModel removedOrder;
            if (Positions.Remove(id, out removedOrder))
                removedOrder.PropertyChanged -= Order_PropertyChanged;
        }

        private void OrdersCleared()
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
            model.Orders.Added -= OrderAdded;
            model.Orders.Removed -= OrderRemoved;
            model.Orders.Cleared -= OrdersCleared;
            model.State.StateChanged -= StateChanged;
            model.AccountTypeChanged -= AccountTypeChanged;
        }
    }
}
