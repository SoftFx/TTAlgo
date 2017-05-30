using System.IO;
using TickTrader.DedicatedServer.Extensions;

namespace TickTrader.DedicatedServer.DS.Models
{
    public class WorkingDirectoryNamingStrategy : IDirectoryNamingStrategy
    {
        private string _botId;

        public WorkingDirectoryNamingStrategy(string botId)
        {
            _botId = botId;
        }

        public string GetFullPath()
        {
            return Path.Combine(ServerModel.Environment.AlgoWorkingFolder, _botId.Escape());
        }
    }
}
