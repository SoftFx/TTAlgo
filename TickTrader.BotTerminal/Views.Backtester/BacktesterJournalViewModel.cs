using System.Collections.ObjectModel;
using System.Threading.Tasks;
using TickTrader.Algo.BacktesterApi;
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

        public ObservableCollection<JournalMsg> JournalRecords { get; private set; } = new ObservableCollection<JournalMsg>();

        //public void SetData(List<BotLogRecord> records)
        //{
        //    _journalContent.Value = records;
        //}

        public void Append(PluginLogRecord record)
        {
            JournalRecords.Add(new JournalMsg(record));
        }

        public void Clear()
        {
            JournalRecords.Clear();
        }

        public async Task LoadJournal(BacktesterResults results)
        {
            var records = new ObservableCollection<JournalMsg>();
            await Task.Run(() =>
            {
                foreach(var record in results.Journal)
                {
                    records.Add(new JournalMsg(record));
                }
            });

            JournalRecords = records;
            NotifyOfPropertyChange(nameof(JournalRecords));
        }


        public sealed class JournalMsg : BaseJournalMessage
        {
            private readonly PluginLogRecord _record;


            public PluginLogRecord.Types.LogSeverity Severity => _record.Severity;


            public JournalMsg(PluginLogRecord record) : base(record.TimeUtc)
            {
                _record = record;
                Message = record.Message;
            }
        }
    }
}
