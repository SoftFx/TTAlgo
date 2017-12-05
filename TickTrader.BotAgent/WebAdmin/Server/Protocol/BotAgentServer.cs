using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using TickTrader.Algo.Protocol;
using TickTrader.BotAgent.BA;
using TickTrader.BotAgent.WebAdmin.Server.Extensions;
using TickTrader.BotAgent.WebAdmin.Server.Models;

namespace TickTrader.BotAgent.WebAdmin.Server.Protocol
{
    public class BotAgentServer : IBotAgentServer
    {
        private IBotAgent _botAgent;
        private ServerCredentials _serverCreds;


        public BotAgentServer(IServiceProvider services, IConfiguration serverConfig)
        {
            _botAgent = services.GetRequiredService<IBotAgent>();
            _serverCreds = serverConfig.GetCredentials();
        }


        public bool ValidateCreds(string login, string password)
        {
            return _serverCreds.Login == login && _serverCreds.Password == password;
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
