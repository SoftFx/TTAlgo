using System.Linq;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.TestCollection.Bots
{
    [TradeBot(DisplayName = "[T] Cancel All Limits/Stops Script", Version = "1.1", Category = "Test Orders",
        SetupMainSymbol = false, Description = "Cancels all pending orders one by one.")]
    public class CancelAll : TradeBot
    {
        [Parameter(DisplayName = "Target Instance Id", DefaultValue = "")]
        public string TargetInstanceId { get; set; }

        [Parameter(DisplayName = "Batch size", DefaultValue = 50)]
        public int BatchSize { get; set; }

        [Parameter(DisplayName = "Run until stopped", DefaultValue = false)]
        public bool IsInfinite { get; set; }


        protected async override void OnStart()
        {
            while (!IsStopped)
            {
                var pendings = string.IsNullOrEmpty(TargetInstanceId)
                    ? Account.Orders.Where(o => o.Type != OrderType.Position)
                    : Account.Orders.Where(o => o.Type != OrderType.Position && o.InstanceId == TargetInstanceId);

                var cancelTasks = pendings.Take(BatchSize).Select(o => CancelOrderAsync(o.Id)).ToArray();

                if (cancelTasks.Length == 0 && IsInfinite)
                {
                    await Task.Delay(1000);
                    continue;
                }

                if (cancelTasks.Length == 0 && !IsInfinite)
                {
                    Exit();
                    return;
                }

                await Task.WhenAll(cancelTasks);
            }
        }
    }
}
