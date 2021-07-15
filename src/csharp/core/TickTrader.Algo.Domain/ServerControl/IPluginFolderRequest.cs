namespace TickTrader.Algo.Domain.ServerControl
{
    public interface IPluginFolderRequest
    {
        string PluginId { get; }

        PluginFolderInfo.Types.PluginFolderId FolderId { get; }
    }


    public partial class PluginFolderInfoRequest : IPluginFolderRequest { }

    public partial class ClearPluginFolderRequest : IPluginFolderRequest { }

    public partial class DeletePluginFileRequest : IPluginFolderRequest { }

    public partial class DownloadPluginFileRequest : IPluginFolderRequest { }

    public partial class UploadPluginFileRequest : IPluginFolderRequest { }
}
