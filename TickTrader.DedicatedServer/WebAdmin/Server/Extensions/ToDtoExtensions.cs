using System.Linq;
using TickTrader.Algo.Common.Model;
using TickTrader.Algo.Core.Metadata;
using TickTrader.Algo.Common.Model.Config;
using TickTrader.DedicatedServer.DS;
using TickTrader.DedicatedServer.WebAdmin.Server.Dto;
using TickTrader.DedicatedServer.DS.Info;

namespace TickTrader.DedicatedServer.WebAdmin.Server.Extensions
{
    public static class ToDtoExtensions
    {
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
                Account = bot.Account.ToDto(),
                State = bot.State.ToString(),
                PackageName = bot.PackageName,
                BotName = bot.BotName,
                FaultMessage = bot.FaultMessage,
                Config = bot.ToConfigDto()
            };
        }

        public static TradeBotLogDto ToDto(this IBotLog botlog)
        {
            return new TradeBotLogDto
            {
                Snapshot = botlog.Messages.OrderByDescending(le => le.TimeUtc).Select(e => e.ToDto()).ToArray(),
                Files = botlog.Files.Select(fm => new FileDto { Name = fm.Name, Size = fm.Size }).ToArray()
            };
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
            var descriptor = bot.Package?.GetPluginRef(bot.Descriptor)?.Descriptor;
            var config = new TradeBotConfigDto()
            {
                Symbol = bot.Config.MainSymbol,
                Parameters = bot.Config.Properties.Select(p =>
                new ParameterDto()
                {
                    Id = p.Id,
                    Value = ((Parameter)p).ValObj,
                    Descriptor =  descriptor?.Parameters.FirstOrDefault(dp => dp.Id == p.Id)?.ToDto()
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
                return "Enum";

            switch (parameter.DataType)
            {
                case "System.Int32": return "Int";
                case "System.Double": return "Double";
                case "System.String": return "String";
                case "TickTrader.Algo.Api.File": return "File";
                default: return "Unknown";
            }
        }
    }
}
