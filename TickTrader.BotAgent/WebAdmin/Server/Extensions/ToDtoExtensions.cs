using System.Linq;
using TickTrader.Algo.Common.Model;
using TickTrader.Algo.Core.Metadata;
using TickTrader.Algo.Common.Model.Config;
using TickTrader.BotAgent.BA;
using TickTrader.BotAgent.WebAdmin.Server.Dto;
using TickTrader.BotAgent.BA.Info;
using TickTrader.BotAgent.BA.Models;
using TickTrader.BotAgent.WebAdmin.Server.Models;
using TickTrader.Algo.Core;
using System.Reflection;

namespace TickTrader.BotAgent.WebAdmin.Server.Extensions
{
    public static class ToDtoExtensions
    {
        private static readonly string nIntTypeName;
        private static readonly string nDoubleTypeName;

        static ToDtoExtensions()
        {
            nIntTypeName = typeof(int?).GetTypeInfo().FullName;
            nDoubleTypeName = typeof(double?).GetTypeInfo().FullName;
        }

        public static AccountInfoDto ToDto(this ConnectionInfo info)
        {
            return new AccountInfoDto
            {
                Symbols = info.Symbols.Select(s => s.Name).OrderBy(x => x).ToArray()
            };
        }

        public static TradeBotDto ToDto(this ITradeBot bot)
        {
            return new TradeBotDto()
            {
                Id = bot.Id,
                Isolated = bot.Isolated,
                Account = bot.Account.ToDto(),
                State = bot.State.ToString(),
                PackageName = bot.PackageName,
                BotName = bot.BotName,
                FaultMessage = bot.FaultMessage,
                Config = bot.ToConfigDto(),
                Permissions = bot.Permissions.ToDto()

            };
        }

        public static PermissionsDto ToDto(this PluginPermissions permissions)
        {
            return new PermissionsDto
            {
                TradeAllowed = permissions.TradeAllowed
            };
        }

        public static TradeBotLogDto ToDto(this IBotLog botlog)
        {
            return new TradeBotLogDto
            {
                Snapshot = botlog.Messages.OrderByDescending(le => le.TimeUtc).Select(e => e.ToDto()).ToArray(),
                Files = botlog.Files.Select(fm => fm.ToDto()).ToArray()
            };
        }

        public static FileDto ToDto(this IFile file)
        {
            return new FileDto { Name = file.Name, Size = file.Size };
        }

        public static LogEntryDto ToDto(this ILogEntry entry)
        {
            return new LogEntryDto
            {
                Time = entry.TimeUtc,
                Type = entry.Type.ToString(),
                Message = entry.Message
            };
        }

        public static TradeBotConfigDto ToConfigDto(this ITradeBot bot)
        {
            var pluginDescriptor = bot.Package?.GetPluginRef(bot.Descriptor)?.Descriptor;
            var config = new TradeBotConfigDto()
            {
                Symbol = bot.Config.MainSymbol,
                Parameters = bot.Config.Properties.Select(p =>
                     new ParameterDto()
                     {
                         Id = p.Id,
                         Value = ((Parameter)p).ValObj,
                         Descriptor = pluginDescriptor?.Parameters.FirstOrDefault(dp => dp.Id == p.Id)?.ToDto()
                     }).ToArray()
            };
            return config;
        }

        public static PackageDto ToDto(this IPackage model)
        {
            return new PackageDto()
            {
                Name = model.Name,
                Created = model.Created,
                Plugins = model.GetPluginsByType(AlgoTypes.Robot).Select(p => p.ToPluginDto()).ToArray(),
                IsValid = model.IsValid
            };
        }

        public static PluginDto ToPluginDto(this PluginInfo plugin)
        {
            return new PluginDto()
            {
                Id = plugin.Descriptor.Id,
                DisplayName = plugin.Descriptor.DisplayName,
                UserDisplayName = plugin.Descriptor.UserDisplayName,
                Type = plugin.Descriptor.AlgoLogicType.ToString(),
                Parameters = plugin.Descriptor.Parameters.Select(p => p.ToDto())
            };
        }

        public static BotStateDto ToBotStateDto(this ITradeBot bot)
        {
            return new BotStateDto
            {
                Id = bot.Id,
                State = bot.State.ToString(),
                FaultMessage = bot.FaultMessage
            };
        }

        public static AccountDto ToDto(this IAccount account)
        {
            return new AccountDto()
            {
                Server = account.Address,
                Login = account.Username,
                LastConnectionStatus = ConnectionErrorCodes.None
            };
        }

        public static ParameterDescriptorDto ToDto(this ParameterDescriptor parameter)
        {
            return new ParameterDescriptorDto()
            {
                Id = parameter.Id,
                DisplayName = parameter.DisplayName,
                DataType = GetDataType(parameter),
                DefaultValue = parameter.DefaultValue,
                EnumValues = parameter.EnumValues,
                IsEnum = parameter.IsEnum,
                IsRequired = parameter.IsRequired,
                FileFilter = string.Join("|", parameter.FileFilters.Select(f => f.FileMask))
            };
        }

        private static string GetDataType(ParameterDescriptor parameter)
        {
            if (parameter.IsEnum)
                return ParameterTypes.Enumeration;
            else if (parameter.DataType == nDoubleTypeName)
                return ParameterTypes.NullableDouble;
            else if (parameter.DataType == nIntTypeName)
                return ParameterTypes.NullableInteger;
            else
                switch (parameter.DataType)
                {
                    case "System.Int32": return ParameterTypes.Integer;
                    case "System.Double": return ParameterTypes.Double;
                    case "System.String": return ParameterTypes.String;
                    case "System.Boolean": return ParameterTypes.Boolean;
                    case "TickTrader.Algo.Api.File": return ParameterTypes.File;
                    default: return "Unknown";
                }
        }
    }
}
