using Machinarium.Qnil;
using NLog;
using System;
using System.Linq;
using TickTrader.Algo.Api;
using TickTrader.Algo.Common.Info;
using TickTrader.Algo.Common.Model.Config;
using TickTrader.Algo.Common.Model.Library;
using TickTrader.Algo.Common.Model.Setup;

namespace TickTrader.BotTerminal
{
    internal class BotManager : IAlgoSetupContext
    {
        private static ILogger _logger = LogManager.GetCurrentClassLogger();


        private VarDictionary<string, TradeBotModel> _bots;


        public LocalAlgoAgent Agent { get; }

        public IVarSet<string, TradeBotModel> Bots => _bots;

        public int RunningBotsCnt => _bots.Snapshot.Values.Count(b => b.IsRunning);

        public bool HasRunningBots => _bots.Snapshot.Values.Any(b => b.IsRunning);


        public event Action<TradeBotModel> StateChanged = delegate { };


        public BotManager(LocalAlgoAgent agent)
        {
            Agent = agent;
            _bots = new VarDictionary<string, TradeBotModel>();
        }


        public void StopBots()
        {
            _bots.Values.Foreach(StopBot);
        }

        public void AddBot(TradeBotModel botModel)
        {
            _bots.Add(botModel.InstanceId, botModel);
            Agent.IdProvider.RegisterBot(botModel);
            botModel.StateChanged += StateChanged;
        }

        public void ChangeBotConfig(string instanceId, PluginConfig config)
        {
            if (_bots.TryGetValue(instanceId, out var bot))
            {
                bot.Configurate(config);
                _bots[instanceId] = bot;
            }
        }

        public void RemoveBot(string instanceId)
        {
            if (_bots.TryGetValue(instanceId, out var botModel))
            {
                botModel.Remove();
                _bots.Remove(instanceId);
                Agent.IdProvider.UnregisterPlugin(instanceId);
                botModel.StateChanged -= StateChanged;
            }
        }


        private async void StopBot(TradeBotModel bot)
        {
            if (bot.State == BotModelStates.Running)
            {
                await bot.Stop();
            }
        }


        #region IAlgoSetupContext

        TimeFrames IAlgoSetupContext.DefaultTimeFrame => TimeFrames.M1;

        string IAlgoSetupContext.DefaultSymbolCode => "EURUSD";

        MappingKey IAlgoSetupContext.DefaultMapping => new MappingKey(MappingCollection.DefaultFullBarToBarReduction);

        #endregion
    }
}
