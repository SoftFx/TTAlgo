using System;

namespace TickTrader.BotAgent.WebAdmin.Server.Dto
{
    public class PackageDto
    {
        public string Name { get; set; }
        public DateTime Created { get; set; }
        public PluginDto[] Plugins { get; set; }
        public bool IsValid { get; set; }
    }
}
