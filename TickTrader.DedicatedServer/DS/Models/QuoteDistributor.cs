using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SoftFX.Extended.Events;
using TickTrader.Algo.Common.Model;

namespace TickTrader.DedicatedServer.DS.Models
{
    public class QuoteDistributor : QuoteDistributorBase
    {
        private object _sync;

        public QuoteDistributor(ConnectionModel connection, object syncObj) : base(connection)
        {
            _sync = syncObj;
        }

        protected override void EnqueueUpdate(TickEventArgs e)
        {
            lock(_sync) UpdateRate(e.Tick);
        }
    }
}
