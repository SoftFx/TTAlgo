using SoftFX.Extended;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TickTrader.Algo.Common.Model;
using TickTrader.DedicatedServer.Infrastructure;

namespace TickTrader.DedicatedServer.DS.Models
{
    public class SymbolManager : SymbolCollectionBase
    {
        private object _sycn;
        private QuoteDistributor _distributor;

        public SymbolManager(ConnectionModel connection, object sync) : base(connection, new SyncAdapter(sync))
        {
            _sycn = sync;
            _distributor = new QuoteDistributor(connection, _sycn);
        }

        public override QuoteDistributorBase Distributor => _distributor;
    }
}
