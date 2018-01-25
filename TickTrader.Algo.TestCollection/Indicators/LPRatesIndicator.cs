using System.Collections.Generic;
using System.Net.Http;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http.Headers;
using TickTrader.Algo.Api;
using System;
using System.Linq;
using TickTrader.SeriesStorage;
using System.Threading.Tasks;
using TickTrader.SeriesStorage.LevelDb;

namespace TickTrader.Algo.TestCollection.Indicators
{
    [Indicator(Category = "Rates Indicator", DisplayName = "[T] LP Rates Indicator", Version = "1.0",
        Description = "Shows rates from different servers.")]
    class LPRatesIndicator : Indicator
    {
        #region Symbols
        [Parameter(DisplayName = "SoftFX server", DefaultValue = "cryptottlivewebapi.xbtce.net:8443")]
        public string SoftFxServer { get; set; }

        [Parameter(DisplayName = "SoftFX Symbol", DefaultValue = "BTCUSD")]
        public string SoftFxSymbol { get; set; }

        [Parameter(DisplayName = "Tidex Symbol", DefaultValue = "btc_wusd")]
        public string TidexSymbol { get; set; }

        [Parameter(DisplayName = "Livecoin Symbol", DefaultValue = "BTC/USD")]
        public string LivecoinSymbol { get; set; }

        [Parameter(DisplayName = "Okex Symbol", DefaultValue = "btc_usdt")]
        public string OkexSymbol { get; set; }

        [Parameter(DisplayName = "Binance Symbol", DefaultValue = "BTCUSDT")]
        public string BinanceSymbol { get; set; }
        #endregion

        #region BidOutpus
        [Output(DisplayName = "SoftFx Bid", Target = OutputTargets.Overlay, DefaultColor = Colors.Black, PlotType = PlotType.DiscontinuousLine)]
        public DataSeries SoftFxBid { get; set; }

        [Output(DisplayName = "Tidex Bid", Target = OutputTargets.Overlay, DefaultColor = Colors.SandyBrown, PlotType = PlotType.DiscontinuousLine)]
        public DataSeries TidexBid { get; set; }

        [Output(DisplayName = "Livecoin Bid", Target = OutputTargets.Overlay, DefaultColor = Colors.DarkRed, PlotType = PlotType.DiscontinuousLine)]
        public DataSeries LivecoinBid { get; set; }

        [Output(DisplayName = "Okex Bid", Target = OutputTargets.Overlay, DefaultColor = Colors.ForestGreen, PlotType = PlotType.DiscontinuousLine)]
        public DataSeries OkexBid { get; set; }

        [Output(DisplayName = "Binance Bid", Target = OutputTargets.Overlay, DefaultColor = Colors.Gold, PlotType = PlotType.DiscontinuousLine)]
        public DataSeries BinanceBid { get; set; }
        #endregion

        #region AskOutputs
        [Output(DisplayName = "SoftFx Ask", Target = OutputTargets.Overlay, DefaultColor = Colors.Black, DefaultLineStyle = LineStyles.LinesDots, PlotType = PlotType.DiscontinuousLine)]
        public DataSeries SoftFxAsk { get; set; }

        [Output(DisplayName = "Tidex Ask", Target = OutputTargets.Overlay, DefaultColor = Colors.SandyBrown, DefaultLineStyle = LineStyles.LinesDots, PlotType = PlotType.DiscontinuousLine)]
        public DataSeries TidexAsk { get; set; }

        [Output(DisplayName = "Livecoin Ask", Target = OutputTargets.Overlay, DefaultColor = Colors.DarkRed, DefaultLineStyle = LineStyles.LinesDots, PlotType = PlotType.DiscontinuousLine)]
        public DataSeries LivecoinAsk { get; set; }

        [Output(DisplayName = "Okex Ask", Target = OutputTargets.Overlay, DefaultColor = Colors.ForestGreen, DefaultLineStyle = LineStyles.LinesDots, PlotType = PlotType.DiscontinuousLine)]
        public DataSeries OkexAsk { get; set; }

        [Output(DisplayName = "Binance Ask", Target = OutputTargets.Overlay, DefaultColor = Colors.Gold, DefaultLineStyle = LineStyles.LinesDots, PlotType = PlotType.DiscontinuousLine)]
        public DataSeries BinanceAsk { get; set; }
        #endregion

        private Dictionary<LiquidityProvider, string> _tickerUrl;
        private List<LiquidityProvider> _requiredLP;

        private HttpClient _softFxClient;
        private HttpClient _defaultClient;

        private readonly static IKeySerializer<DateTime> _keySerializer = new DateTimeKeySerializer();
        private readonly DateTime _startTime = DateTime.Now.ToUniversalTime();
        private string _dbPath;
        private bool _isPreviousBarsProccessed = false;

        protected override void Init()
        {
            var pathInfo = System.IO.Directory.CreateDirectory("LevelDB");
            _dbPath = pathInfo.Name + "\\" + Symbol.Name;

            InitClients();
            InitTickerUrls();
            InitRequiredLP();
            base.Init();
        }

        protected override void Calculate()
        {
            if (_startTime > Bars[0].CloseTime || IsUpdate)
                return;

            if (!_isPreviousBarsProccessed)
            {
                ProcessPreviousBars();
                _isPreviousBarsProccessed = true;
                return;
            }

            Dictionary<string, double> lps_tick = new Dictionary<string, double>();

            foreach (var lp in _requiredLP)
            {
                var tick = GetTick(lp);
                if (tick.StatusCode == HttpStatusCode.OK)
                {
                    var bid = GetBid(lp, tick);
                    var ask = GetAsk(lp, tick);

                    AddToIndicatorOutput(lp, bid, ask);
                    AddToLpTickDictionary(lp, bid, ask, ref lps_tick);
                }

            }
            SaveToDB(lps_tick);
        }

        #region Initialization methods
        private void InitClients()
        {
            InitSoftFxClient();
            InitDefaultClient();
        }

        private void InitTickerUrls()
        {
            _tickerUrl = new Dictionary<LiquidityProvider, string>();

            _tickerUrl.Add(LiquidityProvider.SoftFx, $"https://{SoftFxServer}/api/v1/public/tick/{SoftFxSymbol}");
            _tickerUrl.Add(LiquidityProvider.Tidex, $"https://api.tidex.com/api/3/ticker/{TidexSymbol}");
            _tickerUrl.Add(LiquidityProvider.Livecoin, $"https://api.livecoin.net//exchange/ticker?currencyPair={LivecoinSymbol}");
            _tickerUrl.Add(LiquidityProvider.Okex, $"https://www.okex.com/api/v1/ticker.do?symbol={OkexSymbol}");
            _tickerUrl.Add(LiquidityProvider.Binance, $"https://api.binance.com/api/v3/ticker/bookTicker?symbol={BinanceSymbol}");
        }

        private void InitRequiredLP()
        {
            _requiredLP = new List<LiquidityProvider>();

            if (!SoftFxSymbol.Equals(""))
                _requiredLP.Add(LiquidityProvider.SoftFx);

            if (!LivecoinSymbol.Equals(""))
                _requiredLP.Add(LiquidityProvider.Livecoin);

            if (!TidexSymbol.Equals(""))
                _requiredLP.Add(LiquidityProvider.Tidex);

            if (!OkexSymbol.Equals(""))
                _requiredLP.Add(LiquidityProvider.Okex);

            if (!BinanceSymbol.Equals(""))
                _requiredLP.Add(LiquidityProvider.Binance);

        }

        private void InitSoftFxClient()
        {
            HttpClientHandler handler = new HttpClientHandler();
            handler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            _softFxClient = new HttpClient(handler);
            _softFxClient.DefaultRequestHeaders.Accept.Clear();
            _softFxClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (SoftFxServer.Equals("tp.st.soft-fx.eu:8443"))
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
        }

        private void InitDefaultClient()
        {
            _defaultClient = new HttpClient();
            _defaultClient.DefaultRequestHeaders.Add("Accept", "application/json");
        }
        #endregion

        private HttpResponseMessage GetTick(LiquidityProvider lp)
        {
            _tickerUrl.TryGetValue(lp, out var url);

            if (lp == LiquidityProvider.SoftFx)
                return _softFxClient.GetAsync(url).Result;
            else
                return _defaultClient.GetAsync(url).Result;
        }

        private void AddToIndicatorOutput(LiquidityProvider lp, double bid, double ask, int position = 0)
        {
            switch (lp)
            {
                case LiquidityProvider.SoftFx:
                    SoftFxBid[position] = bid;
                    SoftFxAsk[position] = ask;
                    break;
                case LiquidityProvider.Livecoin:
                    LivecoinBid[position] = bid;
                    LivecoinAsk[position] = ask;
                    break;
                case LiquidityProvider.Tidex:
                    TidexBid[position] = bid;
                    TidexAsk[position] = ask;
                    break;
                case LiquidityProvider.Okex:
                    OkexBid[position] = bid;
                    OkexAsk[position] = ask;
                    break;
                case LiquidityProvider.Binance:
                    BinanceBid[position] = bid;
                    BinanceAsk[position] = ask;
                    break;
            }
        }

        private void AddToLpTickDictionary(LiquidityProvider lp, double bid, double ask, ref Dictionary<string, double> dict)
        {
            dict.Add(lp.ToString() + "Bid", bid);
            dict.Add(lp.ToString() + "Ask", ask);
        }

        private double GetBid(LiquidityProvider lp, HttpResponseMessage response)
        {
            dynamic quote = JsonConvert.DeserializeObject(response.Content.ReadAsStringAsync().Result);

            switch (lp)
            {
                case LiquidityProvider.SoftFx:
                    return quote[0]["BestBid"]["Price"];

                case LiquidityProvider.Livecoin:
                    return quote["best_bid"];

                case LiquidityProvider.Tidex:
                    return quote[TidexSymbol]["buy"];

                case LiquidityProvider.Okex:
                    string okexStr = quote["ticker"]["buy"];
                    return (double.TryParse(okexStr, out double okexBid)) ? okexBid : double.NaN;

                case LiquidityProvider.Binance:
                    string binanceStr = quote["bidPrice"];
                    return (double.TryParse(binanceStr, out double binanceBid)) ? binanceBid : double.NaN;

                default:
                    return double.NaN;
            }
        }

        private double GetAsk(LiquidityProvider lp, HttpResponseMessage response)
        {
            dynamic quote = JsonConvert.DeserializeObject(response.Content.ReadAsStringAsync().Result);

            switch (lp)
            {
                case LiquidityProvider.SoftFx:
                    return quote[0]["BestAsk"]["Price"];

                case LiquidityProvider.Livecoin:
                    return quote["best_ask"];

                case LiquidityProvider.Tidex:
                    return quote[TidexSymbol]["sell"];

                case LiquidityProvider.Okex:
                    string okexStr = quote["ticker"]["sell"];
                    return (double.TryParse(okexStr, out double okexAsk)) ? okexAsk : double.NaN;

                case LiquidityProvider.Binance:
                    string binanceStr = quote["askPrice"];
                    return (double.TryParse(binanceStr, out double binanceAsk)) ? binanceAsk : double.NaN;

                default:
                    return double.NaN;
            }
        }

        private void ProcessPreviousBars()
        {
            using (var storage = new LevelDbStorage(_dbPath))
            {
                for (int pos = Bars.Count - 1; pos > 0; pos--)
                    foreach (var lp in _requiredLP)
                    {
                        var bid = double.NaN;
                        var ask = double.NaN;

                        if (storage.Collections.Contains(lp.ToString() + "Bid"))
                            using (var bidCollection = storage.GetBinaryCollection(lp.ToString() + "Bid", _keySerializer))
                            {
                                if (bidCollection.Read(Bars[pos].OpenTime, out var seg))
                                    bid = BitConverter.ToDouble(seg.Array, 0);
                            }

                        if (storage.Collections.Contains(lp.ToString() + "Ask"))
                            using (var askCollection = storage.GetBinaryCollection(lp.ToString() + "Ask", _keySerializer))
                            {
                                if (askCollection.Read(Bars[pos].OpenTime, out var seg))
                                    ask = BitConverter.ToDouble(seg.Array, 0);
                            }

                        AddToIndicatorOutput(lp, bid, ask, pos);
                    }
            }
        }

        private void SaveToDB(Dictionary<string, double> dict)
        {
            using (var storage = new LevelDbStorage(_dbPath))
            {
                foreach (var lp in dict)
                    using (var collection = storage.GetBinaryCollection(lp.Key, _keySerializer))
                    {
                        collection.Write(Bars[0].OpenTime, GetSegment(lp.Value));
                    }
            }
        }

        public static ArraySegment<byte> GetSegment(double src)
        {
            return new ArraySegment<byte>(BitConverter.GetBytes(src));
        }
    }

    public enum LiquidityProvider { SoftFx, Livecoin, Tidex, Okex, Binance };

    public class DateTimeKeySerializer : IKeySerializer<DateTime>
    {
        public int KeySize => 8;

        public DateTime Deserialize(IKeyReader reader)
        {
            var ticks = reader.ReadBeLong();
            return new DateTime(ticks);
        }

        public void Serialize(DateTime key, IKeyBuilder builder)
        {
            builder.WriteBe(key.Ticks);
        }
    }
}