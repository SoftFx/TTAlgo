using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.Core
{
    [Serializable]
    public class PluginExecutorConfig
    {
        public string MainSymbolCode { get; set; }
        public TimeFrames TimeFrame { get; set; }
        public ITradeExecutor TradeExecutor { get; set; }
        public string WorkingFolder { get; set; }
        public string BotWorkingFolder { get; set; }
        public string InstanceId { get; set; }
        public PluginPermissions Permissions { get; set; }
    }
}
