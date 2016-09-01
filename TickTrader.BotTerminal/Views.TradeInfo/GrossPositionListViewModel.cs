using Caliburn.Micro;
using SoftFX.Extended;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machinarium.Qnil;

namespace TickTrader.BotTerminal
{
    class GrossPositionListViewModel : PropertyChangedBase
    {
        private AccountModel model;

        public GrossPositionListViewModel(AccountModel model)
        {
            this.model = model;

            Positions = new ObservableSrotedList<string, OrderModel>();

            foreach (var pair in model.Orders.Snapshot)
                Positions.Add(pair.Key, pair.Value);

            model.Orders.Updated += OrderUpdated;
            model.State.StateChanged += StateChanged;
            model.AccountTypeChanged += AccountTypeChanged;
        }

        private void OrderUpdated(DictionaryUpdateArgs<string, OrderModel> args)
        {
            switch(args.Action)
            {
                case DLinqAction.Insert: AddOrder(args.NewItem); break;
                case DLinqAction.Remove: RemoveOrder(args.Key); break;
            }
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

        private void AddOrder(OrderModel order)
        {
            if (Filter(order))
            {
                Positions.Add(order.Id, order);
                order.PropertyChanged += OrderPropertyChanged;
            }
        }

        private void OrderPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OrderModel order = (OrderModel)sender;

            if (!Filter(order))
                RemoveOrder(order.Id);
            else
            {
                if (Positions.GetOrDefault(order.Id) == null)
                    AddOrder(order);
            }
        }

        private void RemoveOrder(string id)
        {
            OrderModel removedOrder;
            if (Positions.Remove(id, out removedOrder))
                removedOrder.PropertyChanged -= OrderPropertyChanged;
        }

        private bool Filter(OrderModel record)
        {
            return record.OrderType == TradeRecordType.Position;
        }

        public void Close()
        {
            model.Orders.Updated -= OrderUpdated;
            model.State.StateChanged -= StateChanged;
            model.AccountTypeChanged -= AccountTypeChanged;

            foreach (OrderModel order in Positions)
                order.PropertyChanged -= OrderPropertyChanged;

            Positions.Clear();
        }
    }
}
