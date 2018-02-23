using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TickTrader.Algo.Common.Model.Config;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Metadata;

namespace TickTrader.BotAgent.BA.Entities
{
    public class TradeBotInfo
    {
        public string Id { get; set; }
        //public bool Isolated { get; }
        //public bool IsRunning { get; }
        public string FaultMessage { get; set; }
        //public IBotLog Log { get; }
        //public IAlgoData AlgoData { get; }
        public AccountKey Account { get; set; }
        public TradeBotConfig Config { get; set; }
        //public PluginKey AlgoKey { get; }
        public AlgoPluginDescriptor Descriptor { get; set; }
        public string BotName { get; set; }
        public BotStates State { get; set; }
    }
}
