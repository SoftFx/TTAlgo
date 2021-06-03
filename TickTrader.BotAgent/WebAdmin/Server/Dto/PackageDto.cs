using System;

namespace TickTrader.BotAgent.WebAdmin.Server.Dto
{
    public class PackageDto
    {
        public string Id { get; set; }
        public string DisplayName { get; set; }
        public DateTime Created { get; set; }
        public PluginDto[] Plugins { get; set; }
        public bool IsValid { get; set; }
    }
}
