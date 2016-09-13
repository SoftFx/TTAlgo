using SoftFX.Extended;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using Machinarium.Qnil;

namespace TickTrader.BotTerminal
{
    class OrderListViewModel : AccountBasedViewModel
    {
        private SymbolCollectionModel _symbols;

        public OrderListViewModel(AccountModel model)
            : base(model)
        {
            Orders = model.Orders
                .Where((id, order) => order.OrderType != TradeRecordType.Position)
                .OrderBy((id, order) => id)
                .AsObservable();
        }

        public IObservableListSource<OrderModel> Orders { get; private set; }
    }
}
