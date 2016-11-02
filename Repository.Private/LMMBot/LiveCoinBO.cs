using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace LMMBot
{
    public class LiveCoinTicker
    {
        public double last { get; set; }
        public double high { get; set; }
        public double low { get; set; }
        public double volume { get; set; }
        public double vwap { get; set; }
        public double max_bid { get; set; }
        public double min_ask { get; set; }
        public double best_bid { get; set; }
        public double best_ask { get; set; }
        public override string ToString()
        {
            return "best_bid: " + best_bid + "; best_ask: " + best_ask;
        }
    }

    public class LiveCoinFeeder
    {
        HttpClient httpClient;
        const string basicAddress = "https://api.livecoin.net//exchange/ticker?currencyPair=";
        readonly string requestUrl;

        public LiveCoinFeeder(string symbol)
        {
            requestUrl = basicAddress + symbol;
            httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        }
        public async Task<LiveCoinTicker> GetLatestQuote()
        {
            string response = await httpClient.GetStringAsync(requestUrl);
            LiveCoinTicker quote = JsonConvert.DeserializeObject<LiveCoinTicker>(response);
            return quote;
        }
    }

}
