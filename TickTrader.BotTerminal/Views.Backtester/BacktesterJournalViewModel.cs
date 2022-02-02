using System.Collections.ObjectModel;
using TickTrader.Algo.Domain;

namespace TickTrader.BotTerminal
{
    internal class BacktesterJournalViewModel : Page
    {
        //private Property<ObservableCollection<BotLogRecord>> _journalContent = new Property<ObservableCollection<BotLogRecord>>();

        public BacktesterJournalViewModel()
        {
            DisplayName = "Journal";
        }

        public ObservableCollection<PluginLogRecord> JournalRecords { get; } = new ObservableCollection<PluginLogRecord>();

        //public void SetData(List<BotLogRecord> records)
        //{
        //    _journalContent.Value = records;
        //}

        public void Append(PluginLogRecord record)
        {
            JournalRecords.Add(record);
        }

        public void Clear()
        {
            JournalRecords.Clear();
        }
    }
}
