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
using TickTrader.Algo.Domain;

namespace TickTrader.BotTerminal
{
    class AssetsViewModel : AccountBasedViewModel
    {
        public AssetsViewModel(AccountModel model, IReadOnlyDictionary<string, CurrencyEntity> currencies, IConnectionStatusInfo connection)
            : base(model, connection)
        {
            Assets = model.Assets
                .OrderBy((c, a) => c)
                .Select(a => new AssetViewModel(a, currencies.Read(a.Currency)))
                .AsObservable();
        }

        protected override bool SupportsAccount(AccountInfo.Types.Type accType)
        {
            return accType == AccountInfo.Types.Type.Cash;
        }

        public IObservableList<AssetViewModel> Assets { get; private set; }
    }
}
