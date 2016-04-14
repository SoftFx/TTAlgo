using Machinarium.ActorModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Setup;

namespace TickTrader.Algo.Core.Realtime
{
    public interface IRealtimeMetadataProvider
    {
    }

    public interface IRealtimeFeedProvider
    {
        IEnumerable<BarEntity> QueryBars(string symbolCode, DateTime from, DateTime to, Api.TimeFrames timeFrame);
        IEnumerable<QuoteEntity> QueryTicks(string symbolCode, DateTime from, DateTime to, Api.TimeFrames timeFrame);
        IEnumerable<Level2QuoteEntity> QueryLeve2(string symbolCode, DateTime from, DateTime to, Api.TimeFrames timeFrame);
        void Subscribe(IRealtimeFeedListener listener);
        void Unsubscribe(IRealtimeFeedListener listener);
    }

    public interface IRealtimeAccountDataProvider
    {
    }

    public interface IRealtimeFeedListener
    {
        string Symbol { get; }
        int Depth { get; }
    }

    public class RealtimePluginExecutor
    {
        private PluginSetup setup;

        public RealtimePluginExecutor(PluginSetup setup)
        {
        }

        private class Agent : MarshalByRefObject
        {
            private ActorCore actor = new ActorCore();
        }
    }
}
