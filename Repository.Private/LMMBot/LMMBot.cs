using BotCommon;
using System;
using System.Collections.Generic;
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
            int loopsCount = 0;
            foreach (KeyValuePair<string, double> symbol2Volume in configuration.LPSymbols)
            {
                if (TryRunMmLoop(symbol2Volume))
                    loopsCount++;
            }

            if (loopsCount == 0)
                Exit();
        }

        private bool TryRunMmLoop(KeyValuePair<string, double> config)
        {
            var localSymbolName = LMMBotTOMLConfiguration.ConvertToLocalSymbolName(config.Key);
            Symbol symbol;
            if (TryGetSymbol(localSymbolName, out symbol))
            {
                if (IsVolumeValid(symbol, config.Value))
                {
                    Print($"LP subscribing to {config.Key}");
                    MmLoop(symbol, new LiveCoinFeeder(config.Key), config.Value);
                    return true;
                }
                else
                {
                    PrintError($"TradeVolume {config.Value} is invalid for '{config.Key}'. Volume should be between { symbol.MinTradeVolume} and {symbol.MaxTradeVolume}");
                }
            }
            else
            {
                PrintError("Cannot find symbol: {0} ", localSymbolName);
            }

            return false;
        }

        private bool TryGetSymbol(string localSymbolName, out Symbol symbol)
        {
            return !(symbol = Symbols[localSymbolName]).IsNull;
        }
        private bool IsVolumeValid(Symbol symbol, double volume)
        {
            return (volume - symbol.MinTradeVolume) > -Double.Epsilon
                && (symbol.MaxTradeVolume - volume) > -Double.Epsilon;
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
                    Status.WriteLine($"Rates from livecoin: {quote}");
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
