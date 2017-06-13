using SoftFX.Extended;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Common.Model;
using TickTrader.BotTerminal.Lib;
using ISymbolInfo = TickTrader.BusinessObjects.ISymbolInfo;

namespace TickTrader.BotTerminal
{
    internal class SymbolModel : Algo.Common.Model.SymbolModel, TickTrader.Algo.Common.Model.Setup.ISymbolInfo
    {
        public SymbolModel(QuoteDistributor distributor, SymbolInfo info, IDictionary<string, CurrencyInfo> currencies)
            : base(distributor, info, currencies)
        {
            this.Amounts = new OrderAmountModel(info);
            this.PredefinedAmounts = Amounts.GetPredefined();

            BidTracker = new RateDirectionTracker();
            AskTracker = new RateDirectionTracker();

            BidTracker.Precision = info.Precision;
            AskTracker.Precision = info.Precision;
        }

        public OrderAmountModel Amounts { get; private set; }
        public RateDirectionTracker BidTracker { get; private set; }
        public RateDirectionTracker AskTracker { get; private set; }
        public List<decimal> PredefinedAmounts { get; private set; }

        public IFeedSubscription Subscribe(int depth = 1)
        {
            return Distributor.Subscribe(Name, depth);
        }

        public override void Update(SymbolInfo newInfo)
        {
            base.Update(newInfo);

            BidTracker.Precision = newInfo.Precision;
            AskTracker.Precision = newInfo.Precision;
        }

        protected override void OnNewTick(Quote tick)
        {
            if (tick.HasBid)
                BidTracker.Rate = tick.Bid;

            if (tick.HasAsk)
                AskTracker.Rate = tick.Ask;

            base.OnNewTick(tick);
        }
    }
}
