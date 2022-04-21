using System;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Domain.CalculatorInterfaces;

namespace TickTrader.Algo.Calculator.AlgoMarket
{
    public interface ISideNode
    {
        double Value { get; }

        bool HasValue { get; }

        CalculationError Error { get; }

        event Action ValueUpdate;


        void Subscribe(ISymbolInfoWithRate symbol);
    }


    internal abstract class BaseSideNode
    {
        protected ISymbolInfoWithRate _symbol;


        public CalculationError Error
        {
            get
            {
                if (_symbol == null)
                    return CalculationError.SymbolNotFound;

                return HasValue ? CalculationError.None : CalculationError.OffQuote;
            }
        }

        public abstract bool HasValue { get; }

        public event Action ValueUpdate;


        protected BaseSideNode(ISymbolInfoWithRate smb) => Subscribe(smb);


        public void Subscribe(ISymbolInfoWithRate symbol)
        {
            if (_symbol != null)
                _symbol.RateUpdated -= SideUpdateEvent;

            _symbol = symbol;

            if (_symbol != null)
                _symbol.RateUpdated += SideUpdateEvent;
        }

        private void SideUpdateEvent(ISymbolInfo symbol) => ValueUpdate?.Invoke();
    }


    internal sealed class AskSideNode : BaseSideNode, ISideNode
    {
        public double Value => _symbol?.Ask ?? double.NaN;

        public override bool HasValue => _symbol?.HasAsk ?? false;


        internal AskSideNode(ISymbolInfoWithRate smb) : base(smb) { }
    }


    internal sealed class BidSideNode : BaseSideNode, ISideNode
    {
        public double Value => _symbol?.Bid ?? double.NaN;

        public override bool HasValue => _symbol?.HasBid ?? false;


        internal BidSideNode(ISymbolInfoWithRate smb) : base(smb) { }
    }
}
