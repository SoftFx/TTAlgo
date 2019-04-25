using Caliburn.Micro;
using Machinarium.Var;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using TickTrader.Algo.Api;

namespace TickTrader.BotTerminal
{
    internal class TradeHistoryGridViewModel : PropertyChangedBase
    {
        private ProfileManager _profileManager;

        public TradeHistoryGridViewModel(ICollection<TransactionReport> src, ProfileManager profile = null)
        {
            Items = new Property<ICollectionView>();
            Items.Value = CollectionViewSource.GetDefaultView(src);
            AccType = new Property<AccountTypes?>();

            IsGrossAccount = AccType.Var == AccountTypes.Gross;
            IsNetAccount = AccType.Var == AccountTypes.Net;
            IsCachAccount = AccType.Var == AccountTypes.Cash;
            IsMarginAccount = IsGrossAccount | IsNetAccount;
            IsAccTypeSet = AccType.Var.IsNotNull();

            AutoSizeColumns = true;
            ConvertTimeToLocal = true;

            _profileManager = profile;
            if (_profileManager != null)
            {
                _profileManager.ProfileUpdated += UpdateProvider;
                UpdateProvider();
            }
        }

        public Property<ICollectionView> Items { get; }
        public Property<AccountTypes?> AccType { get; }
        public Predicate<object> Filter { get => Items.Value.Filter; set => Items.Value.Filter = value; } 

        public BoolVar IsNetAccount { get; }
        public BoolVar IsCachAccount { get; }
        public BoolVar IsGrossAccount { get; }
        public BoolVar IsMarginAccount { get; }
        public BoolVar IsAccTypeSet { get; }

        public bool AutoSizeColumns { get; set; }
        public bool ConvertTimeToLocal { get; set; }

        public AccountTypes GetAccTypeValue() => AccType.Value.Value;
        public ProviderColumnsState StateProvider { get; private set; }

        public override void Refresh()
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
            if (_profileManager.CurrentProfile.ColumnsShow != null)
            {
                var prefix = nameof(TradeHistoryGridViewModel);

                if (_profileManager.OpenBacktester)
                    prefix += "_backtester";

                StateProvider = new ProviderColumnsState(_profileManager.CurrentProfile.ColumnsShow, prefix);
                NotifyOfPropertyChange(nameof(StateProvider));
            }
        }
    }
}
