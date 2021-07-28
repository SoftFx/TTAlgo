namespace TickTrader.Algo.Server.PublicAPI
{
    public partial class FileTransferSettings
    {
        public const int DefaultSize = 512 * 1024;

        public static FileTransferSettings Default => new FileTransferSettings(DefaultSize, 0);


        public FileTransferSettings(int chunkSize, int chunkOffset)
        {
            ChunkSize = chunkSize;
            ChunkOffset = chunkOffset;
        }
    }


    public partial class UploadPackageRequest
    {
        public UploadPackageRequest(string packageId, string filename)
            : this(packageId, filename, FileTransferSettings.Default)
        {
        }

        public UploadPackageRequest(string packageId, string filename, FileTransferSettings transferSettings)
        {
            PackageId = packageId;
            Filename = filename;
            TransferSettings = transferSettings;
        }
    }


    public partial class RemovePackageRequest
    {
        public RemovePackageRequest(string packageId)
        {
            PackageId = packageId;
        }
    }


    public partial class DownloadPackageRequest
    {
        public DownloadPackageRequest(string packageId)
            : this(packageId, FileTransferSettings.Default)
        {
        }

        public DownloadPackageRequest(string packageId, FileTransferSettings transferSettings)
        {
            PackageId = packageId;
            TransferSettings = transferSettings;
        }
    }


    public partial class PluginFolderInfoRequest
    {
        public PluginFolderInfoRequest(string pluginId, PluginFolderInfo.Types.PluginFolderId folderId)
        {
            PluginId = pluginId;
            FolderId = folderId;
        }
    }


    public partial class ClearPluginFolderRequest
    {
        public ClearPluginFolderRequest(string pluginId, PluginFolderInfo.Types.PluginFolderId folderId)
        {
            PluginId = pluginId;
            FolderId = folderId;
        }
    }


    public partial class DeletePluginFileRequest
    {
        public DeletePluginFileRequest(string pluginId, PluginFolderInfo.Types.PluginFolderId folderId, string fileName)
        {
            PluginId = pluginId;
            FolderId = folderId;
            FileName = fileName;
        }
    }


    public partial class DownloadPluginFileRequest
    {
        public DownloadPluginFileRequest(string pluginId, PluginFolderInfo.Types.PluginFolderId folderId, string fileName)
            : this(pluginId, folderId, fileName, FileTransferSettings.Default)
        {
        }

        public DownloadPluginFileRequest(string pluginId, PluginFolderInfo.Types.PluginFolderId folderId, string fileName, FileTransferSettings transferSettings)
        {
            PluginId = pluginId;
            FolderId = folderId;
            FileName = fileName;
            TransferSettings = transferSettings;
        }
    }


    public partial class UploadPluginFileRequest
    {
        public UploadPluginFileRequest(string pluginId, PluginFolderInfo.Types.PluginFolderId folderId, string fileName)
            : this(pluginId, folderId, fileName, FileTransferSettings.Default)
        {
        }

        public UploadPluginFileRequest(string pluginId, PluginFolderInfo.Types.PluginFolderId folderId, string fileName, FileTransferSettings transferSettings)
        {
            PluginId = pluginId;
            FolderId = folderId;
            FileName = fileName;
            TransferSettings = transferSettings;
        }
    }
}
