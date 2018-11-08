using System.Linq;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.TestCollection.Bots
{
    [TradeBot(DisplayName = "[T] Close All Positions Script", Version = "1.1", Category = "Test Orders",
        SetupMainSymbol = false, Description = "Closes all positions for gross accounts")]
    public class CloseAll : TradeBot
    {
        [Parameter(DisplayName = "Target Instance Id", DefaultValue = "")]
        public string TargetInstanceId { get; set; }


        protected async override void OnStart()
        {
            if (Account.Type == AccountTypes.Gross)
            {
                var positions = string.IsNullOrEmpty(TargetInstanceId)
                ? Account.Orders.Where(o => o.Type == OrderType.Position).ToList()
                : Account.Orders.Where(o => o.Type == OrderType.Position && o.InstanceId == TargetInstanceId).ToList();

                foreach (var pos in positions)
                    await CloseOrderAsync(pos.Id);
            }
            else if (Account.Type == AccountTypes.Net)
            {
                // TO DO : open opposite positions to close existing
            }
            else
                Print("This script works only for Net or Gross accounts!");

            Exit();
        }
    }
}
