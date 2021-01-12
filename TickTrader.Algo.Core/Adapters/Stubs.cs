using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.Core
{
    public interface IPluginLogger
    {
        void UpdateStatus(string status);
        void OnPrintInfo(string info);
        void OnPrint(string entry);
        void OnPrint(string entry, params object[] parameters);
        void OnPrintError(string entry);
        void OnPrintError(string entry, params object[] parameters);
        void OnPrintTrade(string entry);
        void OnPrintTradeSuccess(string entry);
        void OnPrintTradeFail(string entry);
        void OnPrintAlert(string entry);
        void OnError(Exception ex);
        void OnError(string message, Exception ex);
        void OnError(string message);
        void OnInitialized();
        void OnStart();
        void OnStop();
        void OnExit();
        void OnAbort();
        void OnConnected();
        void OnDisconnected();
        void OnConnectionInfo(string connectionInfo);
    }

    public static class Null
    {
        private static readonly DiagnosticInfo nullDiagnostics = new NullDiagnosticInfo();
        private static readonly Order order = new NullOrder();
        private static readonly BookEntry[] book = new BookEntry[0];
        private static readonly Quote quote = new NullQuote();
        private static readonly IPluginLogger nullLogger = new NullLogger();
        private static readonly Currency currency = new NullCurrency();
        private static readonly Asset asset = new NullAsset();
        private static readonly BarSeries barSeries = new BarSeriesProxy() { Buffer = new EmptyBuffer<Bar>() };
        private static readonly QuoteSeries quoteSeries = new QuoteSeriesProxy() { Buffer = new EmptyBuffer<Quote>() };
        private static readonly ITimerApi timerApi = new NullTimerApi();
        private static readonly ITradeApi tradeApi = new NullTradeApi();

        public static DiagnosticInfo Diagnostics => nullDiagnostics;
        public static Order Order => order;
        public static BookEntry[] Book => book;
        public static Quote Quote => quote;
        public static IPluginLogger Logger => nullLogger;
        public static Currency Currency => currency;
        public static Asset Asset => asset;
        public static BarSeries BarSeries => barSeries;
        public static QuoteSeries QuoteSeries => quoteSeries;
        internal static ITimerApi TimerApi => timerApi;
        public static ITradeApi TradeApi => tradeApi;
    }

    public class NullLogger : IPluginLogger
    {
        public IAlertAPI Alert { get; }

        public void OnError(Exception ex)
        {
        }

        public void OnError(string message, Exception ex)
        {
        }

        public void OnError(string message)
        {
        }

        public void OnExit()
        {
        }

        public void OnInitialized()
        {
        }

        public void OnPrint(string entry)
        {
        }

        public void OnPrint(string entry, params object[] parameters)
        {
        }

        public void OnPrintError(string entry)
        {
        }

        public void OnPrintError(string entry, params object[] parameters)
        {
        }

        public void OnPrintInfo(string info)
        {
        }

        public void OnPrintTrade(string entry)
        {
        }

        public void OnPrintTradeSuccess(string entry)
        {
        }

        public void OnPrintTradeFail(string entry)
        {
        }

        public void OnStart()
        {
        }

        public void OnStop()
        {
        }

        public void OnAbort()
        {
        }

        public void UpdateStatus(string status)
        {
        }

        public void OnConnected()
        {
        }

        public void OnDisconnected()
        {
        }

        public void OnConnectionInfo(string connectionInfo)
        {
        }

        public void OnPrintAlert(string entry)
        {
        }
    }

    internal class NullTradeApi : ITradeApi
    {
        private static Task<TradeResultEntity> rejectResult
            = Task.FromResult<TradeResultEntity>(new TradeResultEntity(OrderCmdResultCodes.Unsupported, null));

        public Task<TradeResultEntity> CancelOrder(bool isAysnc, CancelOrderRequest request)
        {
            return rejectResult;
        }

        public Task<TradeResultEntity> CloseOrder(bool isAysnc, CloseOrderCoreRequest request)
        {
            return rejectResult;
        }

        public Task<TradeResultEntity> ModifyOrder(bool isAysnc, ReplaceOrderCoreRequest request)
        {
            return rejectResult;
        }

        public Task<TradeResultEntity> OpenOrder(bool isAysnc, OpenOrderCoreRequest request)
        {
            return rejectResult;
        }
    }

    internal class NullDiagnosticInfo : DiagnosticInfo
    {
        public int FeedQueueSize { get { return 0; } }
    }

    internal class NullQuote : Quote
    {
        public double Ask { get { return double.NaN; } }
        public double Bid { get { return double.NaN; } }
        public bool HasBid => false;
        public bool HasAsk => false;
        public bool IsAskIndicative => false;
        public bool IsBidIndicative => false;
        public BookEntry[] AskBook { get { return Null.Book; } }
        public BookEntry[] BidBook { get { return Null.Book; } }
        public string Symbol { get { return ""; } }
        public DateTime Time { get { return DateTime.MinValue; } }

        public override string ToString() { return "{null}"; }
    }

    [Serializable]
    public class NullCurrency : Currency
    {
        public NullCurrency() : this("")
        {
        }

        public NullCurrency(string code)
        {
            Name = code;
        }

        public string Name { get; }
        public int Digits => 2;
        public bool IsNull => true;
        public string Type { get; }

        public override string ToString() { return "{null}"; }
    }

    [Serializable]
    public class NullAsset : Asset
    {
        public NullAsset() : this("")
        {
        }

        public NullAsset(string currency)
        {
            Currency = currency;
        }

        public string Currency { get; }
        public Currency CurrencyInfo => Null.Currency;
        public double Volume => 0;
        public double LockedVolume => 0;
        public double FreeVolume => 0;
        public bool IsNull => true;

        public override string ToString() { return "{null}"; }
    }

    public class NullCustomFeedProvider : CustomFeedProvider
    {
        public IEnumerable<Bar> GetBars(string symbol, TimeFrames timeFrame, DateTime from, DateTime to, BarPriceType side, bool backwardOrder)
        {
            return Null.BarSeries;
        }

        public IEnumerable<Bar> GetBars(string symbol, TimeFrames timeFrame, DateTime from, int count, BarPriceType side)
        {
            return Null.BarSeries;
        }

        public BarSeries GetBarSeries(string symbol)
        {
            return Null.BarSeries;
        }

        public BarSeries GetBarSeries(string symbol, BarPriceType side)
        {
            return Null.BarSeries;
        }

        public IEnumerable<Quote> GetQuotes(string symbol, DateTime from, DateTime to, bool level2, bool backwardOrder)
        {
            return Null.QuoteSeries;
        }

        public IEnumerable<Quote> GetQuotes(string symbol, DateTime from, int count, bool level2)
        {
            return Null.QuoteSeries;
        }

        public void Subscribe(string symbol, int depth = 1)
        {
        }

        public void Unsubscribe(string symbol)
        {
        }
    }

    public class NullTimerApi : ITimerApi
    {
        public DateTime Now => throw new NotImplementedException("Timer API is not available!");
        public DateTime UtcNow => throw new NotImplementedException("Timer API is not available!");

        public Timer CreateTimer(TimeSpan period, Action<Timer> callback)
        {
            throw new NotImplementedException("Timer API is not available!");
        }

        public Task Delay(TimeSpan period)
        {
            throw new NotImplementedException("Timer API is not available!");
        }
    }
}
