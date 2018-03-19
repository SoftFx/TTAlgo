using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Common.Model;
using TickTrader.Algo.Core;
using TickTrader.BotTerminal.Lib;
using ISymbolInfo = TickTrader.BusinessObjects.ISymbolInfo;

namespace TickTrader.BotTerminal
{
    //internal class SymbolModel : Algo.Common.Model.SymbolModel, TickTrader.Algo.Common.Model.Setup.ISymbolInfo
    //{
    //    public SymbolModel(QuoteDistributor distributor, SymbolEntity info, IReadOnlyDictionary<string, CurrencyEntity> currencies)
    //        : base(distributor, info, currencies)
    //    {
    //        this.Amounts = new OrderAmountModel(info);
    //        this.PredefinedAmounts = Amounts.GetPredefined();

    //        BidTracker = new RateDirectionTracker();
    //        AskTracker = new RateDirectionTracker();

    //        BidTracker.Precision = info.Precision;
    //        AskTracker.Precision = info.Precision;
    //    }

    //    public OrderAmountModel Amounts { get; private set; }
    //    public RateDirectionTracker BidTracker { get; private set; }
    //    public RateDirectionTracker AskTracker { get; private set; }
    //    public List<decimal> PredefinedAmounts { get; private set; }

    //    public IFeedSubscription Subscribe(int depth = 1)
    //    {
    //        return Distributor.Subscribe(Name, depth);
    //    }

    //    public override void Update(SymbolEntity newInfo)
    //    {
    //        base.Update(newInfo);

    //        BidTracker.Precision = newInfo.Precision;
    //        AskTracker.Precision = newInfo.Precision;
    //    }

    //    protected override void OnNewTick(QuoteEntity tick)
    //    {
    //        if (tick.HasBid)
    //            BidTracker.Rate = tick.Bid;

    //        if (tick.HasAsk)
    //            AskTracker.Rate = tick.Ask;

    //        base.OnNewTick(tick);
    //    }
    //}
}
