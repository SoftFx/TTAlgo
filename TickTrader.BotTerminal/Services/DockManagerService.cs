using System;
using System.IO;

namespace TickTrader.BotTerminal
{
    public class DockManagerService
    {
        public const string Tab_Symbols = "Tab_Symbols";
        public const string Tab_Bots = "Tab_Bots";
        public const string Tab_Algo = "Tab_Algo";
        public const string Tab_Trade = "Tab_Trade";
        public const string Tab_History = "Tab_History";
        public const string Tab_Journal = "Tab_Journal";
        public const string Tab_BotJournal = "Tab_BotJournal";


        public event Action<Stream> SaveLayoutEvent;
        public event Action<Stream> LoadLayoutEvent;
        public event Action<string> ShowHiddenTabEvent;


        public void SaveLayout(Stream stream)
        {
            SaveLayoutEvent?.Invoke(stream);
        }

        public void LoadLayout(Stream stream)
        {
            LoadLayoutEvent?.Invoke(stream);
        }

        public void ShowHiddenTab(string contentId)
        {
            ShowHiddenTabEvent?.Invoke(contentId);
        }
    }
}
