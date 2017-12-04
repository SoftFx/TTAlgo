using System.Collections.Generic;

namespace TickTrader.BotAgent.WebAdmin.Server.Dto
{
    public class ParameterDescriptorDto
    {
        public string DataType { get; internal set; }
        public object DefaultValue { get; internal set; }
        public string DisplayName { get; internal set; }
        public List<string> EnumValues { get; internal set; }
        public string FileFilter { get; internal set; }
        public bool IsEnum { get; internal set; }
        public bool IsRequired { get; internal set; }
        public string Id { get; internal set; }
    }
}
