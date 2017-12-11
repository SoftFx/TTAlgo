using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
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


        public event Action<AccountModelUpdateEntity> AccountUpdated = delegate { };
        public event Action<BotModelUpdateEntity> BotUpdated = delegate { };
        public event Action<PackageModelUpdateEntity> PackageUpdated = delegate { };


        public BotAgentServer(IServiceProvider services, IConfiguration serverConfig)
        {
            _botAgent = services.GetRequiredService<IBotAgent>();
            _serverCreds = serverConfig.GetCredentials();

            _botAgent.AccountChanged += OnAccountChanged;
            _botAgent.BotChanged += OnBotChanged;
            _botAgent.PackageChanged += OnPackageChanged;
        }


        public bool ValidateCreds(string login, string password)
        {
            return _serverCreds.Login == login && _serverCreds.Password == password;
        }

        public AccountListReportEntity GetAccountList()
        {
            return new AccountListReportEntity
            {
                Accounts = _botAgent.Accounts.Select(
                    acc => new AccountModelEntity
                    {
                        Login = acc.Username,
                        Server = acc.Address,
                    }).ToArray(),
            };
        }

        public BotListReportEntity GetBotList()
        {
            return new BotListReportEntity
            {
                Bots = _botAgent.TradeBots.Select(
                    bot => new BotModelEntity
                    {
                        InstanceId = bot.Id,
                        Isolated = bot.Isolated,
                        State = ToProtocol.Convert(bot.State),
                        Permissions = new PluginPermissionsEntity { TradeAllowed = bot.Permissions.TradeAllowed },
                        Account = new AccountKeyEntity { Server = bot.Account.Address, Login = bot.Account.Username },
                        Plugin = new PluginKeyEntity { DescriptorId = bot.Descriptor, PackageName = bot.PackageName },
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
                        Plugins = package.GetPlugins().Select(
                            plugin => new PluginInfoEntity
                            {
                                Key = new PluginKeyEntity { DescriptorId = plugin.Id.DescriptorId, PackageName = plugin.Id.PackageName },
                                Descriptor = new PluginDescriptorEntity
                                {
                                    ApiVersion = plugin.Descriptor.ApiVersionStr,
                                    Id = plugin.Descriptor.Id,
                                    DisplayName = plugin.Descriptor.DisplayName,
                                    UserDisplayName = plugin.Descriptor.UserDisplayName,
                                    Category = plugin.Descriptor.Category,
                                    Copyright = plugin.Descriptor.Copyright,
                                    Description = plugin.Descriptor.Description,
                                    Type = ToProtocol.Convert(plugin.Descriptor.AlgoLogicType),
                                    Version = plugin.Descriptor.Version,
                                },
                            }).ToArray(),
                    }).ToArray(),
            };
        }


        private void OnAccountChanged(IAccount account, ChangeAction action)
        {
            try
            {
                AccountUpdated(new AccountModelUpdateEntity
                {
                    Type = ToProtocol.Convert(action),
                    Item = new AccountModelEntity
                    {
                        Login = account.Username,
                        Server = account.Address,
                    },
                });
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to send update: {ex.Message}", ex);
            }
        }

        private void OnBotChanged(ITradeBot bot, ChangeAction action)
        {
            try
            {
                BotUpdated(new BotModelUpdateEntity
                {
                    Type = ToProtocol.Convert(action),
                    Item = new BotModelEntity
                    {
                        InstanceId = bot.Id,
                        Isolated = bot.Isolated,
                        State = ToProtocol.Convert(bot.State),
                        Permissions = new PluginPermissionsEntity { TradeAllowed = bot.Permissions.TradeAllowed },
                        Account = new AccountKeyEntity { Server = bot.Account.Address, Login = bot.Account.Username },
                        Plugin = new PluginKeyEntity { DescriptorId = bot.Descriptor, PackageName = bot.PackageName },
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to send update: {ex.Message}", ex);
            }
        }

        private void OnPackageChanged(IPackage package, ChangeAction action)
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
                        Plugins = package.GetPlugins().Select(
                            plugin => new PluginInfoEntity
                            {
                                Key = new PluginKeyEntity { DescriptorId = plugin.Id.DescriptorId, PackageName = plugin.Id.PackageName },
                                Descriptor = new PluginDescriptorEntity
                                {
                                    ApiVersion = plugin.Descriptor.ApiVersion.ToString(),
                                    Id = plugin.Descriptor.Id,
                                    DisplayName = plugin.Descriptor.DisplayName,
                                    UserDisplayName = plugin.Descriptor.UserDisplayName,
                                    Category = plugin.Descriptor.Category,
                                    Copyright = plugin.Descriptor.Copyright,
                                    Description = plugin.Descriptor.Description,
                                    Type = ToProtocol.Convert(plugin.Descriptor.AlgoLogicType),
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
    }
}
