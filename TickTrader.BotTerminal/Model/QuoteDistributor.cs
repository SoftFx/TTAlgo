using NLog;
using SoftFX.Extended;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using TickTrader.Algo.Common.Model;
using TickTrader.BotTerminal.Lib;
using SoftFX.Extended.Events;

namespace TickTrader.BotTerminal
{
    internal class QuoteDistributor : QuoteDistributorBase
    {
        private ActionBlock<Quote> rateUpdater;

        public QuoteDistributor(ConnectionModel connection)
            : base(connection)
        {
            rateUpdater = DataflowHelper.CreateUiActionBlock<Quote>(UpdateRate, 100, 100, CancellationToken.None);
        }

        protected override void EnqueueUpdate(TickEventArgs e)
        {
            rateUpdater.SendAsync(e.Tick).Wait();
        }
    }
}