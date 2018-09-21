using System.Collections.Generic;

namespace TickTrader.Algo.Common.Info
{
    public enum BotFolderId
    {
        AlgoData = 0,
        BotLogs = 1,
    }


    public class BotFolderInfo
    {
        public string BotId { get; set; }

        public BotFolderId FolderId { get; set; }

        public string Path { get; set; }

        public List<BotFileInfo> Files { get; set; }


        public BotFolderInfo()
        {
            Files = new List<BotFileInfo>();
        }
    }
}
