using System.Linq;
using TickTrader.BotAgent.WebAdmin.Server.Dto;
using TickTrader.BotAgent.WebAdmin.Server.Models;
using System.Reflection;
using TickTrader.Algo.Core.Setup;
using TickTrader.Algo.Domain;

namespace TickTrader.BotAgent.WebAdmin.Server.Extensions
{
    public static class ToDtoExtensions
    {
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
                Account = bot.AccountId.ToAccountDto(),
                State = bot.State.ToString(),
                PackageName = bot.Config.Key.PackageId,
                PluginId = bot.Config.Key.DescriptorId,
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

        public static FileDto ToDto(this PluginFileInfo file)
        {
            return new FileDto { Name = file.Name, Size = file.Size };
        }

        public static LogEntryDto ToDto(this LogRecordInfo entry)
        {
            return new LogEntryDto
            {
                Time = entry.TimeUtc.ToDateTime(),
                Type = entry.Severity.ToString(),
                Message = entry.Message,
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
                Id = package.PackageId,
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

        public static BotStateDto ToBotStateDto(this PluginStateUpdate bot)
        {
            return new BotStateDto
            {
                Id = bot.Id,
                State = bot.State.ToString(),
                FaultMessage = bot.FaultMessage,
            };
        }

        public static AccountDto ToAccountDto(this string accountId)
        {
            AccountId.Unpack(accountId, out var accId);
            return new AccountDto()
            {
                Server = accId.Server,
                Login = accId.UserId,
            };
        }

        public static AccountDto ToDto(this AccountModelInfo account)
        {
            AccountId.Unpack(account.AccountId, out var accId);
            return new AccountDto()
            {
                Server = accId.Server,
                Login = accId.UserId,
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
            else if (parameter.DataType == ParameterDescriptor.NullableDoubleTypeName)
                return ParameterTypes.NullableDouble;
            else if (parameter.DataType == ParameterDescriptor.NullableIntTypeName)
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
            else if (parameter.DataType == ParameterDescriptor.NullableDoubleTypeName)
            {
                UiConverter.NullableDouble.FromObject(parameter.DefaultValue, out var defNullDoubleVal);
                return defNullDoubleVal;
            }
            else if (parameter.DataType == ParameterDescriptor.NullableIntTypeName)
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
