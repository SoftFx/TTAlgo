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
                BotAgents.Add(agent.ServerAddress, new BotAgentConnectionManager(agent));
            }
        }


        public async Task<string> Connect(string login, string password, string server, int port, string displayName)
        {
            var creds = _botAgentStorage.Update(login, password, server, port, displayName);
            _botAgentStorage.Save();

            if (!BotAgents.ContainsKey(server))
            {
                BotAgents.Add(server, new BotAgentConnectionManager(creds));
            }
            var connection = BotAgents[server];
            await connection.WaitConnect();

            if (string.IsNullOrWhiteSpace(connection.LastError))
            {
                _botAgentStorage.UpdateConnect(server, true);
                _botAgentStorage.Save();
            }

            return connection.LastError;
        }

        public void Connect(string server)
        {
            try
            {
                if (BotAgents.TryGetValue(server, out var connection))
                {
                    _botAgentStorage.UpdateConnect(server, true);
                    _botAgentStorage.Save();
                    connection.Connect();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"BotAgent Connect failed: {ex.Message}");
            }
        }

        public void Disconnect(string server)
        {
            try
            {
                if (BotAgents.TryGetValue(server, out var connection))
                {
                    _botAgentStorage.UpdateConnect(server, false);
                    _botAgentStorage.Save();
                    connection.Disconnect();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"BotAgent Disconnect failed: {ex.Message}");
            }
        }

        public void Remove(string server)
        {
            try
            {
                if (BotAgents.TryGetValue(server, out var connection))
                {
                    _botAgentStorage.Remove(server);
                    _botAgentStorage.Save();
                    BotAgents.Remove(server);
                    connection.Disconnect();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"BotAgent Connect failed: {ex.Message}");
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
