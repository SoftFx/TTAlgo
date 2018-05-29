using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using TickTrader.Algo.Common.Info;
using TickTrader.Algo.Core;
using TickTrader.Algo.Protocol;
using TickTrader.BotAgent.BA;
using TickTrader.BotAgent.BA.Models;
using TickTrader.BotAgent.WebAdmin.Server.Extensions;
using TickTrader.BotAgent.WebAdmin.Server.Models;

namespace TickTrader.BotAgent.WebAdmin.Server.Protocol
{
    public class BotAgentServer : IBotAgentServer
    {
        private static IAlgoCoreLogger _logger = CoreLoggerFactory.GetLogger<BotAgentServer>();


        private IBotAgent _botAgent;
        private ServerCredentials _serverCreds;


        public event Action<UpdateInfo<PackageInfo>> PackageUpdated = delegate { };
        public event Action<UpdateInfo<AccountKey>> AccountUpdated = delegate { };
        public event Action<UpdateInfo<string>> BotUpdated = delegate { };
        //public event Action<BotStateUpdateEntity> BotStateUpdated = delegate { };
        //public event Action<AccountStateUpdateEntity> AccountStateUpdated = delegate { };


        public BotAgentServer(IServiceProvider services, IConfiguration serverConfig)
        {
            _botAgent = services.GetRequiredService<IBotAgent>();
            _serverCreds = serverConfig.GetCredentials();

            if (_serverCreds == null)
                throw new Exception("Server credentials not found");

            _botAgent.AccountChanged += OnAccountChanged;
            _botAgent.BotChanged += OnBotChanged;
            _botAgent.PackageChanged += OnPackageChanged;
            _botAgent.BotStateChanged += OnBotStateChanged;
            _botAgent.AccountStateChanged += OnAccountStateChanged;
        }


        public bool ValidateCreds(string login, string password)
        {
            return _serverCreds.Login == login && _serverCreds.Password == password;
        }

        public List<AccountKey> GetAccountList()
        {
            return _botAgent.GetAccounts().Select(
                    acc => new AccountKey
                    {
                        Login = acc.Login,
                        Server = acc.Server,
                        //UseNewProtocol = acc.UseSfxProtocol,
                        //ConnectionState = ToProtocol.Convert(acc.ConnectionState),
                        //LastError = new ConnectionErrorEntity
                        //{
                        //    Code = ToProtocol.Convert(acc.LastError?.Code ?? Algo.Common.Model.Interop.ConnectionErrorCodes.None),
                        //    Text = acc.LastError?.TextMessage,
                        //},
                    }).ToList();
        }

        public List<string> GetBotList()
        {
            return _botAgent.GetTradeBots().Select(
                    bot => bot.InstanceId
                    //new BotModelEntity
                    //{
                    //InstanceId = bot.Id,
                    //State = ToProtocol.Convert(bot.State),
                    //Permissions = new PluginPermissionsEntity { TradeAllowed = bot.Permissions.TradeAllowed, Isolated = bot.Permissions.Isolated },
                    //Account = new AccountKeyEntity { Server = bot.Account.Address, Login = bot.Account.Username },
                    //Plugin = new PluginKeyEntity { DescriptorId = bot.Descriptor, PackageName = bot.PackageName },
                    ).ToList();
        }

        public List<PackageInfo> GetPackageList()
        {
            return _botAgent.GetPackages();
        }


        private UpdateType Convert(ChangeAction action)
        {
            switch (action)
            {
                case ChangeAction.Added:
                    return UpdateType.Added;
                case ChangeAction.Modified:
                    return UpdateType.Replaced;
                case ChangeAction.Removed:
                    return UpdateType.Removed;
                default:
                    throw new ArgumentException();
            }
        }

        private void OnAccountChanged(AccountModelInfo account, ChangeAction action)
        {
            try
            {
                AccountUpdated(new UpdateInfo<AccountKey>
                {
                    //Type = ToProtocol.Convert(action),
                    Type = Convert(action),
                    Value = new AccountKey
                    {
                        Login = account.Login,
                        Server = account.Server,
                        //UseNewProtocol = account.UseSfxProtocol,
                        //ConnectionState = ToProtocol.Convert(account.ConnectionState),
                        //LastError = new ConnectionErrorEntity
                        //{
                        //    Code = ToProtocol.Convert(account.LastError?.Code ?? Algo.Common.Model.Interop.ConnectionErrorCodes.None),
                        //    Text = account.LastError?.TextMessage,
                        //},
                    },
                });
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to send update: {ex.Message}", ex);
            }
        }

        private void OnBotChanged(BotModelInfo bot, ChangeAction action)
        {
            try
            {
                BotUpdated(new UpdateInfo<string>
                {
                    Type = Convert(action),
                    Value = bot.InstanceId,
                    //new TradeBotInfo
                    //{
                    //    InstanceId = bot.Id,
                    //    State = ToProtocol.Convert(bot.State),
                        //Permissions = new PluginPermissionsEntity { TradeAllowed = bot.Permissions.TradeAllowed, Isolated = bot.Permissions.Isolated },
                        //Account = new AccountKeyEntity { Server = bot.Account.Address, Login = bot.Account.Username },
                        //Plugin = new PluginKeyEntity { DescriptorId = bot.Descriptor, PackageName = bot.PackageName },
                    //}
                });
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to send update: {ex.Message}", ex);
            }
        }

        private void OnPackageChanged(PackageInfo package, ChangeAction action)
        {
            try
            {
                PackageUpdated(new UpdateInfo<PackageInfo>
                {
                    Type = Convert(action),
                    Value = package,
                });
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to send update: {ex.Message}", ex);
            }
        }

        private void OnBotStateChanged(BotModelInfo bot)
        {
            try
            {
                //BotStateUpdated(new BotStateUpdateEntity
                //{
                //    BotId = bot.Id,
                //    State = ToProtocol.Convert(bot.State),
                //});
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to send update: {ex.Message}", ex);
            }
        }

        private void OnAccountStateChanged(AccountModelInfo account)
        {
            try
            {
                //AccountStateUpdated(new AccountStateUpdateEntity
                //{
                //    Account = new AccountKeyEntity { Login = account.Login, Server = account.Server },
                //    //ConnectionState = ToProtocol.Convert(account.ConnectionState),
                //    //LastError = new ConnectionErrorEntity
                //    //{
                //    //    Code = ToProtocol.Convert(account.LastError?.Code ?? Algo.Common.Model.Interop.ConnectionErrorCodes.None),
                //    //    Text = account.LastError?.TextMessage,
                //    //},
                //});
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to send update: {ex.Message}", ex);
            }
        }
    }
}
