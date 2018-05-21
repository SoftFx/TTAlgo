using System.Linq;
using TickTrader.Algo.Core.Metadata;
using TickTrader.Algo.Common.Model.Config;
using TickTrader.BotAgent.BA;
using TickTrader.BotAgent.WebAdmin.Server.Dto;
using TickTrader.BotAgent.BA.Models;
using TickTrader.BotAgent.WebAdmin.Server.Models;
using TickTrader.Algo.Core;
using System.Reflection;
using TickTrader.Algo.Common.Model.Interop;
using TickTrader.Algo.Common.Model.Setup;
using TickTrader.BotAgent.BA.Entities;

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

        public static AccountInfoDto ToDto(this TradeMetadataInfo info)
        {
            return new AccountInfoDto
            {
                Symbols = info.Symbols.Select(s => s.Name).OrderBy(x => x).ToArray()
            };
        }

        public static TradeBotDto ToDto(this TradeBotInfo bot)
        {
            return new TradeBotDto()
            {
                Id = bot.Id,
                Account = bot.Account.ToDto(),
                State = bot.State.ToString(),
                PackageName = bot.Config.Plugin.PackageName,
                BotName = bot.BotName,
                FaultMessage = bot.FaultMessage,
                Config = bot.ToConfigDto(),
                Permissions = bot.Config.PluginConfig.Permissions.ToDto(),
            };
        }

        public static PermissionsDto ToDto(this PluginPermissions permissions)
        {
            return new PermissionsDto
            {
                TradeAllowed = permissions.TradeAllowed,
                Isolated = permissions.Isolated,
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

        public static TradeBotConfigDto ToConfigDto(this TradeBotInfo bot)
        {
            var pluginDescriptor = bot.Metadata;
            var config = new TradeBotConfigDto()
            {
                Symbol = bot.Config.PluginConfig.MainSymbol,
                Parameters = bot.Config.PluginConfig.Properties.Select(p =>
                     new ParameterDto()
                     {
                         Id = p.Id,
                         Value = ((Parameter)p).ValObj,
                         Descriptor = pluginDescriptor?.Descriptor.Parameters.FirstOrDefault(dp => dp.Id == p.Id)?.ToDto()
                     }).ToArray()
            };
            return config;
        }

        public static PackageDto ToDto(this PackageInfo model)
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
                DisplayName = plugin.Descriptor.UiDisplayName,
                UserDisplayName = plugin.Descriptor.DisplayName,
                Type = plugin.Descriptor.Type.ToString(),
                Parameters = plugin.Descriptor.Parameters.Select(p => p.ToDto())
            };
        }

        public static BotStateDto ToBotStateDto(this TradeBotInfo bot)
        {
            return new BotStateDto
            {
                Id = bot.Id,
                State = bot.State.ToString(),
                FaultMessage = bot.FaultMessage
            };
        }

        public static AccountDto ToDto(this AccountKey account)
        {
            return new AccountDto()
            {
                Server = account.Server,
                Login = account.Login,
                //LastConnectionStatus = ConnectionErrorCodes.None,
                //UseNewProtocol = account.UseNewProtocol
            };
        }

        public static AccountDto ToDto(this AccountInfo account)
        {
            return new AccountDto()
            {
                Server = account.Server,
                Login = account.Login,
                LastConnectionStatus = ConnectionErrorCodes.None,
                UseNewProtocol = account.UseSfxProtocol
            };
        }

        public static ParameterDescriptorDto ToDto(this ParameterDescriptor parameter)
        {
            return new ParameterDescriptorDto()
            {
                Id = parameter.Id,
                DisplayName = parameter.DisplayName,
                DataType = GetDataType(parameter),
                DefaultValue = ConvertDefaultValue(parameter),
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

        private static object ConvertDefaultValue(ParameterDescriptor parameter)
        {
            if (parameter.IsEnum)
            {
                UiConverter.String.FromObject(parameter.DefaultValue, out var defEnumVal);
                if (string.IsNullOrEmpty(defEnumVal))
                    defEnumVal = parameter.EnumValues.FirstOrDefault();
                return defEnumVal;
            }
            else if (parameter.DataType == nDoubleTypeName)
            {
                UiConverter.NullableDouble.FromObject(parameter.DefaultValue, out var defNullDoubleVal);
                return defNullDoubleVal;
            }
            else if (parameter.DataType == nIntTypeName)
            {
                UiConverter.NullableInt.FromObject(parameter.DefaultValue, out var defNullIntVal);
                return defNullIntVal;
            }
            else
                switch (parameter.DataType)
                {
                    case "System.Int32":
                        UiConverter.Int.FromObject(parameter.DefaultValue, out var defIntVal);
                        return defIntVal;
                    case "System.Double":
                        UiConverter.Double.FromObject(parameter.DefaultValue, out var defDoubleVal);
                        return defDoubleVal;
                    case "System.String":
                        UiConverter.String.FromObject(parameter.DefaultValue, out var defStringVal);
                        return defStringVal;
                    case "System.Boolean":
                        UiConverter.Bool.FromObject(parameter.DefaultValue, out var defBoolVal);
                        return defBoolVal;
                    case "TickTrader.Algo.Api.File":
                        UiConverter.String.FromObject(parameter.DefaultValue, out var defFileNameVal);
                        return defFileNameVal;
                    default: return null;
                }
        }
    }
}
