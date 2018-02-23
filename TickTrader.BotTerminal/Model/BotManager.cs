using Machinarium.Qnil;
using System;
using System.Collections.Generic;
using System.Linq;
using TickTrader.Algo.Common.Model.Setup;

namespace TickTrader.BotTerminal
{
    internal class BotManager
    {
        private AlgoEnvironment _algoEnv;
        private DynamicDictionary<string, TradeBotModel> _bots;


        public IDynamicDictionarySource<string, TradeBotModel> Bots => _bots;


        public BotManager(AlgoEnvironment algoEnv)
        {
            _algoEnv = algoEnv;
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

        public List<TradeBotStorageEntry> GetSnapshot()
        {
            return _bots.Values.Select(b => new TradeBotStorageEntry
            {
                DescriptorId = b.Setup.Descriptor.Id,
                PluginFilePath = b.PluginFilePath,
                InstanceId = b.InstanceId,
                Isolated = b.Isolated,
                Started = b.State == BotModelStates.Running,
                Config = b.Setup.Save(),
                Permissions = b.Permissions,
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

        private void AddBot(PluginSetupModel setup)
        {
            var botModel = new TradeBotModel(null, null, null);
            _bots.Add(botModel.InstanceId, botModel);
            _algoEnv.IdProvider.AddPlugin(botModel);
        }

        private void RemoveBot(string instanceId)
        {
            if (_bots.TryGetValue(instanceId, out var botModel))
            {
                _bots.Remove(instanceId);
                _algoEnv.IdProvider.RemovePlugin(instanceId);
            }
        }
    }
}
