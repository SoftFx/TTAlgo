using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TickTrader.Algo.Api;

namespace Test
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
            return "Last: " + last;
        }
    }
    public class BookBand : BookEntry
    {
        public double Price { get; internal set; }
        public double Volume { get; internal set; }
    }
    public class OrderBook
    {
        public long timestamp { get; set; }
        public List<List<double>> asks { get; set; }
        public List<List<double>> bids { get; set; }

        static public BookEntry[] ConvertToBookEntry(List<List<double>> oBook)
        {
            List<BookBand> listBookBand = new List<BookBand>();
            foreach (List<double> entry in oBook)
                listBookBand.Add(new BookBand { Price = entry[0],Volume = entry[1]});
            return listBookBand.ToArray<BookBand>();
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            HttpClient request = new HttpClient();
            request.DefaultRequestHeaders.Add("Accept", "application/json");
            Task<string> taskString = request.GetStringAsync("https://api.livecoin.net/exchange/order_book?currencyPair=EMC/USD");
            Console.WriteLine(taskString.Result);

            OrderBook orderBook = JsonConvert.DeserializeObject<OrderBook>(taskString.Result);
            Console.WriteLine(orderBook.ToString());
        }

    }
}
