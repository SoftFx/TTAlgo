using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.TestCollection.Bots
{
    [TradeBot(DisplayName = "[T] On Stop Bot", Version = "1.0", Category = "Test Bot Routine",
        Description = "The purpose of this bot is to test if OnStop and AsyncStop are called and order commands are available during OnStop and AsyncStop. " +
            "If CallExit is set, bot will exit immediately after start otherwise you should manually stop it.")]
    public class OnStopBot : TradeBot
    {
        [Parameter(DefaultValue = false)]
        public bool CallExit { get; set; }

        protected override void Init()
        {
            if (CallExit)
                Exit();
        }

        protected async override Task AsyncStop()
        {
            Print("AsyncStop!");
            await OpenOrderAsync(Symbol.Name, OrderType.Market, OrderSide.Buy, 0.1, null, null, null);
        }

        protected override void OnStop()
        {
            Print("OnStop!");
            OpenOrder(Symbol.Name, OrderType.Market, OrderSide.Buy, 0.1, null, null, null);
        }
    }
}
