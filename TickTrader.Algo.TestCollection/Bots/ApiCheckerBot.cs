using TickTrader.Algo.Api;
using System;
using System.Net;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using Newtonsoft.Json;

namespace ApiCheckerBot
{
    [TradeBot(DisplayName = "[T] Api Checker Bot", Version = "1.0", Category = "Test Plugin Info",
        Description = "Diagnostic bot, that checks public web API methods of SoftFX servers.")]
    public class ApiCheckerBot : TradeBot
    {
        [Parameter(DisplayName = "Repeat Period (msec)", DefaultValue = 5000)]
        public int RepeatMsec { get; set; }

        //[Parameter(DisplayName = "Server URL", DefaultValue = "https://cryptottlivewebapi.xbtce.net:8443")]
        [Parameter(DisplayName = "Server URL", DefaultValue = "https://tp.st.soft-fx.eu:8443")]
        public string ServerName { get; set; }

        [Parameter(DisplayName = "Currency", DefaultValue = true)]
        public bool Currency { get; set; }

        [Parameter(DisplayName = "Level2", DefaultValue = false)]
        public bool Level2 { get; set; }

        [Parameter(DisplayName = "Quotehistory", DefaultValue = false)]
        public bool QuoteHistory { get; set; }

        [Parameter(DisplayName = "Symbol", DefaultValue = false)]
        public bool ApiSymbol { get; set; }

        [Parameter(DisplayName = "Tick", DefaultValue = false)]
        public bool Tick { get; set; }

        [Parameter(DisplayName = "Ticker", DefaultValue = false)]
        public bool Ticker { get; set; }

        [Parameter(DisplayName = "Tradesession", DefaultValue = false)]
        public bool Tradesession { get; set; }

        private Dictionary<OperationType, string> _operations;
        private Dictionary<OperationType, int> _operationErrors;
        private Dictionary<OperationType, int> _operationTests;

        private HttpClient _httpClient;

        private List<string> _symbols;
        private int _loopCount = 0;

        protected override void Init()
        {
            _httpClient = new HttpClient(new PublicContentHandler());
            _operations = new Dictionary<OperationType, string>();
            _operationTests = new Dictionary<OperationType, int>();
            _operationErrors = new Dictionary<OperationType, int>();
            _symbols = new List<string>();

            InitHttpClient();
            InitOperations();
            if (IsSymbolNeed())
                InitSymbolList();

            base.Init();
        }

        protected override async void OnStart()
        {
            var beginTestTime = DateTime.Now.Ticks / 10_000 - RepeatMsec;

            try
            {
                while (true)
                {
                    var testDuration = DateTime.Now.Ticks / 10_000 - beginTestTime;

                    var residueTime = RepeatMsec - testDuration;

                    if (residueTime > 0)
                        Thread.Sleep((int)residueTime);
                    else
                    {
                        _loopCount++;

                        var loopRequests = _operationTests.Sum(v => v.Value);
                        beginTestTime = DateTime.Now.Ticks / 10_000;

                        foreach (var operation in _operations.Keys)
                            await OperationTest(operation);


                        var loopTime = DateTime.Now.Ticks / 10_000 - beginTestTime;
                        loopRequests = _operationTests.Sum(v => v.Value) - loopRequests;

                        var perfomanceInfo = $"Last loop:  {loopTime} msec, {loopRequests} req, {loopRequests * 1000 / loopTime} req/sec";


                        PrintResultStatus(perfomanceInfo);
                    }
                }
            }
            catch (Exception e)
            {
                ExitWithError(e.Message);
            }
        }

        private void InitHttpClient()
        {
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            if (ServerName.Equals("https://tp.st.soft-fx.eu:8443"))
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
        }

        private void InitOperations()
        {
            if (Currency)
            {
                _operations[OperationType.Currency] = "/api/v1/public/currency";
                _operationErrors[OperationType.Currency] = 0;
                _operationTests[OperationType.Currency] = 0;
            }


            if (Level2)
            {
                _operations[OperationType.Level2] = "/api/v1/public/level2";
                _operationErrors[OperationType.Level2] = 0;
                _operationTests[OperationType.Level2] = 0;
            }

            if (QuoteHistory)
            {
                _operations[OperationType.QuoteHistory] = "/api/v1/public/quotehistory";
                _operationErrors[OperationType.QuoteHistory] = 0;
                _operationTests[OperationType.QuoteHistory] = 0;
            }

            if (ApiSymbol)
            {
                _operations[OperationType.Symbol] = "/api/v1/public/symbol";
                _operationErrors[OperationType.Symbol] = 0;
                _operationTests[OperationType.Symbol] = 0;
            }

            if (Tick)
            {
                _operations[OperationType.Tick] = "/api/v1/public/tick";
                _operationErrors[OperationType.Tick] = 0;
                _operationTests[OperationType.Tick] = 0;
            }

            if (Ticker)
            {
                _operations[OperationType.Ticker] = "/api/v1/public/ticker";
                _operationErrors[OperationType.Ticker] = 0;
                _operationTests[OperationType.Ticker] = 0;
            }

            if (Tradesession)
            {
                _operations[OperationType.Tradesession] = "/api/v1/public/tradesession";
                _operationErrors[OperationType.Tradesession] = 0;
                _operationTests[OperationType.Tradesession] = 0;
            }
        }

        private void InitSymbolList()
        {
            var requestUrl = $"{ServerName}/api/v1/public/symbol";

            var response = _httpClient.GetAsync(requestUrl).Result;

            if (response.StatusCode == HttpStatusCode.OK)
            {
                dynamic symbols = JsonConvert.DeserializeObject(response.Content.ReadAsStringAsync().Result);
                foreach (var node in symbols)
                    _symbols.Add((string)node.Symbol);

                if (_symbols.Count == 0)
                    ExitWithError($"Cannot init symbol list. Responce for {requestUrl} has no symbol.");
            }
            else
            {
                ExitWithError($"Cannot init symbol list. {response.Content.ReadAsStringAsync().Result.Trim('{', '}')}");
            }


        }

        private bool IsSymbolNeed()
        {
            if (_operations.ContainsKey(OperationType.Level2) ||
                _operations.ContainsKey(OperationType.QuoteHistory) ||
                _operations.ContainsKey(OperationType.Symbol) ||
                _operations.ContainsKey(OperationType.Tick) ||
                _operations.ContainsKey(OperationType.Ticker))
                return true;
            else
                return false;
        }

        private void ExitWithError(string message)
        {
            PrintError(message);
            Exit();
        }

        private async Task HttpRequestTest(OperationType operation, string requestUrl)
        {
            var formattedRequestUrl = requestUrl.Replace("#", "%23");

            var response = await _httpClient.GetAsync(formattedRequestUrl);

            var result = $"{requestUrl}. {GetDescription(response.StatusCode)}";

            _operationTests[operation]++;

            if (response.StatusCode == HttpStatusCode.OK)
                if (response.Content.ReadAsStringAsync().Result.Equals("[]"))
                {
                    PrintError($"{requestUrl}. Returned empty responce body.");
                    _operationErrors[operation]++;
                }
                else
                    Print(result);
            else
            {
                PrintError(result);
                _operationErrors[operation]++;
            }

        }

        private async Task OperationTest(OperationType operation)
        {
            switch (operation)
            {
                case OperationType.Currency:
                    await CurrencyTest(operation);
                    break;
                case OperationType.QuoteHistory:
                    await QuoteHistoryTest(operation);
                    break;
                case OperationType.Tradesession:
                    await TradesessionTest(operation);
                    break;
                default:
                    await StandartTest(operation);
                    break;
            }
        }

        private async Task StandartTest(OperationType operation)
        {
            var requestUrl = $"{ServerName}{_operations[operation]}";
            await HttpRequestTest(operation, requestUrl);

            foreach (var symbol in _symbols)
                await HttpRequestTest(operation, $"{requestUrl}/{symbol}");
        }

        private async Task CurrencyTest(OperationType operation)
        {
            var requestUrl = $"{ServerName}{_operations[operation]}";
            await HttpRequestTest(operation, requestUrl);


            var response = _httpClient.GetAsync(requestUrl).Result;
            if (response.StatusCode == HttpStatusCode.OK)
            {
                dynamic currencies = JsonConvert.DeserializeObject(response.Content.ReadAsStringAsync().Result);
                foreach (var currency in currencies)
                    await HttpRequestTest(operation, $"{requestUrl}/{(string)currency.Name}");
            }
        }

        private async Task QuoteHistoryTest(OperationType operation)
        {
            Random rand = new Random();

            var timestamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 2, 1))).TotalSeconds;
            var randomSymbol = _symbols[rand.Next() % _symbols.Count];
            var periodicity = "H1";
            var ticksCount = 100;

            var quoteHistoryUrls = new string[]
            {
                $"{ServerName}/api/v1/public/quotehistory/{randomSymbol}/{periodicity}/bars/ask?timestamp={timestamp}&count={ticksCount}",
                $"{ServerName}/api/v1/public/quotehistory/{randomSymbol}/{periodicity}/bars/bid?timestamp={timestamp}&count={ticksCount}",
                $"{ServerName}/api/v1/public/quotehistory/{randomSymbol}/level2?timestamp={timestamp}&count={ticksCount}",
                $"{ServerName}/api/v1/public/quotehistory/{randomSymbol}/ticks?timestamp={timestamp}&count={ticksCount}"
            };

            foreach (var url in quoteHistoryUrls)
                await HttpRequestTest(operation, url);

        }

        private async Task TradesessionTest(OperationType operation)
        {
            var requestUrl = $"{ServerName}{_operations[operation]}";
            await HttpRequestTest(operation, requestUrl);
        }

        private void PrintResultStatus(string lastLoopInfo)
        {
            Status.WriteLine("Server:  {0}", ServerName);
            Status.WriteLine("Total loops:  {0}", _loopCount);
            Status.WriteLine(lastLoopInfo);
            Status.WriteLine("\nTests\tErrors\tApi Method");
            foreach (var operation in _operations.Keys)
                Status.WriteLine("{0}\t{1}\t{2}", _operationTests[operation], _operationErrors[operation], _operations[operation]);
            Status.WriteLine();
        }

        private string GetDescription(HttpStatusCode cd)
        {
            switch (cd)
            {
                case (HttpStatusCode)200:
                    return "Success";
                case (HttpStatusCode)400:
                    return "Bad Request. The request could not be understood by the server due to malformed syntax.";
                case (HttpStatusCode)404:
                    return "History not found";
                case (HttpStatusCode)429:
                    return "Too Many Requests. The server blocks requeust because the throttling quote was exceeded.";
                case (HttpStatusCode)500:
                    return "Internal Server Error. The server encountered an unexpected condition which prevented it from fulfilling the request.";
                default:
                    return "Unknown http status code";
            }
        }
    }

    public class PublicContentHandler : HttpClientHandler
    {
        public PublicContentHandler()
        {
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
        }
    }

    public enum OperationType { Currency, Level2, QuoteHistory, Symbol, Tick, Ticker, Tradesession }
}
