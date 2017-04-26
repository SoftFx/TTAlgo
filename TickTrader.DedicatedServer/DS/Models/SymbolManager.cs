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

        public SymbolManager(ClientCore client, object sync) : base(client, new SyncAdapter(sync))
        {
            _sycn = sync;
        }
    }
}
