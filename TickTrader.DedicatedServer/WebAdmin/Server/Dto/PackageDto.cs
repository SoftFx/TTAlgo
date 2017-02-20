using System;

namespace TickTrader.DedicatedServer.WebAdmin.Server.Dto
{
    public class PackageDto
    {
        public string Name { get; set; }
        public DateTime Created { get; set; }
        public PluginDto[] Plugins { get; set; }
        public bool IsValid { get; set; }
    }
}
