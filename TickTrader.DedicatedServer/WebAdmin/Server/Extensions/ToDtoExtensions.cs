using System.Linq;
using TickTrader.Algo.Common.Model;
using TickTrader.Algo.Core.Metadata;
using TickTrader.DedicatedServer.DS;
using TickTrader.DedicatedServer.WebAdmin.Server.Dto;

namespace TickTrader.DedicatedServer.WebAdmin.Server.Extensions
{
    public static class ToDtoExtensions
    {
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
