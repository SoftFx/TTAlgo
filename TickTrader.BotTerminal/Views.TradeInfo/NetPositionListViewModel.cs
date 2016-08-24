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
    class NetPositionListViewModel : PropertyChangedBase
    {
        private AccountModel model;

        public NetPositionListViewModel(AccountModel model)
        {
            this.model = model;

            Positions = new ObservableSrotedList<string, PositionModel>();

            foreach (var position in model.Positions.Values)
                Positions.Add(position.Symbol, position);

            model.Positions.Added += PositionAdded;
            model.Positions.Removed += PositionRemoved;
            model.Positions.Cleared += PositionsCleared;

            model.State.StateChanged += StateChanged;
            model.AccountTypeChanged += AccountTypeChanged;
        }

       
        public ObservableSrotedList<string, PositionModel> Positions { get; private set; }
        public bool IsBusy { get; private set; }
        public bool IsEnabled { get; private set; }

        private void UpdateState()
        {
            IsBusy = model.State.Current == AccountModel.States.WaitingData;
            NotifyOfPropertyChange(nameof(IsBusy));
        }

        private void AccountTypeChanged()
        {
            IsEnabled = model.Type == AccountType.Net;
            NotifyOfPropertyChange(nameof(IsEnabled));
        }

        private void StateChanged(AccountModel.States arg1, AccountModel.States arg2)
        {
            UpdateState();
        }

        private void PositionAdded(KeyValuePair<string, PositionModel> pair)
        {
            Positions.Add(pair.Key, pair.Value);
        }

        private void PositionRemoved(string id)
        {
            Positions.Remove(id);
        }

        private void PositionsCleared()
        {
            Positions.Clear();
        }

        public void Close()
        {
            model.Positions.Added -= PositionAdded;
            model.Positions.Removed -= PositionRemoved;
            model.Positions.Cleared -= PositionsCleared;
            model.State.StateChanged -= StateChanged;
            model.AccountTypeChanged -= AccountTypeChanged;
        }
    }


}
