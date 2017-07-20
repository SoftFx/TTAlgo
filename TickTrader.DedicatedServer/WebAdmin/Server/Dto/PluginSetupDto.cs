using System;

namespace TickTrader.DedicatedServer.WebAdmin.Server.Dto
{
    public class PluginSetupDto
    {
        public string PackageName { get; set; }
        public string PluginId { get; set; }
        public string InstanceId { get; set; }
        public string Symbol { get; set; }
        public bool Isolated { get; set; }
        public PermissionsDto Permissions { get; set; }
        public bool TradeIs { get; set; }
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
