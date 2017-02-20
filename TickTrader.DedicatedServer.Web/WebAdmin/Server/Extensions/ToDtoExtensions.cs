using System.Linq;
using TickTrader.Algo.Core.Metadata;
using TickTrader.DedicatedServer.DS.Models;
using TickTrader.DedicatedServer.WebAdmin.Server.Dto;

namespace TickTrader.DedicatedServer.WebAdmin.Server.Extensions
{
    public static class ToDtoExtensions
    {
        public static PackageDto ToPackageDto(this PackageModel model)
        {
            return new PackageDto()
            {
                Name = model.Name,
                Created = model.Created,
                Plugins = model.Container?.Plugins.Select(p => p.ToPluginDto()).ToArray(),
                IsValid = model.IsValid
            };
        }

        public static PluginDto ToPluginDto(this AlgoPluginRef plugin)
        {
            return new PluginDto()
            {
                Id = plugin.Id,
                DisplayName = plugin.DisplayName,
                Type = plugin.Descriptor.AlgoLogicType.ToString()
            };
        }
    }
}
