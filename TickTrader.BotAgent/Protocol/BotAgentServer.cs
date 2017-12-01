using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using TickTrader.Algo.Protocol;
using TickTrader.BotAgent.BA;

namespace TickTrader.BotAgent.Protocol
{
    public class BotAgentServer : IBotAgentServer
    {
        private IBotAgent _botAgent;
        private IConfiguration _serverConfig;
        private ILogger<BotAgentServer> _logger;


        public BotAgentServer(IServiceProvider services, IConfiguration serverConfig)
        {
            _botAgent = services.GetRequiredService<IBotAgent>();
            _serverConfig = serverConfig;
            _logger = services.GetRequiredService<ILogger<BotAgentServer>>();
        }


        public void Connected(int sessionId)
        {
            _logger.LogDebug($"Session {sessionId} connected");
        }

        public void Disconnected(int sessionId, string reason)
        {
            _logger.LogDebug($"Session {sessionId} disconnected: {reason}");
        }

        public AccountListReportEntity GetAccountList(string requestId)
        {
            return new AccountListReportEntity
            {
                RequestId = requestId,
                Accounts = _botAgent.Accounts.Select(
                    acc => new AccountModelEntity
                    {
                        Key = new AccountKeyEntity { Server = acc.Address, Login = acc.Username },
                        Bots = acc.TradeBots.Select(bot => new BotModelEntity
                        {
                            InstanceId = bot.Id,
                            Isolated = bot.Isolated,
                            State = (BotState)((int)bot.State),
                            Permissions = new PluginPermissionsEntity { TradeAllowed = bot.Permissions.TradeAllowed },
                            Account = new AccountKeyEntity { Server = bot.Account.Address, Login = bot.Account.Username },
                            Plugin = new PluginKeyEntity { DescriptorId = bot.Descriptor, PackageName = bot.PackageName },
                        }).ToArray(),
                    }).ToArray()
            };
        }
    }
}
