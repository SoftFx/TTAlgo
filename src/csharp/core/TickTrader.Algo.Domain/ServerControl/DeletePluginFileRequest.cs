namespace TickTrader.Algo.Domain.ServerControl
{
    public partial class DeletePluginFileRequest
    {
        public DeletePluginFileRequest(string pluginId, PluginFolderInfo.Types.PluginFolderId folderId, string fileName)
        {
            PluginId = pluginId;
            FolderId = folderId;
            FileName = fileName;
        }
    }
}
