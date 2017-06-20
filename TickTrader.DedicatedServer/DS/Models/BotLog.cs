using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Lib;

namespace TickTrader.DedicatedServer.DS.Models
{
    public class BotLog : IBotLog
    {
        private object _sync;

        public BotLog(object sync)
        {
            _sync = sync;
        }

        public string Status { get; private set; }

        public event Action<string> StatusUpdated;

        internal void Update(BotLogRecord[] recrods)
        {
            lock (_sync)
            {
                foreach (var rec in recrods)
                {
                    if (rec.Severity == LogSeverities.CustomStatus)
                    {
                        Status = rec.Message;
                        StatusUpdated?.Invoke(rec.Message);
                    }
                }
            }
        }

        public void UpdateStatus(string status)
        {
            lock (_sync)
            {
                Status = status;
                StatusUpdated?.Invoke(status);
            }
        }
    }
}
