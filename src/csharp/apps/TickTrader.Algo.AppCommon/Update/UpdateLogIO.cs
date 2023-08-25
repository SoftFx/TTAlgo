using System;
using System.IO;

namespace TickTrader.Algo.AppCommon.Update
{
    public class UpdateLogIO
    {
        private const int MaxRetryCnt = 3;
        private const string DateTimeFormat = "yyyy-MM-dd HH:mm:ss.fff";

        private readonly string _path;


        public bool ThrowOnWriteError { get; set; } = false;


        public UpdateLogIO(string workDir)
        {
            _path = Path.Combine(workDir, UpdateHelper.LogFileName);
        }


        public void LogUpdateStatus(string msg)
        {
            var text = $"{GetCurrentTimeString()} | Status | {msg}";
            AppendMsg(new LogMsg(text, null));
        }

        public void LogUpdateInfo(string msg)
        {
            var text = $"{GetCurrentTimeString()} | Info | {msg}";
            AppendMsg(new LogMsg(text, null));
        }

        public void LogUpdateError(string msg, Exception error)
        {
            var text = $"{GetCurrentTimeString()} | Error | {msg}";
            AppendMsg(new LogMsg(text, error));
        }

        public static string TryReadLogOnce(string workDir)
        {
            var path = Path.Combine(workDir, UpdateHelper.LogFileName);
            try
            {
                return ReadLogInternal(path);
            }
            catch { }

            return null;
        }


        public static string TryReadLog(string workDir)
        {
            var path = Path.Combine(workDir, UpdateHelper.LogFileName);
            for (var i = 0; i < MaxRetryCnt; i++)
            {
                try
                {
                    return ReadLogInternal(path);
                }
                catch { }
            }

            return string.Empty;
        }

        private static string GetCurrentTimeString() => DateTime.UtcNow.ToString(DateTimeFormat);


        private void AppendMsg(LogMsg msg)
        {
            for (var i = 0; i < MaxRetryCnt; i++)
            {
                try
                {
                    using (var file = File.Open(_path, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
                    using (var writer = new StreamWriter(file))
                    {
                        if (!string.IsNullOrEmpty(msg.Text))
                            writer.WriteLine(msg.Text);
                        if (msg.Error != null)
                            writer.WriteLine(msg.Error.ToString());
                    }

                    return;
                }
                catch
                {
                    if (ThrowOnWriteError && i == MaxRetryCnt - 1)
                        throw;
                }
            }
        }

        private static string ReadLogInternal(string path)
        {
            if (!File.Exists(path))
                return string.Empty;

            using (var file = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var reader = new StreamReader(file))
            {
                return reader.ReadToEnd();
            }
        }


        private readonly ref struct LogMsg
        {
            public string Text { get; }

            public Exception Error { get; }


            public LogMsg(string text, Exception error)
            {
                Text = text;
                Error = error;
            }
        }
    }
}
