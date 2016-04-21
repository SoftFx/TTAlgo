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

    [Serializable]
    public struct TimePeriod
    {
        public TimePeriod(DateTime start, DateTime end)
            : this()
        {
            this.Start = start;
            this.End = end;
        }

        public TimePeriod(DateTime start, TimeSpan duration)
            : this()
        {
            this.Start = start;
            this.End = start + duration;
        }

        public DateTime Start { get; private set; }
        public TimeSpan Duration { get { return End - Start; } }
        public DateTime End { get; private set; }
    }

    public abstract class RealtimePluginExecutor : MarshalByRefObject
    {
        internal object lockObj = new object();
        private ActionBlock<IRealtimeUpdate> updateScheduler;
        private BufferBlock<IRealtimeUpdate> updateQueue;
        //private PluginExecutor executor;
        private Dictionary<string, IFeedFixture> feedFixtures = new Dictionary<string, IFeedFixture>();
        private Task initTask;

        public RealtimePluginExecutor(PluginSetup setup, TimePeriod basePeriod, Api.TimeFrames timeFrame)
        {
            this.Setup = setup;
            this.BasePeriod = BasePeriod;
            this.updateQueue = new BufferBlock<IRealtimeUpdate>();
            //this.updateQueue = new ActionBlock<IRealtimeUpdate>(a => a.Apply());
        }

        internal Api.TimeFrames Timeframe { get; private set; }
        internal TimePeriod BasePeriod { get; private set; }
        internal PluginSetup Setup { get; private set; }
        internal IRealtimeFeedProvider Feed { get; private set; }
        internal SubsciptionProxy FeedUpdateProxy { get; private set; }
        internal abstract PluginExecutor Builder { get; }

        public void Start()
        {
            lock(lockObj)
            {
                if (initTask != null)
                    throw new InvalidOperationException("Cannot start: Already started!");
                initTask = Task.Factory.StartNew(Init);
            }
        }

        public async Task StopAsync()
        {
            lock (lockObj)
            {
                if (initTask == null)
                    throw new InvalidOperationException("Cannot stop: Executor is not started!");
            }

            await initTask;

            updateQueue.Complete();
            updateScheduler.Complete();
            await updateQueue.Completion;
            await updateScheduler.Completion;

            lock (lockObj) initTask = null;
        }

        private void Init()
        {
            
        }

        internal interface IFeedFixture
        {
            string SymbolCode { get; }
        }

        internal class BarFixture : IPluginSubscriber, IFeedFixture
        {
            private RealtimePluginExecutor executor;

            public BarFixture(string symbolCode, RealtimePluginExecutor executor)
            {
                this.SymbolCode = symbolCode;
                this.executor = executor;
            }

            public int Depth { get { return 1; } }
            public string SymbolCode { get; private set; }

            public void Init()
            {
                lock (executor.Feed.SyncRoot)
                {   
                    var data = executor.Feed.QueryBars(SymbolCode, executor.BasePeriod.Start, executor.BasePeriod.End, executor.Timeframe);
                    executor.Builder.GetBarBuffer(SymbolCode).Append(data);
                    executor.FeedUpdateProxy.Add(this);
                }

                foreach (var buffer in executor.Builder.DataBuffers)
                {
                }
            }

            //private IFeedFixture GetFixture(string symbolCode)
            //{

            //}

            public void Stop()
            {
            }

            public void OnUpdate(QuoteEntity quote)
            {
            }
        }
    }

    //public class IndicatorRealtimeBuilder : RealtimePluginExecutor
    //{
    //    private IndicatorBuilder builder;

    //    public IndicatorRealtimeBuilder(PluginSetup setup) : base(setup)
    //    {
    //    }

    //    public void Start()
    //    {
    //        builder = Setup.CreateIndicatorBuilder();

    //        foreach (var buffer in builder.DataBuffers)
    //        {
    //            string symbolCode = buffer.Key;
    //        }
    //    }
    //}
}
