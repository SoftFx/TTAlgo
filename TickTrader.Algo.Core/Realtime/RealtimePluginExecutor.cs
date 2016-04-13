using Machinarium.ActorModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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


    [Serializable]
    public class RealtimePluginSetup
    {
        public RealtimePluginSetup()
        {
        }

        public Dictionary<string, object> Parameters { get; private set; }
        public Dictionary<string, InputMapping> Inputs { get; private set; }

        
    }

    public interface InputMapping
    {
        string SymbolCode { get;}
        void Apply(PluginExecutor executor);
    }

    [Serializable]
    public class BarToDouble
    {
    }

    [Serializable]
    public class TickInputSetup
    {
    }

    public class RealtimePluginExecutor
    {
        

        private class Agent : MarshalByRefObject
        {
            private ActorCore actor = new ActorCore();
        }
    }
}
