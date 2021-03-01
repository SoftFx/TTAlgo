using Machinarium.Qnil;
using NLog;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace TickTrader.BotTerminal
{
    internal class BotAgentManager
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();


        private BotAgentStorageModel _botAgentStorage;


        public ObservableCollection<BotAgentServerEntry> PredefinedServers { get; set; }

        public VarDictionary<string, BotAgentConnectionManager> BotAgents { get; set; }


        public BotAgentManager(PersistModel appStorage)
        {
            _botAgentStorage = appStorage.BotAgentStorage;

            PredefinedServers = new ObservableCollection<BotAgentServerEntry>();
            var cfgSection = ProtocolConfigSection.GetCfgSection();
            foreach (ProtocolServerElement server in cfgSection.Servers)
            {
                PredefinedServers.Add(new BotAgentServerEntry(server));
            }

            BotAgents = new VarDictionary<string, BotAgentConnectionManager>();
            foreach (var agent in _botAgentStorage.BotAgents.Values)
            {
                if (string.IsNullOrEmpty(agent.Name))
                {
                    agent.PatchName();
                }
                BotAgents.Add(agent.Name, new BotAgentConnectionManager(agent));
            }
        }


        public async Task<string> Connect(string agentName, string login, string password, string server, int port)
        {
            var creds = _botAgentStorage.Update(agentName, login, password, server, port);
            _botAgentStorage.Save();

            if (!BotAgents.TryGetValue(agentName, out var connection))
            {
                connection = new BotAgentConnectionManager(creds);
                BotAgents.Add(agentName, connection);
            }
            await connection.WaitConnect();

            if (string.IsNullOrWhiteSpace(connection.LastError))
            {
                _botAgentStorage.UpdateConnect(agentName, true);
                _botAgentStorage.Save();
            }

            return connection.LastError;
        }

        public void Connect(string agentName)
        {
            try
            {
                if (BotAgents.TryGetValue(agentName, out var connection))
                {
                    _botAgentStorage.UpdateConnect(agentName, true);
                    _botAgentStorage.Save();
                    connection.Connect();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"AlgoServer Connect failed: {ex.Message}");
            }
        }

        public void Disconnect(string agentName)
        {
            try
            {
                if (BotAgents.TryGetValue(agentName, out var connection))
                {
                    _botAgentStorage.UpdateConnect(agentName, false);
                    _botAgentStorage.Save();
                    connection.Disconnect();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"AlgoServer Disconnect failed: {ex.Message}");
            }
        }

        public void Remove(string agentName)
        {
            try
            {
                if (BotAgents.TryGetValue(agentName, out var connection))
                {
                    _botAgentStorage.Remove(agentName);
                    _botAgentStorage.Save();
                    BotAgents.Remove(agentName);
                    connection.Disconnect();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"AlgoServer Connect failed: {ex.Message}");
            }
        }

        public void RestoreConnections()
        {
            foreach (var connection in BotAgents.Snapshot.Values.Where(c => c.Creds.Connect))
            {
                connection.Connect();
            }
        }

        public Task ShutdownDisconnect()
        {
            return Task.WhenAll(BotAgents.Snapshot.Values.Select(c => c.WaitDisconnect()));
        }
    }
}
