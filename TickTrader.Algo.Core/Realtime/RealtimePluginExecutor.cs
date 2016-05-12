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
    public interface IPluginSetup
    {
        PluginBuilder CreateBuilder();
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

    public class RealtimePluginExecutor : MarshalByRefObject
    {
        //private static readonly NullFeedProvider nullFeed = new NullFeedProvider();

        //internal object lockObj = new object();
        //private ActionBlock<IRealtimeUpdate> updateScheduler;
        //private BufferBlock<IRealtimeUpdate> updateQueue;
        //private IRealtimeFeedProvider feed = nullFeed;
        ////private PluginExecutor executor;
        //private Dictionary<string, IFeedFixture> feedFixtures = new Dictionary<string, IFeedFixture>();
        //private Task initTask;
        //private bool initForbidden;

        //public RealtimePluginExecutor(IPluginSetup setup)
        //{
        //    this.Setup = setup;

        //    this.updateQueue = new BufferBlock<IRealtimeUpdate>();
        //    this.updateScheduler = new ActionBlock<IRealtimeUpdate>(a => a.Apply());
        //    //this.updateQueue = new ActionBlock<IRealtimeUpdate>(a => a.Apply());
        //}

        //internal Api.TimeFrames Timeframe { get; private set; }
        //internal IPluginSetup Setup { get; private set; }
        //internal SubsciptionProxy FeedUpdateProxy { get; private set; }
        //internal PluginBuilder Builder { get; private set; }

        //public IRealtimeFeedProvider Feed
        //{
        //    get { return feed; }
        //    set
        //    {
        //        if (value == null)
        //            throw new InvalidOperationException("Feed cannot be null!");

        //        if (initForbidden)
        //            throw new InvalidOperationException("Feed cannot be changed after start!");

        //        this.feed = value;
        //    }
        //}

        //public string MainSymbolCode { get; set; }
        //public TimePeriod BasePeriod { get; set; }
        //public Api.TimeFrames TimeFrame { get; set; } 

        //public void Start()
        //{
        //    if (string.IsNullOrEmpty(MainSymbolCode))
        //        throw new InvalidOperationException("Main symbol is not initialized!");

        //    lock (lockObj)
        //    {
        //        initForbidden = true;

        //        if (initTask != null)
        //            throw new InvalidOperationException("Cannot start: Already started!");

        //        initTask = Task.Factory.StartNew(InitAndBuild);
        //    }
        //}

        //public async Task StopAsync()
        //{
        //    lock (lockObj)
        //    {
        //        if (initTask == null)
        //            throw new InvalidOperationException("Cannot stop: Executor is not started!");
        //    }

        //    await initTask;

        //    updateQueue.Complete();
        //    updateScheduler.Complete();
        //    await updateQueue.Completion;
        //    await updateScheduler.Completion;

        //    lock (lockObj) initTask = null;
        //}

        //private void InitAndBuild()
        //{
        //    FeedUpdateProxy = new SubsciptionProxy(feed, updateQueue);

        //    // Setup builder
        //    Builder = Setup.CreateBuilder();
        //    Builder.MainSymbol = MainSymbolCode;

        //    // Create main feed buffer
        //    var timeSyncFixture = GetOrCreateFeedFixture(MainSymbolCode);

        //    // Created configured feed buffers
        //    foreach (var buffer in Builder.DataBuffers)
        //        GetOrCreateFeedFixture(buffer.Key);

        //    // Build indicators for existing data
        //    int toBuild = timeSyncFixture.Count;
        //    Builder.BuildNext(toBuild);

        //    // start processing updates
        //    updateQueue.LinkTo(updateScheduler);
        //}

        //private IFeedFixture GetOrCreateFeedFixture(string smbCode)
        //{
        //    IFeedFixture fixture;
        //    if (!feedFixtures.TryGetValue(smbCode, out fixture))
        //    {
        //        fixture = new BarSeriesFixture(smbCode, this);
        //        fixture.Init();
        //        feedFixtures.Add(smbCode, fixture);
        //    }

        //    return fixture;
        //}
    }

    //internal interface IFeedFixture
    //{
    //    string SymbolCode { get; }
    //    int Count { get; }
    //    void Init();
    //    DateTime GetRefTimestamp(int index);
    //}

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
