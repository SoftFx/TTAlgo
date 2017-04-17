using System.Linq;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.TestCollection.Bots
{
    [TradeBot(DisplayName = "[T] Cancel All Limits/Stops Script", Version = "1.0", Category = "Test Orders",
        Description = "Cancels all pending orders one by one.")]
    public class CancelAll : TradeBot
    {
        protected async override void OnStart()
        {
            var pendings = Account.Orders.Where(o => o.Type != OrderType.Position).ToList();

            foreach (var order in pendings)
                await CancelOrderAsync(order.Id);

            Exit();
        }
    }
}
