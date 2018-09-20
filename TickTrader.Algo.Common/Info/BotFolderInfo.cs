using System.Collections.Generic;

namespace TickTrader.Algo.Common.Info
{
    public enum BotFolderType
    {
        AlgoData = 0,
        BotLogs = 1,
    }


    public class BotFolderInfo
    {
        public string Path { get; set; }

        public BotFolderType Type { get; set; }

        public List<BotFileInfo> Files { get; set; }


        public BotFolderInfo()
        {
            Files = new List<BotFileInfo>();
        }
    }
}
