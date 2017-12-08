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
        void OnError(Exception ex);
        void OnInitialized();
        void OnStart();
        void OnStop();
        void OnExit();
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

        public static DiagnosticInfo Diagnostics => nullDiagnostics;
        public static Order Order => order;
        public static BookEntry[] Book => book;
        public static Quote Quote => quote;
        public static IPluginLogger Logger => nullLogger;
        public static Currency Currency => currency;
        public static Asset Asset => asset;
        public static BarSeries BarSeries => barSeries;
        public static QuoteSeries QuoteSeries => quoteSeries;
    }

    internal class PluginLoggerAdapter : IPluginMonitor
    {
        
        private IPluginLogger logger;

        public PluginLoggerAdapter()
        {
            this.logger = Null.Logger;
        }

        public IPluginLogger Logger
        {
            get { return logger; }
            set
            {
                if (value == null)
                    throw new InvalidOperationException("Logger cannot be null!");

                this.logger = value;
            }
        }

        public void UpdateStatus(string status)
        {
            logger.UpdateStatus(status);
        }

        public void Print(string entry)
        {
            logger.OnPrint(entry);
        }

        public void Print(string entry, object[] parameters)
        {
            logger.OnPrint(entry, parameters);
        }

        public void PrintError(string entry)
        {
            logger.OnPrintError(entry);
        }

        public void PrintError(string entry, object[] parameters)
        {
            logger.OnPrintError(entry, parameters);
        }

        public void PrintInfo(string entry)
        {
            logger.OnPrintInfo(entry);
        }

        public void PrintTrade(string entry)
        {
            logger.OnPrintTrade(entry);
        }
    }

    public class NullLogger : IPluginLogger
    {
        public void OnError(Exception ex)
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

        public void OnStart()
        {
        }

        public void OnStop()
        {
        }

        public void UpdateStatus(string status)
        {
        }
    }

    internal class NullTradeApi : TradeCommands
    {
        private static Task<OrderCmdResult> rejectResult
            = Task.FromResult<OrderCmdResult>(new TradeResultEntity(OrderCmdResultCodes.Unsupported, OrderEntity.Null));

        public Task<OrderCmdResult> CancelOrder(bool isAysnc, string orderId)
        {
            return rejectResult;
        }

        public Task<OrderCmdResult> CloseOrder(bool isAysnc, string orderId, double? volume)
        {
            return rejectResult;
        }

        public Task<OrderCmdResult> CloseOrderBy(bool isAysnc, string orderId, string byOrderId)
        {
            return rejectResult;
        }

        [Obsolete]
        public Task<OrderCmdResult> ModifyOrder(bool isAysnc, string orderId, double price, double? tp, double? sl, string comment)
        {
            return rejectResult;
        }

        public Task<OrderCmdResult> ModifyOrder(bool isAysnc, string orderId, double? price, double? stopPrice, double? maxVisibleVolume, double? sl, double? tp, string comment, DateTime? expiration)
        {
            return rejectResult;
        }

        [Obsolete]
        public Task<OrderCmdResult> OpenOrder(bool isAysnc, string symbol, OrderType type, OrderSide side, double price, double volume, double? tp, double? sl, string comment, OrderExecOptions options, string tag)
        {
            return rejectResult;
        }

        public Task<OrderCmdResult> OpenOrder(bool isAysnc, string symbol, OrderType type, OrderSide side, double volume, double? maxVisibleVolume, double? price, double? stopPrice, double? sl, double? tp, string comment, OrderExecOptions options, string tag, DateTime? expiration)
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
        public int Digits => -1;
        public bool IsNull => true;
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

        public void Subscribe(string symbol, int depth = 1)
        {
        }

        public void Unsubscribe(string symbol)
        {
        }
    }
}
