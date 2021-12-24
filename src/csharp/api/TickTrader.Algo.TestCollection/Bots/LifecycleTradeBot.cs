using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.TestCollection.Bots
{
    [TradeBot(DisplayName = "[T] Lifecycle Trade Bot", Version = "1.1", Category = "Test Bot Routine",
        SetupMainSymbol = false, Description = "Calls sync/async open order in Init, OnStart, AsyncStop, OnStop")]
    public class LifecycleTradeBot : TradeBot
    {
        [Parameter(DisplayName = "Use async calls", DefaultValue = true)]
        public bool UseAsyncCalls { get; set; }


        protected override async void Init()
        {
            if (UseAsyncCalls)
            {
                await OpenOrderAsync(Symbol.Name, OrderType.Limit, OrderSide.Buy, 0.1, null, 1.0, null, null, null, "Init async");
            }
            else
            {
                OpenOrder(Symbol.Name, OrderType.Limit, OrderSide.Buy, 0.1, null, 1.0, null, null, null, "Init sync");
            }
        }

        protected override async void OnStart()
        {
            if (UseAsyncCalls)
            {
                await OpenOrderAsync(Symbol.Name, OrderType.Limit, OrderSide.Buy, 0.1, null, 1.0, null, null, null, "OnStart async");
            }
            else
            {
                OpenOrder(Symbol.Name, OrderType.Limit, OrderSide.Buy, 0.1, null, 1.0, null, null, null, "OnStart sync");
            }
        }

        protected override async Task AsyncStop()
        {
            if (UseAsyncCalls)
                await OpenOrderAsync(Symbol.Name, OrderType.Limit, OrderSide.Buy, 0.1, null, 1.0, null, null, null, "AsyncStop async");
        }

        protected override void OnStop()
        {
            if (!UseAsyncCalls)
                OpenOrder(Symbol.Name, OrderType.Limit, OrderSide.Buy, 0.1, null, 1.0, null, null, null, "OnStop sync");
        }
    }
}
