using System;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Calculator
{
    internal interface ISideNode
    {
        double Value { get; }

        bool HasValue { get; }

        event Action ValueUpdate;
    }

    internal abstract class BaseSideNode
    {
        protected readonly ISymbolInfo _symbol;


        public event Action ValueUpdate;

        protected BaseSideNode(ISymbolInfo smb)
        {
            _symbol = smb;

            _symbol.RateUpdated += SideUpdateEvent;
        }

        private void SideUpdateEvent(ISymbolInfo symbol)
        {
            ValueUpdate?.Invoke();
        }
    }

    internal sealed class AskSideNode : BaseSideNode, ISideNode
    {
        public double Value => _symbol.Ask;

        public bool HasValue => _symbol.HasAsk;


        internal AskSideNode(ISymbolInfo smb) : base(smb) { }
    }

    internal sealed class BidSideNode : BaseSideNode, ISideNode
    {
        public double Value => _symbol.Bid;

        public bool HasValue => _symbol.HasBid;


        internal BidSideNode(ISymbolInfo smb) : base(smb) { }
    }
}
