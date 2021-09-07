using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Api
{
    public interface EnvironmentInfo
    {
        string DataFolder { get; }
        /// <summary>
        /// Bot data folder path, equals <code>DataFolder</code> for indicators.
        /// </summary>
        string BotDataFolder { get; }
    }
}
