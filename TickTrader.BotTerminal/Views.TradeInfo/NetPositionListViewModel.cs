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
    class NetPositionListViewModel : PropertyChangedBase
    {
        private AccountModel model;

        public NetPositionListViewModel(AccountModel model)
        {
            this.model = model;

            Positions = new ObservableSrotedList<string, PositionModel>();

            foreach (var pair in model.Positions.Snapshot)
                Positions.Add(pair.Key, pair.Value);

            model.Positions.Updated += PositionsUpdated;
            model.State.StateChanged += StateChanged;
            model.AccountTypeChanged += AccountTypeChanged;
        }

        private void PositionsUpdated(DictionaryUpdateArgs<string, PositionModel> args)
        {
            switch(args.Action)
            {
                case DLinqAction.Insert: Positions.Add(args.Key, args.NewItem); break;
                case DLinqAction.Remove: Positions.Remove(args.Key); break;
            }
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

        public void Close()
        {
            model.Positions.Updated -= PositionsUpdated;
            model.State.StateChanged -= StateChanged;
            model.AccountTypeChanged -= AccountTypeChanged;

            Positions.Clear();
        }
    }


}
