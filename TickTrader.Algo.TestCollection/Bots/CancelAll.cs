using System.Linq;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.TestCollection.Bots
{
    [TradeBot(DisplayName = "[T] Cancel All Limits/Stops Script", Version = "1.1", Category = "Test Orders",
        SetupMainSymbol = false, Description = "Cancels all pending orders one by one.")]
    public class CancelAll : TradeBot
    {
        [Parameter(DisplayName = "Target Instance Id", DefaultValue = "")]
        public string TargetInstanceId { get; set; }


        protected async override void OnStart()
        {
            var pendings = string.IsNullOrEmpty(TargetInstanceId)
                ? Account.Orders.Where(o => o.Type != OrderType.Position).ToList()
                : Account.Orders.Where(o => o.Type != OrderType.Position && o.InstanceId == TargetInstanceId).ToList();

            foreach (var order in pendings)
                await CancelOrderAsync(order.Id);

            Exit();
        }
    }
}
