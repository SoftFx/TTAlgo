using Caliburn.Micro;
using Machinarium.Var;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Data;
using TickTrader.Algo.Domain;

namespace TickTrader.BotTerminal
{
    internal class TradeHistoryGridViewModel : PropertyChangedBase
    {
        private readonly bool _isBacktester;
        private ProfileManager _profileManager;
        
        public TradeHistoryGridViewModel(ICollection<TransactionReport> src, ProfileManager profile = null, bool isBacktester = false)
        {
            Items = new Property<ICollectionView>();
            Items.Value = CollectionViewSource.GetDefaultView(src);
            AccType = new Property<AccountInfo.Types.Type?>();

            IsGrossAccount = AccType.Var == AccountInfo.Types.Type.Gross;
            IsNetAccount = AccType.Var == AccountInfo.Types.Type.Net;
            IsCachAccount = AccType.Var == AccountInfo.Types.Type.Cash;
            IsMarginAccount = IsGrossAccount | IsNetAccount;
            IsAccTypeSet = AccType.Var.IsNotNull();

            AutoSizeColumns = !isBacktester;
            ConvertTimeToLocal = false;

            _profileManager = profile;
            _isBacktester = isBacktester;

            if (_profileManager != null)
            {
                _profileManager.ProfileUpdated += UpdateProvider;
                UpdateProvider();
            }
        }

        public Property<ICollectionView> Items { get; }
        public Property<AccountInfo.Types.Type?> AccType { get; }
        public Predicate<object> Filter { get => Items.Value.Filter; set => Items.Value.Filter = value; } 

        public BoolVar IsNetAccount { get; }
        public BoolVar IsCachAccount { get; }
        public BoolVar IsGrossAccount { get; }
        public BoolVar IsMarginAccount { get; }
        public BoolVar IsAccTypeSet { get; }

        public bool IsSlippageSupported { get; set; } = true;
        public bool AutoSizeColumns { get; private set; }
        public bool ConvertTimeToLocal { get; set; }

        public AccountInfo.Types.Type GetAccTypeValue() => AccType.Value.Value;
        public ViewModelStorageEntry StateProvider { get; private set; }

        public void RefreshItems()
        {
            Items.Value.Refresh();
        }

        public void SetCollection(ICollection<TransactionReport> src)
        {
            var filterCopy = Items.Value.Filter;

            Items.Value = CollectionViewSource.GetDefaultView(src);
            Items.Value.Filter = filterCopy;
        }

        private void UpdateProvider()
        {
            StateProvider = _profileManager.CurrentProfile.GetViewModelStorage(_isBacktester ? ViewModelStorageKeys.HistoryBacktester : ViewModelStorageKeys.History);
            NotifyOfPropertyChange(nameof(StateProvider));
        }
    }
}
