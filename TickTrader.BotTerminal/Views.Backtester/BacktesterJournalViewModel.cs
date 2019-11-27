using Caliburn.Micro;
using Machinarium.Var;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TickTrader.Algo.Common.Lib;
using TickTrader.Algo.Common.Model;
using TickTrader.Algo.Core;

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

        public async Task SaveToFile(System.IO.Stream stream, IActionObserver observer)
        {
            var records = JournalRecords;

            long progress = 0;

            observer.SetMessage("Saving journal...");
            observer.StartProgress(0, records.Count);

            using (new UiUpdateTimer(() => observer.SetProgress(Interlocked.Read(ref progress))))
            {
                await Task.Run(() =>
                {
                    using (var writer = new System.IO.StreamWriter(stream))
                    {
                        for (int i = 0; i < records.Count; i++)
                        {
                            var record = records[i];
                            var sevString = TxtFormat(record.Severity);

                            writer.Write(record.Time.Timestamp.ToString(InvariantFormat.DateFormat));
                            writer.Write(" [{0}] ", sevString);

                            var nextLineSpaceSize = InvariantFormat.DateFormatFixedLength + 4 + sevString.Length;
                            var msgLines = SplitIntoLines(record.Message);
                            writer.WriteLine(msgLines[0]);
                            for (int j = 1; j < msgLines.Length; j++)
                            {
                                writer.Write(new string(' ', nextLineSpaceSize));
                                writer.WriteLine(msgLines[j]);
                            }

                            if (i % 10 == 0)
                                Interlocked.Exchange(ref progress, i);
                        }
                    }
                });
            }

            observer.StartProgress(0, records.Count);
        }

        private string TxtFormat(LogSeverities severity)
        {
            switch (severity)
            {
                case LogSeverities.TradeSuccess: return "Trade";
                default: return severity.ToString();
            }
        }

        private string[] SplitIntoLines(string message)
        {
            return message.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        }

    }
}
