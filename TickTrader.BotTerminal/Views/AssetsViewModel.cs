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
    class AssetsViewModel : PropertyChangedBase
    {
        private AccountModel model;

        public AssetsViewModel(AccountModel model)
        {
            this.model = model;

            Assets = new ObservableSrotedList<string, AssetModel>();

            foreach (var asset in model.Assets.Values)
                Assets.Add(asset.Currency, asset);

            model.Assets.Added += AssetAdded;
            model.Assets.Removed += AssetRemoved;
            model.Assets.Cleared += AssetsCleared;

            model.State.StateChanged += StateChanged;
            model.AccountTypeChanged += AccountTypeChanged;
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

        private void AssetAdded(KeyValuePair<string, AssetModel> pair)
        {
            Assets.Add(pair.Key, pair.Value);
        }

        private void AssetRemoved(string id)
        {
            Assets.Remove(id);
        }

        private void AssetsCleared()
        {
            Assets.Clear();
        }

        public void Close()
        {
            model.Assets.Added -= AssetAdded;
            model.Assets.Removed -= AssetRemoved;
            model.Assets.Cleared -= AssetsCleared;
            model.State.StateChanged -= StateChanged;
            model.AccountTypeChanged -= AccountTypeChanged;
        }
    }


}
