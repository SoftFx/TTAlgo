using Machinarium.Qnil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TickTrader.Algo.Api;
using TickTrader.Algo.Common.Model.Setup;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Metadata;

namespace TickTrader.BotTerminal
{
    internal class BotManager : IAlgoSetupFactory
    {
        private DynamicDictionary<string, TradeBotModel> _bots;


        public AlgoEnvironment AlgoEnv { get; }

        public IDynamicDictionarySource<string, TradeBotModel> Bots => _bots;

        public int RunningBotsCnt => Bots.Snapshot.Values.Count(b => b.IsRunning);

        public bool HasRunningBots => Bots.Snapshot.Values.Any(b => b.IsRunning);


        public event Action<TradeBotModel> StateChanged = delegate { };


        public BotManager(AlgoEnvironment algoEnv)
        {
            AlgoEnv = algoEnv;
            _bots = new DynamicDictionary<string, TradeBotModel>();
        }


        public void StopBots()
        {
            _bots.Values.Foreach(StopBot);
        }

        public void ClearBots()
        {
            _bots.Clear();
        }

        public void AddBot(TradeBotModel botModel)
        {
            _bots.Add(botModel.InstanceId, botModel);
            AlgoEnv.IdProvider.AddPlugin(botModel);
            botModel.StateChanged += StateChanged;
        }

        public void RemoveBot(string instanceId)
        {
            if (_bots.TryGetValue(instanceId, out var botModel))
            {
                _bots.Remove(instanceId);
                AlgoEnv.IdProvider.RemovePlugin(instanceId);
                botModel.StateChanged -= StateChanged;
            }
        }

        public List<TradeBotStorageEntry> GetSnapshot()
        {
            return _bots.Values.Select(b => new TradeBotStorageEntry
            {
                DescriptorId = b.Setup.Descriptor.Id,
                PluginFilePath = b.PluginFilePath,
                InstanceId = b.InstanceId,
                Started = b.State == BotModelStates.Running,
                Config = b.Setup.Save(),
                StateViewOpened = b.StateViewOpened,
                StateSettings = b.StateViewSettings.StorageModel,
            }).ToList();
        }

        public void RestoreFromSnapshot(List<TradeBotStorageEntry> snapshot)
        {

        }


        private async void StopBot(TradeBotModel bot)
        {
            if (bot.State == BotModelStates.Running)
            {
                await bot.Stop();
            }
        }


        #region IAlgoSetupFactory

        PluginSetupModel IAlgoSetupFactory.CreateSetup(AlgoPluginRef catalogItem)
        {
            switch (catalogItem.Descriptor.AlgoLogicType)
            {
                case AlgoTypes.Robot: return new TradeBotSetupModel(catalogItem, AlgoEnv, "EURUSD", TimeFrames.M1, "Bid");
                default: throw new ArgumentException("Unknown plugin type");
            }
        }

        #endregion
    }
}
