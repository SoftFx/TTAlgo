using System;
using System.Linq;
using TickTrader.Algo.Protocol;

namespace TickTrader.BotAgent.CmdClient
{
    internal class ClientModel
    {
        private CommandUi _cmdEngine;
        private ProtocolClient _protocolClient;
        private BotAgentClient _agentClient;


        internal ClientModel()
        {
            _cmdEngine = new CommandUi();
            RegisterCommands();
        }

        internal void Start()
        {
            _cmdEngine.Run();
            if (_protocolClient != null)
            {
                _protocolClient.Disconnect();
            }
        }


        private void RegisterCommands()
        {
            _cmdEngine.RegisterCommand("login", LoginCommand);
            _cmdEngine.RegisterCommand("connection status", ConnectionStatusCommand);
            _cmdEngine.RegisterCommand("account info", AccountInfoCommand);
            _cmdEngine.RegisterCommand("package info", PackageInfoCommand);
        }

        private void LoginCommand()
        {
            var server = CommandUi.InputString("server");
            var login = CommandUi.InputString("login");
            var password = CommandUi.InputString("password");

            var clientConfig = new ProtocolClientSettings
            {
                ServerAddress = server,
                ServerCertificateName = "certificate.pfx",
                Login = login,
                Password = password,
                ProtocolSettings = new ProtocolSettings
                {
                    ListeningPort = 8443,
                    LogDirectoryName = "Logs",
                    LogEvents = true,
                    LogMessages = true,
                    LogStates = true,
                }
            };

            if (_protocolClient != null && _protocolClient.State != ClientStates.Offline)
            {
                _protocolClient.Disconnect();
            }

            _agentClient = new BotAgentClient();
            _protocolClient = new ProtocolClient(_agentClient);
            _protocolClient.TriggerConnect(clientConfig);
        }

        private bool CheckConnectionState()
        {
            if (_protocolClient == null)
            {
                Console.WriteLine("Run login command first");
                return false;
            }

            Console.WriteLine($"{_protocolClient.State} - {_protocolClient.LastError}");

            Console.WriteLine();

            return _protocolClient.State == ClientStates.Online;
        }

        private void ConnectionStatusCommand()
        {
            if (!CheckConnectionState())
            {
                return;
            }

            Console.WriteLine($"Current version: {_protocolClient.VersionSpec.CurrentVersionStr}");
        }

        private void AccountInfoCommand()
        {
            if (!CheckConnectionState())
            {
                return;
            }

            var acc = CommandUi.Choose("account", _agentClient.Accounts, a => $"{a.Server} - {a.Login}");
            var bots = _agentClient.Bots.Where(b => b.Account.Server == acc.Server && b.Account.Login == acc.Login).ToList();

            Console.WriteLine($"{acc.Server} - {acc.Login}");
            Console.WriteLine($"Bots cnt: {bots.Count}");
            foreach (var bot in bots)
            {
                Console.WriteLine();
                Console.WriteLine($"id: {bot.InstanceId}");
                Console.WriteLine($"isolated: {bot.Isolated}");
                Console.WriteLine($"state: {bot.State}");
                Console.WriteLine($"descriptor: {bot.Plugin.DescriptorId}");
                Console.WriteLine($"package: {bot.Plugin.PackageName}");
                Console.WriteLine();
            }
        }

        private void PackageInfoCommand()
        {
            if (!CheckConnectionState())
            {
                return;
            }

            var package = CommandUi.Choose("package", _agentClient.Packages, p => p.Name);

            Console.WriteLine($"Name: {package.Name}");
            Console.WriteLine($"Created: {package.Created}");
            foreach (var plugins in package.Plugins)
            {
                Console.WriteLine();
                Console.WriteLine($"ApiVersion: {plugins.Descriptor.ApiVersion}");
                Console.WriteLine($"Id: {plugins.Descriptor.Id}");
                Console.WriteLine($"DisplayName: {plugins.Descriptor.DisplayName}");
                Console.WriteLine($"UserDisplayName: {plugins.Descriptor.UserDisplayName}");
                Console.WriteLine($"Version: {plugins.Descriptor.Version}");
                Console.WriteLine($"Description: {plugins.Descriptor.Description}");
                Console.WriteLine($"Category: {plugins.Descriptor.Category}");
                Console.WriteLine($"Copyright: {plugins.Descriptor.Copyright}");
                Console.WriteLine($"Type: {plugins.Descriptor.Type}");
                Console.WriteLine();
            }
        }
    }
}
