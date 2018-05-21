using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
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


        public event Action<AccountModelUpdateEntity> AccountUpdated = delegate { };
        public event Action<BotModelUpdateEntity> BotUpdated = delegate { };
        public event Action<PackageModelUpdateEntity> PackageUpdated = delegate { };
        public event Action<BotStateUpdateEntity> BotStateUpdated = delegate { };
        public event Action<AccountStateUpdateEntity> AccountStateUpdated = delegate { };


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

        public AccountListReportEntity GetAccountList()
        {
            return new AccountListReportEntity
            {
                Accounts = _botAgent.GetAccounts().Select(
                    acc => new AccountModelEntity
                    {
                        Login = acc.Login,
                        Server = acc.Server,
                        UseNewProtocol = acc.UseSfxProtocol,
                        //ConnectionState = ToProtocol.Convert(acc.ConnectionState),
                        //LastError = new ConnectionErrorEntity
                        //{
                        //    Code = ToProtocol.Convert(acc.LastError?.Code ?? Algo.Common.Model.Interop.ConnectionErrorCodes.None),
                        //    Text = acc.LastError?.TextMessage,
                        //},
                    }).ToArray(),
            };
        }

        public BotListReportEntity GetBotList()
        {
            return new BotListReportEntity
            {
                Bots = _botAgent.GetTradeBots().Select(
                    bot => new BotModelEntity
                    {
                        InstanceId = bot.Id,
                        State = ToProtocol.Convert(bot.State),
                        //Permissions = new PluginPermissionsEntity { TradeAllowed = bot.Permissions.TradeAllowed, Isolated = bot.Permissions.Isolated },
                        //Account = new AccountKeyEntity { Server = bot.Account.Address, Login = bot.Account.Username },
                        //Plugin = new PluginKeyEntity { DescriptorId = bot.Descriptor, PackageName = bot.PackageName },
                    }).ToArray(),
            };
        }

        public PackageListReportEntity GetPackageList()
        {
            return new PackageListReportEntity
            {
                Packages = _botAgent.GetPackages().Select(
                    package => new PackageModelEntity
                    {
                        Name = package.Name,
                        Created = package.Created,
                        Plugins = package.Plugins.Select(
                            plugin => new PluginInfoEntity
                            {
                                Key = new PluginKeyEntity { DescriptorId = plugin.Id.DescriptorId, PackageName = plugin.Id.PackageName },
                                Descriptor = new PluginDescriptorEntity
                                {
                                    ApiVersion = plugin.Descriptor.ApiVersionStr,
                                    Id = plugin.Descriptor.Id,
                                    DisplayName = plugin.Descriptor.UiDisplayName,
                                    UserDisplayName = plugin.Descriptor.DisplayName,
                                    Category = plugin.Descriptor.Category,
                                    Copyright = plugin.Descriptor.Copyright,
                                    Description = plugin.Descriptor.Description,
                                    Type = ToProtocol.Convert(plugin.Descriptor.Type),
                                    Version = plugin.Descriptor.Version,
                                },
                            }).ToArray(),
                    }).ToArray(),
            };
        }


        private void OnAccountChanged(AccountInfo account, ChangeAction action)
        {
            try
            {
                AccountUpdated(new AccountModelUpdateEntity
                {
                    Type = ToProtocol.Convert(action),
                    Item = new AccountModelEntity
                    {
                        Login = account.Login,
                        Server = account.Server,
                        UseNewProtocol = account.UseSfxProtocol,
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
                BotUpdated(new BotModelUpdateEntity
                {
                    Type = ToProtocol.Convert(action),
                    Item = new BotModelEntity
                    {
                        InstanceId = bot.Id,
                        State = ToProtocol.Convert(bot.State),
                        //Permissions = new PluginPermissionsEntity { TradeAllowed = bot.Permissions.TradeAllowed, Isolated = bot.Permissions.Isolated },
                        //Account = new AccountKeyEntity { Server = bot.Account.Address, Login = bot.Account.Username },
                        //Plugin = new PluginKeyEntity { DescriptorId = bot.Descriptor, PackageName = bot.PackageName },
                    }
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
                PackageUpdated(new PackageModelUpdateEntity
                {
                    Type = ToProtocol.Convert(action),
                    Item = new PackageModelEntity
                    {
                        Name = package.Name,
                        Created = package.Created,
                        Plugins = package.Plugins.Select(
                            plugin => new PluginInfoEntity
                            {
                                Key = new PluginKeyEntity { DescriptorId = plugin.Id.DescriptorId, PackageName = plugin.Id.PackageName },
                                Descriptor = new PluginDescriptorEntity
                                {
                                    ApiVersion = plugin.Descriptor.ApiVersionStr,
                                    Id = plugin.Descriptor.Id,
                                    DisplayName = plugin.Descriptor.UiDisplayName,
                                    UserDisplayName = plugin.Descriptor.DisplayName,
                                    Category = plugin.Descriptor.Category,
                                    Copyright = plugin.Descriptor.Copyright,
                                    Description = plugin.Descriptor.Description,
                                    Type = ToProtocol.Convert(plugin.Descriptor.Type),
                                    Version = plugin.Descriptor.Version,
                                },
                            }).ToArray(),
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
                BotStateUpdated(new BotStateUpdateEntity
                {
                    BotId = bot.Id,
                    State = ToProtocol.Convert(bot.State),
                });
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
                AccountStateUpdated(new AccountStateUpdateEntity
                {
                    Account = new AccountKeyEntity { Login = account.Login, Server = account.Server },
                    //ConnectionState = ToProtocol.Convert(account.ConnectionState),
                    //LastError = new ConnectionErrorEntity
                    //{
                    //    Code = ToProtocol.Convert(account.LastError?.Code ?? Algo.Common.Model.Interop.ConnectionErrorCodes.None),
                    //    Text = account.LastError?.TextMessage,
                    //},
                });
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to send update: {ex.Message}", ex);
            }
        }
    }
}
