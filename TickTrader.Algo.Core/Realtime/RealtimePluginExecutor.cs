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

    public class RealtimePluginHost
    {
        private List<RealtimePluginExecutor> plugins = new List<RealtimePluginExecutor>();

        internal void Subscribe()
        {
        }
    }

    internal interface ISymbolFeedSubscriber
    {
        string Symbol { get; }
        int RequiredDepth { get; }
        void OnUpdate(QuoteEntity quote);
    }

    public class RealtimeFeedFixture
    {
        private Math.BarSampler sampler;
        private LinkedList<object> updateQueue = new LinkedList<object>();

        public RealtimeFeedFixture(Api.TimeFrames timeFrame)
        {
            this.sampler = Math.BarSampler.Get(timeFrame);
        }

        public void Update(QuoteEntity entity)
        {
            lock (updateQueue)
            {
                if (updateQueue.Count < 5)
                    updateQueue.AddLast(entity);
                else
                {

                }
            }
        }
    }

    public class RealtimePluginExecutor : MarshalByRefObject
    {
        private PluginSetup setup;

        public RealtimePluginExecutor(PluginSetup setup)
        {
            this.setup = setup;
        }

        public void Start()
        {
        }
    }
}
