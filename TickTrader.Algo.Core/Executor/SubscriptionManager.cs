using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Core.Executor
{
    internal class SubscriptionManager
    {
        private IPluginFeedProvider feed;
        //private Dictionary<string, List<

        public SubscriptionManager(IPluginFeedProvider feed)
        {
        }

        public void Add(IInternalFeedConsumer consumer)
        {
        }

        private int GetMaxDepth(List<IInternalFeedConsumer> subscribers)
        {
            int max = 1;

            foreach (var s in subscribers)
            {
                if (s.Depth == 0)
                    return 0;
                if (s.Depth > max)
                    max = s.Depth;
            }

            return max;
        }
    }
}
