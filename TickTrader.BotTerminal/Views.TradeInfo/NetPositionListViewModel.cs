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
    class NetPositionListViewModel : AccountBasedViewModel
    {
        private AccountModel model;

        public NetPositionListViewModel(AccountModel model)
            : base(model)
        {
            this.model = model;

            Positions = model.Positions.OrderBy((id, p) => id).AsObservable();
        }

        protected override bool SupportsAccount(AccountType accType)
        {
            return accType == AccountType.Net;
        }

        public IObservableListSource<PositionModel> Positions { get; private set; }
    }
}
