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
    internal class TradeHistoryGridViewModel
    {
        public TradeHistoryGridViewModel(ICollection<TransactionReport> src)
        {
            Items = new Property<ICollectionView>();
            Items.Value = CollectionViewSource.GetDefaultView(src);
            AccType = new Property<AccountTypes?>();

            IsGrossAccount = AccType.Var == AccountTypes.Gross;
            IsNetAccount = AccType.Var == AccountTypes.Net;
            IsCachAccount = AccType.Var == AccountTypes.Cash;
            IsMarginAccount = IsGrossAccount | IsNetAccount;
            IsAccTypeSet = AccType.Var.IsNotNull();
        }

        public Property<ICollectionView> Items { get; }
        public Property<AccountTypes?> AccType { get; }
        public Predicate<object> Filter { get => Items.Value.Filter; set => Items.Value.Filter = value; } 

        public BoolVar IsNetAccount { get; }
        public BoolVar IsCachAccount { get; }
        public BoolVar IsGrossAccount { get; }
        public BoolVar IsMarginAccount { get; }
        public BoolVar IsAccTypeSet { get; }

        public void Refresh()
        {
            Items.Value.Refresh();
        }

        public void SetCollection(ICollection<TransactionReport> src)
        {
            var filterCopy = Items.Value.Filter;

            Items.Value = CollectionViewSource.GetDefaultView(src);
            Items.Value.Filter = filterCopy;
        }
    }
}
