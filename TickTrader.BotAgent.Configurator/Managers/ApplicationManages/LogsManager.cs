using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TickTrader.BotAgent.Configurator
{
    public class LogsManager
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private const long BlockSize = 2048L;
        private const int MaxMessagesCount = 1000;

        private readonly string[] _separators = new string[] { Environment.NewLine };

        private long _lastSize;
        private LinkedList<string> _messages;

        public string LogsFilePath { get; }

        public LogsManager(string path, string folder)
        {
            _messages = new LinkedList<string>();
            LogsFilePath = Path.Combine(path, folder);

            LoadLog();
        }

        public string LogsStr => string.Join("", _messages);

        public void LoadLog()
        {
            if (!File.Exists(LogsFilePath))
                return;

            try
            {
                long fileSize = new FileInfo(LogsFilePath).Length;
                long shift = 0;

                bool finish = false;

                _lastSize = fileSize;

                using (var fs = new FileStream(LogsFilePath, FileMode.Open))
                {
                    while (fileSize > shift)
                    {
                        long sf = Math.Min(BlockSize, fileSize - shift);

                        shift += sf;

                        byte[] output = new byte[sf];

                        fs.Seek(-shift, SeekOrigin.End);

                        fs.Read(output, 0, (int)sf);

                        string block = Encoding.Default.GetString(output);

                        var records = block.Split(_separators, StringSplitOptions.RemoveEmptyEntries).Reverse().ToList();

                        if (block.Last() != '\n')
                        {
                            _messages.Last.Value = $"{records.First()}{_messages.Last.Value}";
                            records.RemoveAt(0);
                        }

                        for (int i = 0; i < records.Count; ++i)
                        {
                            if (_messages.Count == MaxMessagesCount)
                            {
                                finish = true;
                                break;
                            }

                            if (!records[i].EndsWith(Environment.NewLine))
                                records[i] = $"{records[i]}{Environment.NewLine}";

                            _messages.AddLast(records[i]);
                        }

                        if (finish)
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
        }

        public void UpdateLog()
        {
            if (!File.Exists(LogsFilePath))
                return;

            try
            {
                long fileSize = new FileInfo(LogsFilePath).Length;

                if (_lastSize > fileSize) //to update the file at 12 at night
                    _lastSize = 0;

                long uk = fileSize - _lastSize;
                bool gap = false;

                if (_lastSize != fileSize)
                {
                    using (var fs = new FileStream(LogsFilePath, FileMode.Open))
                    {
                        while (uk > 0)
                        {
                            long sf = Math.Min(BlockSize, uk);

                            byte[] output = new byte[sf];

                            fs.Seek(-uk, SeekOrigin.End);

                            uk -= sf;

                            fs.Read(output, 0, (int)sf);

                            string block = Encoding.Default.GetString(output);

                            var records = block.Split(_separators, StringSplitOptions.RemoveEmptyEntries).ToList();

                            if (gap)
                            {
                                _messages.First.Value = $"{_messages.First.Value}{records.First()}{Environment.NewLine}";
                                records.RemoveAt(0);
                            }

                            gap = block.Last() != '\n';

                            for (int i = 0; i < records.Count; ++i)
                            {
                                if (_messages.Count >= MaxMessagesCount)
                                    _messages.RemoveLast();

                                _messages.AddFirst(i == records.Count - 1 && gap ? records[i] : $"{records[i]}{Environment.NewLine}");
                            }
                        }
                    }

                    _lastSize = fileSize;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
        }
    }
}
