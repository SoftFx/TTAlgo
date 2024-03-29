﻿using TickTrader.Algo.Api;

namespace TickTrader.Algo.TestCollection.Bots
{
    [TradeBot(DisplayName = "[T] Get Currency Info Bot", Version = "1.1", Category = "Test Plugin Info",
        SetupMainSymbol = false, Description = "Prints info about currency with specified code to bot status window")]
    public class GetCurrencyInfoBot : TradeBotCommon
    {
        [Parameter(DisplayName = "Currency Code", DefaultValue = "USD")]
        public string CurrencyCode { get; set; }


        protected override void Init()
        {
            Status.WriteLine(ToObjectPropertiesString(CurrencyCode, Currencies[CurrencyCode]));

            Exit();
        }
    }
}