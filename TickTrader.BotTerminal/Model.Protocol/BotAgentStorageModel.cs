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
                _botAgents = new DynamicList<BotAgentStorageEntry>(_botAgents.Values.Select(b => b.Clone())),
            };
        }


        public void Remove(string server)
        {
            var index = _botAgents.Values.IndexOf(b => b.ServerAddress == server);
            if (index != -1)
            {
                _botAgents.RemoveAt(index);
            }
        }

        public BotAgentStorageEntry Update(string login, string password, string server, int port, string certName)
        {
            var index = _botAgents.Values.IndexOf(b => b.ServerAddress == server);
            if (index == -1)
            {
                _botAgents.Add(new BotAgentStorageEntry { ServerAddress = server });
                index = _botAgents.Count - 1;
            }
            _botAgents[index].Login = login;
            _botAgents[index].Password = password;
            _botAgents[index].Port = port;
            _botAgents[index].CertificateName = certName;

            return _botAgents[index];
        }

        public void UpdateConnect(string server, bool connect)
        {
            var index = _botAgents.Values.IndexOf(b => b.ServerAddress == server);
            if (index != -1)
            {
                _botAgents[index].Connect = connect;
            }
        }
    }
}
