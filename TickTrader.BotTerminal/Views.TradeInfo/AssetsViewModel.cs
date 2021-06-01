using System.Collections.Generic;
using System.Linq;
using Machinarium.Qnil;
using TickTrader.Algo.Account;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;

namespace TickTrader.BotTerminal
{
    sealed class AssetsViewModel : AccountBasedViewModel
    {
        public AssetsViewModel(AccountModel model, IReadOnlyDictionary<string, CurrencyInfo> currencies, IConnectionStatusInfo connection)
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
