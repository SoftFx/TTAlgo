using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machinarium.Qnil;
using TickTrader.Algo.Common.Model;
using TickTrader.Algo.Common.Lib;
using TickTrader.Algo.Api;
using TickTrader.Algo.Core;

namespace TickTrader.BotTerminal
{
    class AssetsViewModel : AccountBasedViewModel
    {
        public AssetsViewModel(AccountModel model, IReadOnlyDictionary<string, CurrencyEntity> currencies, ConnectionModel connection)
            : base(model, connection)
        {
            Assets = model.Assets
                .OrderBy((c, a) => c)
                .Select(a => new AssetViewModel(a, currencies.Read(a.Currency)))
                .AsObservable();
        }

        protected override bool SupportsAccount(AccountTypes accType)
        {
            return accType == AccountTypes.Cash;
        }

        public IObservableListSource<AssetViewModel> Assets { get; private set; }
    }
}
