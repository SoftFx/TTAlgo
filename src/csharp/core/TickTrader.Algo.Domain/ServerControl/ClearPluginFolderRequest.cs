namespace TickTrader.Algo.Domain.ServerControl
{
    public partial class ClearPluginFolderRequest
    {
        public ClearPluginFolderRequest(string pluginId, PluginFolderInfo.Types.PluginFolderId folderId)
        {
            PluginId = pluginId;
            FolderId = folderId;
        }
    }
}
