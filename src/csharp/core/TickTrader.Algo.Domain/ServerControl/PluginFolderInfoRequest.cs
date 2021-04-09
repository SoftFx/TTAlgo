namespace TickTrader.Algo.Domain.ServerControl
{
    public partial class PluginFolderInfoRequest
    {
        public PluginFolderInfoRequest(string pluginId, PluginFolderInfo.Types.PluginFolderId folderId)
        {
            PluginId = pluginId;
            FolderId = folderId;
        }
    }
}
