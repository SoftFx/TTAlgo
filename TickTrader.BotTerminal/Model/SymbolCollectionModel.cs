using Machinarium.Qnil;
using Machinarium.State;
using NLog;
using SoftFX.Extended;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using TickTrader.Algo.Common.Model;
using TickTrader.BotTerminal.Lib;

namespace TickTrader.BotTerminal
{
    internal class SymbolCollectionModel : Algo.Common.Model.SymbolCollectionBase
    {
        public SymbolCollectionModel(ConnectionModel connection)
            : base(connection, new DispatcherSync()) 
        {
        }

        protected override Algo.Common.Model.SymbolModel CreateSymbolsEntity(QuoteDistributorBase distributor, SymbolInfo info, IDictionary<string, CurrencyInfo> currencies)
        {
            return new SymbolModel((QuoteDistributor)distributor, info, currencies);
        }

        protected override QuoteDistributorBase CreateDistributor(ConnectionModel connection)
        {
            return new QuoteDistributor(connection);
        }
    }
}
