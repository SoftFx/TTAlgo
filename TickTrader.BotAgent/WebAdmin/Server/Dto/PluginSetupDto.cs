using System;

namespace TickTrader.BotAgent.WebAdmin.Server.Dto
{
    public class PluginSetupDto
    {
        public string PackageId { get; set; }
        public string PluginId { get; set; }
        public string InstanceId { get; set; }
        public string Symbol { get; set; }
        public PermissionsDto Permissions { get; set; }
        public AccountDto Account { get; set; }
        public PluginSetupParameter[] Parameters { get; set; }
    }

    public class PluginSetupParameter
    {
        public string Id { get; set; }
        public Object Value { get; set; }
        public string DataType { get; set; }
    }
}
