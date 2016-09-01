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
    class AssetsViewModel : PropertyChangedBase
    {
        private AccountModel model;

        public AssetsViewModel(AccountModel model)
        {
            this.model = model;

            Assets = new ObservableSrotedList<string, AssetModel>();

            foreach (var pair in model.Assets.Snapshot)
                Assets.Add(pair.Key, pair.Value);

            model.Assets.Updated += AssetUpdated;
            model.State.StateChanged += StateChanged;
            model.AccountTypeChanged += AccountTypeChanged;
        }

        private void AssetUpdated(DictionaryUpdateArgs<string, AssetModel> args)
        {
            switch(args.Action)
            {
                case DLinqAction.Insert: Assets.Add(args.Key, args.NewItem); break;
                case DLinqAction.Remove: Assets.Remove(args.Key); break;
            }
        }

        private void AccountTypeChanged()
        {
            IsEnabled = model.Type == AccountType.Cash;
            NotifyOfPropertyChange(nameof(IsEnabled));
        }

        public ObservableSrotedList<string, AssetModel> Assets { get; private set; }
        public bool IsBusy { get; private set; }
        public bool IsEnabled { get; private set; }

        private void UpdateState()
        {
            IsBusy = model.State.Current == AccountModel.States.WaitingData;
            NotifyOfPropertyChange(nameof(IsBusy));
        }

        private void StateChanged(AccountModel.States arg1, AccountModel.States arg2)
        {
            UpdateState();
        }

        public void Close()
        {
            model.Assets.Updated -= AssetUpdated;
            model.State.StateChanged -= StateChanged;
            model.AccountTypeChanged -= AccountTypeChanged;

            Assets.Clear();
        }
    }


}
