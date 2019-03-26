using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machinarium.Qnil;
using TickTrader.Algo.Common.Model;
using TickTrader.Algo.Api;

namespace TickTrader.BotTerminal
{
    class NetPositionListViewModel : AccountBasedViewModel
    {
        public NetPositionListViewModel(AccountModel model, IVarSet<string, SymbolModel> symbols, IConnectionStatusInfo connection)
            : base(model, connection)
        {
            Positions = model.Positions
                .OrderBy((id, p) => id)
                .Select(p => new PositionViewModel(p, symbols.GetOrDefault(p.Symbol)))
                .AsObservable();

            Positions.CollectionChanged += PositionsCollectionChanged;

            var pos = Positions.FirstOrDefault(); // for test 
        }

        protected override bool SupportsAccount(AccountTypes accType)
        {
            return accType == AccountTypes.Net;
        }

        public IObservableList<PositionViewModel> Positions { get; private set; }

        private void PositionsCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Replace
              || e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                foreach (var item in e.OldItems)
                    ((PositionViewModel)item).Dispose();
            }
        }
    }
}
