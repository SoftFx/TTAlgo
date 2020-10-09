using System;
using System.Collections.Generic;
using TickTrader.Algo.Core.Repository;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Core
{
    [Serializable]
    public class PluginExecutorConfig : IPluginSetupTarget
    {
        public string MainSymbolCode { get; set; }
        public Feed.Types.Timeframe TimeFrame { get; set; }
        public Feed.Types.Timeframe ModelTimeFrame { get; set; }
        public string WorkingFolder { get; set; }
        public string BotWorkingFolder { get; set; }
        public string InstanceId { get; set; }
        public PluginPermissions Permissions { get; set; }
        public InvokeStartegy InvokeStrategy { get; set; }
        public bool IsLoggingEnabled { get; set; }

        internal FeedBufferStrategy BufferStartegy { get; private set; }
        internal FeedStrategy FeedStrategy { get; private set; }
        internal Dictionary<string, object> PluginParams { get; } = new Dictionary<string, object>();
        internal Dictionary<string, IOutputFixtureFactory> Outputs { get; } = new Dictionary<string, IOutputFixtureFactory>();
        internal Dictionary<Tuple<string, string>, Mapping> Mappings { get; } = new Dictionary<Tuple<string, string>, Mapping>();

        public void SetParameter(string id, object value)
        {
            PluginParams[id] = value;
        }

        public T GetFeedStrategy<T>()
           where T : FeedStrategy
        {
            return (T)FeedStrategy;
        }

        public void MapInput(string inputName, string symbolCode, Mapping mapping)
        {
            // hook to appear in plugin domain
            //mapping?.MapInput(this, inputName, symbolCode);
            Mappings[new Tuple<string, string>(inputName, symbolCode)] = mapping;
        }

        public BarStrategy InitBarStrategy(Feed.Types.MarketSide marketSide)
        {
            var strategy = new BarStrategy(marketSide);
            FeedStrategy = strategy;
            return strategy;
        }

        public QuoteStrategy InitQuoteStrategy()
        {
            var strategy = new QuoteStrategy();
            FeedStrategy = strategy;
            return strategy;
        }

        public void InitSlidingBuffering(int size)
        {
            BufferStartegy = new SlidingBufferStrategy(size);
        }

        public void InitTimeSpanBuffering(DateTime from, DateTime to)
        {
            BufferStartegy = new TimeSpanStrategy(from, to);
        }

        public void SetupOutput<T>(string outputSeriesId)
        {
            Outputs[outputSeriesId] = new OutputFixtureFactory<T>(outputSeriesId);
        }


        internal void LoadFrom(RuntimeConfig config, PluginConfig pluginConfig)
        {
            if (config.FeedStrategyConfig.TryUnpack<BarStratefyConfig>(out var barStrategyConfig))
                InitBarStrategy(barStrategyConfig.MarketSide);
            else if (config.FeedStrategyConfig.TryUnpack<QuoteStrategyConfig>(out var quoteStrategyConfig))
                InitQuoteStrategy();

            if (config.BufferStrategyConfig.TryUnpack<SlidingBufferStrategyConfig>(out var slidingbufferConfig))
                InitSlidingBuffering(slidingbufferConfig.Size);
            else if (config.BufferStrategyConfig.TryUnpack<TimeSpanStrategyConfig>(out var timeSpanConfig))
                InitTimeSpanBuffering(timeSpanConfig.From.ToDateTime(), timeSpanConfig.To.ToDateTime());

            if (config.InvokeStrategyConfig.TryUnpack<PriorityInvokeStrategyConfig>(out var priorityStrategyConfig))
                InvokeStrategy = new PriorityInvokeStartegy();

            IsLoggingEnabled = config.IsLoggingEnabled;
            WorkingFolder = config.WorkingDirectory;
            BotWorkingFolder = config.WorkingDirectory;

            InstanceId = pluginConfig.InstanceId;
            MainSymbolCode = pluginConfig.MainSymbol.Name;
            TimeFrame = pluginConfig.Timeframe;
            Permissions = pluginConfig.Permissions;
        }
    }
}
