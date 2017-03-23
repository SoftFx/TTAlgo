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

        public static PluginDto ToPluginDto(this PlguinInfo plugin)
        {
            return new PluginDto()
            {
                Id = plugin.Id.DescriptorId,
                DisplayName = plugin.Descriptor.DisplayName,
                Type = plugin.Descriptor.AlgoLogicType.ToString()
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
    }
}
