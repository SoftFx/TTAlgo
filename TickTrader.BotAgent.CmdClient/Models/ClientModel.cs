using System;
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
        }


        private void RegisterCommands()
        {
            _cmdEngine.RegsiterCommand("login", LoginCommand);
            _cmdEngine.RegsiterCommand("account info", AccountInfoCommand);
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
                _protocolClient.Disconnect().Wait();
            }

            _agentClient = new BotAgentClient();
            _protocolClient = new ProtocolClient(_agentClient, clientConfig);
            _protocolClient.Connect().Wait();
        }

        private void AccountInfoCommand()
        {
            var acc = CommandUi.Choose("account", _agentClient.Accounts, a => $"{a.Key.Server} - {a.Key.Login}");

            Console.WriteLine($"{acc.Key.Server} - {acc.Key.Login}");
            Console.WriteLine($"Bots cnt: {acc.Bots.Length}");
            foreach (var bot in acc.Bots)
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
    }
}
