using TickTrader.Algo.Api;

namespace TickTrader.Algo.VersionTest
{
    [TradeBot(Category = "Test Plugin Setup", DisplayName = "Incompatible Bot", Version = "1.0",
        Description = "Should display a warning that newer version of API was used to build this bot")]
    public class IncompatibleBot : TradeBot
    {
    }
}
