using System.Linq;
using TickTrader.BotAgent.BA;
using TickTrader.BotAgent.WebAdmin.Server.Dto;
using TickTrader.BotAgent.BA.Models;
using TickTrader.BotAgent.WebAdmin.Server.Models;
using System.Reflection;
using TickTrader.Algo.Common.Model.Setup;
using TickTrader.Algo.Domain;
using System.Threading.Tasks;

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

        public static AccountInfoDto ToDto(this AccountMetadataInfo info)
        {
            return new AccountInfoDto
            {
                Symbols = info.Symbols.Select(s => s.Name).OrderBy(x => x).ToArray()
            };
        }

        public static TradeBotDto ToDto(this PluginModelInfo bot)
        {
            return new TradeBotDto()
            {
                Id = bot.InstanceId,
                Account = bot.Account.ToDto(),
                State = bot.State.ToString(),
                PackageName = bot.Config.Key.Package.Name,
                BotName = bot.Descriptor_?.UiDisplayName,
                FaultMessage = bot.FaultMessage,
                Config = bot.ToConfigDto(),
                Permissions = bot.Config.Permissions.ToDto(),
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

        public static async Task<TradeBotLogDto> ToDto(this IBotLog botlog)
        {
            return new TradeBotLogDto
            {
                Snapshot = (await botlog.GetMessages()).OrderByDescending(le => le.TimeUtc).Select(e => e.ToDto()).ToArray(),
                Files = (await botlog.GetFiles()).Select(fm => fm.ToDto()).ToArray()
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
                Time = entry.TimeUtc.ToDateTime(),
                Type = entry.Severity.ToString(),
                Message = entry.Message
            };
        }

        public static TradeBotConfigDto ToConfigDto(this PluginModelInfo bot)
        {
            var config = new TradeBotConfigDto()
            {
                Symbol = bot.Config.MainSymbol.Name,
                Parameters = bot.Config.UnpackProperties().Where(p => p is IParameterConfig).Select(p =>
                     new ParameterDto()
                     {
                         Id = p.PropertyId,
                         Value = ((IParameterConfig)p).ValObj,
                         Descriptor = bot.Descriptor_?.Parameters.FirstOrDefault(dp => dp.Id == p.PropertyId)?.ToDto()
                     }).ToArray()
            };
            return config;
        }

        public static PackageDto ToDto(this PackageInfo package)
        {
            return new PackageDto()
            {
                Name = package.Key.Name,
                DisplayName = package.Identity.FileName,
                Created = package.Identity.LastModifiedUtc.ToDateTime().ToLocalTime(),
                Plugins = package.Plugins.Where(p => p.Descriptor_.IsTradeBot).Select(p => p.ToPluginDto()).ToArray(),
                IsValid = package.IsValid
            };
        }

        public static PluginDto ToPluginDto(this PluginInfo plugin)
        {
            var descriptor = plugin.Descriptor_;
            return new PluginDto()
            {
                Id = descriptor.Id,
                DisplayName = descriptor.UiDisplayName,
                UserDisplayName = descriptor.DisplayName,
                Type = descriptor.Type.ToString(),
                Parameters = descriptor.Parameters.Select(p => p.ToDto())
            };
        }

        public static BotStateDto ToBotStateDto(this PluginModelInfo bot)
        {
            return new BotStateDto
            {
                Id = bot.InstanceId,
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
            };
        }

        public static AccountDto ToDto(this AccountModelInfo account)
        {
            return new AccountDto()
            {
                Server = account.Key.Server,
                Login = account.Key.Login,
                LastConnectionStatus = ConnectionErrorInfo.Types.ErrorCode.NoConnectionError,
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
                EnumValues = parameter.EnumValues.ToList(),
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
