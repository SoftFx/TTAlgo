using Machinarium.ActorModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using TickTrader.Algo.Core.Setup;

namespace TickTrader.Algo.Core.Realtime
{
    public interface IRealtimeMetadataProvider
    {
    }

    public interface IRealtimeFeedProvider
    {
        object SyncRoot { get; }
        IEnumerable<BarEntity> QueryBars(string symbolCode, DateTime from, DateTime to, Api.TimeFrames timeFrame);
        IEnumerable<QuoteEntity> QueryTicks(string symbolCode, DateTime from, DateTime to, Api.TimeFrames timeFrame);
        IEnumerable<Level2QuoteEntity> QueryLeve2(string symbolCode, DateTime from, DateTime to, Api.TimeFrames timeFrame);
        void Subscribe(ISymbolFeedSubscriber listener);
        void Unsubscribe(ISymbolFeedSubscriber listener);
    }

    public interface IRealtimeAccountDataProvider
    {
    }

    //public interface IRealtimeFeedListener
    //{
    //    string Symbol { get; }
    //    int Depth { get; }
    //}

    

    //public class RealtimeFeedFixture
    //{
    //    private Math.BarSampler sampler;
    //    private LinkedList<object> updateQueue = new LinkedList<object>();

    //    public RealtimeFeedFixture(Api.TimeFrames timeFrame)
    //    {
    //        this.sampler = Math.BarSampler.Get(timeFrame);
    //    }

    //    public void Update(QuoteEntity entity)
    //    {
    //        lock (updateQueue)
    //        {
    //            if (updateQueue.Count < 5)
    //                updateQueue.AddLast(entity);
    //            else
    //            {

    //            }
    //        }
    //    }
    //}


    public interface IRealtimeUpdate
    {
        void Apply();
    }

    public class RealtimeDataSource
    {

    }

    public class BarBasedDataSource
    {

    }

    public class RealtimePluginExecutor : MarshalByRefObject
    {
        private ActionBlock<IRealtimeUpdate> updateQueue;
        //private PluginExecutor executor;
        private Dictionary<string, IFeedFixture> feedFixtures = new Dictionary<string, IFeedFixture>();

        public RealtimePluginExecutor(PluginSetup setup)
        {
            this.Setup = setup;
            this.updateQueue = new ActionBlock<IRealtimeUpdate>(a => a.Apply());
        }

        protected PluginSetup Setup { get; private set; }
        protected IRealtimeFeedProvider Feed { get; private set; }

        protected interface IFeedFixture
        {
        }

        protected class BarFixture
        {
            private RealtimePluginExecutor executor;

            public BarFixture(RealtimePluginExecutor executor)
            {
                this.executor = executor;
            }
        }
    }

    public class IndicatorRealtimeBuilder : RealtimePluginExecutor
    {
        private IndicatorBuilder builder;

        public IndicatorRealtimeBuilder(PluginSetup setup) : base(setup)
        {
        }

        public void Start()
        {
            builder = Setup.CreateIndicatorBuilder();

            foreach (var buffer in builder.DataBuffers)
            {
                string symbolCode = buffer.Key;
            }
        }
    }
}
