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

    public class classBookEntry : BookEntry
    {
        public double Price { get; internal set; }
        public double Volume { get; internal set; }
    }
    public class classQuote : Quote
    {
        public double Ask
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public BookEntry[] AskBook
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public double Bid
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public BookEntry[] BidBook
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public string Symbol
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public DateTime Time
        {
            get
            {
                throw new NotImplementedException();
            }
        }
    }

    public class LivecoinOrderBook
    {
        public long timestamp { get; set; }
        public List<List<double>> asks { get; set; }
        public List<List<double>> bids { get; set; }

        //static public BookEntry[] ConvertToBookEntry(List<List<double>> oBook)
        //{
        //    List<classBookEntry> listBookBand = new List<classBookEntry>();
        //    foreach (List<double> entry in oBook)
        //        listBookBand.Add(new classBookEntry { Price = entry[0], Volume = entry[1] });
        //    return listBookBand.ToArray<classBookEntry>();
        //}
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
