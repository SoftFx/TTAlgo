using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SoftFX.Extended;

namespace TickTrader.BotTerminal
{
    //internal sealed class SymbolObserver : IDisposable
    //{
    //    private SymbolModel _observableSymbol;

    //    public event Action DepthChanged;

    //    public SymbolObserver() { }

    //    public SymbolObserver(SymbolModel symbol)
    //    {
    //        ObservableSymbol = symbol;
    //    }

    //    public RateDirectionTracker Ask { get; } = new RateDirectionTracker();
    //    public RateDirectionTracker Bid { get; } = new RateDirectionTracker();

    //    public SymbolModel ObservableSymbol
    //    {
    //        get { return _observableSymbol; }
    //        set
    //        {
    //            if (_observableSymbol == value)
    //                return;

    //            Unsubscribe();
    //            _observableSymbol = value;
    //            GetSnapshotAndSubscribe();
    //        }
    //    }

    //    private void Unsubscribe()
    //    {
    //        if (_observableSymbol != null)
    //        {
    //            _observableSymbol.Unsubscribe(this);
    //            _observableSymbol.InfoUpdated -= SymbolInfoUpdated;
    //        }
    //    }

    //    private void GetSnapshotAndSubscribe()
    //    {
    //        if (_observableSymbol != null)
    //        {
    //            Ask.Precision = _observableSymbol.Descriptor.Precision;
    //            Ask.Rate = _observableSymbol.LastQuote?.Ask;

    //            Bid.Precision = _observableSymbol.Descriptor.Precision;
    //            Bid.Rate = _observableSymbol.LastQuote?.Bid;

    //            _observableSymbol.Subscribe(this);
    //            _observableSymbol.InfoUpdated += SymbolInfoUpdated;
    //        }
    //    }

    //    int IRateUpdatesListener.Depth
    //    {
    //        get
    //        {
    //            throw new NotImplementedException();
    //        }
    //    }

    //    void IRateUpdatesListener.OnRateUpdate(Quote tick)
    //    {
    //        if (tick.HasAsk)
    //            Ask.Rate = tick.Ask;

    //        if (tick.HasBid)
    //            Bid.Rate = tick.Bid;
    //    }

    //    private void SymbolInfoUpdated(SymbolInfo obj)
    //    {
    //        Bid.Precision = obj.Precision;
    //        Ask.Precision = obj.Precision;
    //    }

    //    public void Dispose()
    //    {
    //        Unsubscribe();
    //    }
    //}
}
