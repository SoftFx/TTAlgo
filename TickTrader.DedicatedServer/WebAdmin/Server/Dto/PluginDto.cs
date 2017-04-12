using System.Collections.Generic;

namespace TickTrader.DedicatedServer.WebAdmin.Server.Dto
{
    public class PluginDto
    {
        public string Id { get; set; }
        public string DisplayName { get; set; }
        public string Type { get; set; }
        public IEnumerable<ParameterDescriptorDto> Parameters { get; internal set; }
    }
}