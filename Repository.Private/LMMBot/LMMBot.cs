using BotCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace LMMBot
{
    [TradeBot]
    public class LMMBot : TradeBot
    {
        bool isStopRequested;
        LMMBotTOMLConfiguration configuration;
        TradeAsync tradeManager;

        [Parameter(DisplayName = "Config File", DefaultValue = "LMMBot.tml")]
        [FileFilter("Toml Config (*.tml)", "*.tml")]
        public File FileConfig { get; set; }

        protected override void OnStart()
        {
            tradeManager = new TradeAsync(this);

            this.Status.WriteLine("Input parameters");
            Nett.Toml.WriteFile<LMMBotTOMLConfiguration>(new LMMBotTOMLConfiguration(), FileConfig.FullPath + "_sample");
            configuration = Nett.Toml.ReadFile<LMMBotTOMLConfiguration>(FileConfig.FullPath);

            base.Status.WriteLine(configuration.ToString());
            foreach (KeyValuePair<string, double> symbol2Volume in configuration.LPSymbols)
            {
                base.Status.WriteLine("LP subscribing to " + symbol2Volume.Key);
                MmLoop(LMMBotTOMLConfiguration.SymbolNameConvertor(symbol2Volume.Key), new LiveCoinFeeder(symbol2Volume.Key)
                    , symbol2Volume.Value);
           }
        }

        private async void MmLoop(string symbol, LiveCoinFeeder feeder, double volume)
        {
            while (!isStopRequested)
            {
                LiveCoinTicker quote = await feeder.GetLatestQuote();
                double askPrice = quote.best_ask * (100 + configuration.MarkupInPercent) / 100;
                double bidPrice = quote.best_bid * (100 - configuration.MarkupInPercent) / 100;
                askPrice = Math.Round(askPrice, Symbols[symbol].Digits);
                bidPrice = Math.Round(bidPrice, Symbols[symbol].Digits);

                tradeManager.SetLimitOrderAsync(symbol, OrderSide.Sell, volume, askPrice, "", configuration.BotTag);
                tradeManager.SetLimitOrderAsync(symbol, OrderSide.Buy, volume, bidPrice, "", configuration.BotTag);
                Status.WriteLine("Rates from livecoin: " + quote.ToString());
                await Task.Delay(1000);
                Status.Flush();
            }
        }

        protected override void OnStop()
        {
            isStopRequested = true;
        }
    }
}
