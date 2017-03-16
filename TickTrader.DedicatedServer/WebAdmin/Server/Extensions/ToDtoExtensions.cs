using System.Linq;
using TickTrader.Algo.Core.Metadata;
using TickTrader.DedicatedServer.DS;
using TickTrader.DedicatedServer.DS.Models;
using TickTrader.DedicatedServer.WebAdmin.Server.Dto;

namespace TickTrader.DedicatedServer.WebAdmin.Server.Extensions
{
    public static class ToDtoExtensions
    {
        public static PackageDto ToPackageDto(this IPackage model)
        {
            return new PackageDto()
            {
                Name = model.Name,
                Created = model.Created,
                Plugins = model.GetPluginsByType(AlgoTypes.Robot).Select(p => p.ToPluginDto()).ToArray(),
                IsValid = model.IsValid
            };
        }

        public static PluginDto ToPluginDto(this ServerPluginRef plugin)
        {
            return new PluginDto()
            {
                Id = plugin.Ref.Id,
                DisplayName = plugin.Ref.DisplayName,
                Type = plugin.Ref.Descriptor.AlgoLogicType.ToString()
            };
        }
    }
}
