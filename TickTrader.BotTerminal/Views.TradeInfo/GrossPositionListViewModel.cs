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
    class GrossPositionListViewModel : AccountBasedViewModel
    {
        public GrossPositionListViewModel(AccountModel model) : base(model)
        {
            Positions = model.Orders
                .Where((id, order) => order.OrderType == TradeRecordType.Position)
                .OrderBy((id, order) => id)
                .AsObservable();
        }

        protected override bool SupportsAccount(AccountType accType)
        {
            return accType == AccountType.Gross;
        }

        public IObservableListSource<OrderModel> Positions { get; private set; }
    }
}
