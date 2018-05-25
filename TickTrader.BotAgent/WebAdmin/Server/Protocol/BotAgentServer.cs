using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using TickTrader.Algo.Common.Info;
using TickTrader.Algo.Core;
using TickTrader.Algo.Protocol;
using TickTrader.BotAgent.BA;
using TickTrader.BotAgent.BA.Entities;
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


        public event Action<UpdateInfo<Algo.Common.Info.PackageInfo>> PackageUpdated = delegate { };
        public event Action<UpdateInfo<Algo.Common.Info.AccountKey>> AccountUpdated = delegate { };
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

        public List<Algo.Common.Info.AccountKey> GetAccountList()
        {
            return _botAgent.GetAccounts().Select(
                    acc => new Algo.Common.Info.AccountKey
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
                    bot => bot.Id
                    //new BotModelEntity
                    //{
                    //InstanceId = bot.Id,
                    //State = ToProtocol.Convert(bot.State),
                    //Permissions = new PluginPermissionsEntity { TradeAllowed = bot.Permissions.TradeAllowed, Isolated = bot.Permissions.Isolated },
                    //Account = new AccountKeyEntity { Server = bot.Account.Address, Login = bot.Account.Username },
                    //Plugin = new PluginKeyEntity { DescriptorId = bot.Descriptor, PackageName = bot.PackageName },
                    ).ToList();
        }

        public List<Algo.Common.Info.PackageInfo> GetPackageList()
        {
            return _botAgent.GetPackages().Select(
                    package => new Algo.Common.Info.PackageInfo
                    {
                        Key = new PackageKey(package.Name, Algo.Core.Repository.RepositoryLocation.LocalRepository),
                        CreatedUtc = package.Created.ToUniversalTime(),
                        Plugins = new List<Algo.Common.Info.PluginInfo>(),
                            //package.Plugins.Select(
                            //plugin => new PluginInfoEntity
                            //{
                            //    Key = new PluginKeyEntity { DescriptorId = plugin.Id.DescriptorId, PackageName = plugin.Id.PackageName },
                            //    Descriptor = new PluginDescriptorEntity
                            //    {
                            //        ApiVersion = plugin.Descriptor.ApiVersionStr,
                            //        Id = plugin.Descriptor.Id,
                            //        DisplayName = plugin.Descriptor.UiDisplayName,
                            //        UserDisplayName = plugin.Descriptor.DisplayName,
                            //        Category = plugin.Descriptor.Category,
                            //        Copyright = plugin.Descriptor.Copyright,
                            //        Description = plugin.Descriptor.Description,
                            //        Type = ToProtocol.Convert(plugin.Descriptor.Type),
                            //        Version = plugin.Descriptor.Version,
                            //    },
                            //}).ToList(),
                    }).ToList();
        }


        private void OnAccountChanged(AccountInfo account, ChangeAction action)
        {
            try
            {
                AccountUpdated(new UpdateInfo<Algo.Common.Info.AccountKey>
                {
                    //Type = ToProtocol.Convert(action),
                    Type = UpdateType.Added,
                    Value = new Algo.Common.Info.AccountKey
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

        private void OnBotChanged(TradeBotInfo bot, ChangeAction action)
        {
            try
            {
                BotUpdated(new UpdateInfo<string>
                {
                    Type = UpdateType.Added,
                    Value = bot.Id,
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

        private void OnPackageChanged(BA.Entities.PackageInfo package, ChangeAction action)
        {
            try
            {
                PackageUpdated(new UpdateInfo<Algo.Common.Info.PackageInfo>
                {
                    Type = UpdateType.Added,
                    Value = new Algo.Common.Info.PackageInfo
                    {
                        Key = new PackageKey(package.Name, Algo.Core.Repository.RepositoryLocation.LocalRepository),
                        CreatedUtc = package.Created.ToUniversalTime(),
                        Plugins = new List<Algo.Common.Info.PluginInfo>(),
                        //package.Plugins.Select(
                        //    plugin => new PluginInfoEntity
                        //    {
                        //        Key = new PluginKeyEntity { DescriptorId = plugin.Id.DescriptorId, PackageName = plugin.Id.PackageName },
                        //        Descriptor = new PluginDescriptorEntity
                        //        {
                        //            ApiVersion = plugin.Descriptor.ApiVersionStr,
                        //            Id = plugin.Descriptor.Id,
                        //            DisplayName = plugin.Descriptor.UiDisplayName,
                        //            UserDisplayName = plugin.Descriptor.DisplayName,
                        //            Category = plugin.Descriptor.Category,
                        //            Copyright = plugin.Descriptor.Copyright,
                        //            Description = plugin.Descriptor.Description,
                        //            Type = ToProtocol.Convert(plugin.Descriptor.Type),
                        //            Version = plugin.Descriptor.Version,
                        //        },
                        //    }).ToArray(),
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to send update: {ex.Message}", ex);
            }
        }

        private void OnBotStateChanged(TradeBotInfo bot)
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

        private void OnAccountStateChanged(AccountInfo account)
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
