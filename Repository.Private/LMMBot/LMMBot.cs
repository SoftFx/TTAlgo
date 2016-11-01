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
                var localSymbolName = LMMBotTOMLConfiguration.SymbolNameConvertor(symbol2Volume.Key);
                Symbol smbInfo = Symbols[localSymbolName];
                if (smbInfo.IsNull)
                    PrintError("Cannot find symbol: {0} ", localSymbolName);
                else
                {
                    Print("LP subscribing to " + symbol2Volume.Key);
                    MmLoop(smbInfo, new LiveCoinFeeder(symbol2Volume.Key), symbol2Volume.Value);
                }
           }
        }

        private async void MmLoop(Symbol symbol, LiveCoinFeeder feeder, double volume)
        {
            while (!isStopRequested)
            {
                try
                {
                    LiveCoinTicker quote = await feeder.GetLatestQuote();
                    double askPrice = quote.best_ask * (100 + configuration.MarkupInPercent) / 100;
                    double bidPrice = quote.best_bid * (100 - configuration.MarkupInPercent) / 100;
                    askPrice = Math.Round(askPrice, symbol.Digits);
                    bidPrice = Math.Round(bidPrice, symbol.Digits);

                    tradeManager.SetLimitOrderAsync(symbol.Name, OrderSide.Sell, volume, askPrice, "", configuration.BotTag);
                    tradeManager.SetLimitOrderAsync(symbol.Name, OrderSide.Buy, volume, bidPrice, "", configuration.BotTag);
                    Status.WriteLine("Rates from livecoin: " + quote.ToString());
                }
                catch (Exception ex)
                {
                    PrintError("Error on symbol {0}: {1}", symbol.Name, ex.Message);
                }
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
