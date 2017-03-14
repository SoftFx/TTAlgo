﻿using SoftFX.Extended;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using Machinarium.Qnil;
using Caliburn.Micro;

namespace TickTrader.BotTerminal
{
    class OrderListViewModel : AccountBasedViewModel
    {
        private SymbolCollectionModel _symbols;

        public OrderListViewModel(AccountModel model, SymbolCollectionModel symbols)
            : base(model)
        {
            _symbols = symbols;

            Orders = model.Orders
                .Where((id, order) => order.OrderType != TradeRecordType.Position)
                .OrderBy((id, order) => id)
                .Select(o => new OrderViewModel(o, symbols.GetOrDefault(o.Symbol)))
                .AsObservable();

            Orders.CollectionChanged += OrdersCollectionChanged;
            Account.AccountTypeChanged += () => NotifyOfPropertyChange(nameof(IsGrossAccount));
        }

        public IObservableListSource<OrderViewModel> Orders { get; private set; }
        public bool IsGrossAccount => Account.Type == AccountType.Gross;

        private void OrdersCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Replace
                || e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                foreach (var item in e.OldItems)
                    ((OrderViewModel)item).Dispose();
            }
        }
    }
}