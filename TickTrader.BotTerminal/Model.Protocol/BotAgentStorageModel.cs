using Machinarium.Qnil;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace TickTrader.BotTerminal
{
    [DataContract(Namespace = "")]
    internal class BotAgentStorageModel : StorageModelBase<BotAgentStorageModel>
    {
        [DataMember(Name = "BotAgents")]
        private DynamicList<BotAgentStorageEntry> _botAgents;


        public DynamicList<BotAgentStorageEntry> BotAgents => _botAgents;


        public BotAgentStorageModel()
        {
            _botAgents = new DynamicList<BotAgentStorageEntry>();
        }


        public override BotAgentStorageModel Clone()
        {
            return new BotAgentStorageModel()
            {
                _botAgents = new DynamicList<BotAgentStorageEntry>(_botAgents.Values.Select(a => a.Clone())),
            };
        }


        public void Remove(BotAgentStorageEntry botAgent)
        {
            var index = _botAgents.Values.IndexOf(a => a.Login == botAgent.Login && a.ServerAddress == botAgent.ServerAddress);
            if (index != -1)
                _botAgents.RemoveAt(index);
        }

        public void Update(BotAgentStorageEntry botAgent)
        {
            int index = _botAgents.Values.IndexOf(a => a.Login == botAgent.Login && a.ServerAddress == botAgent.ServerAddress);
            if (index < 0)
                _botAgents.Values.Add(botAgent);
            else
            {
                if (_botAgents.Values[index].Password != botAgent.Password)
                    _botAgents.Values[index].Password = botAgent.Password;
            }
        }
    }
}
