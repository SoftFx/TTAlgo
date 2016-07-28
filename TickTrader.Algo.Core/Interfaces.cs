using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.Core
{
    public interface IPluginSubscriptionHandler
    {
        void Subscribe(string smbCode, int depth);
        void Unsubscribe(string smbCode);
    }

    public interface IPluginSetupTarget
    {
        void SetParameter(string id, object value);
        void MapBarInput(string id, string symbolCode);
        void MapBarInput(string id, string symbolCode, Func<BarEntity, double> selector);
    }
}
