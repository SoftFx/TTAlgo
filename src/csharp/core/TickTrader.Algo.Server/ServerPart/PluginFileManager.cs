using System;
using System.IO;
using System.Linq;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Domain.ServerControl;

namespace TickTrader.Algo.Server
{
    internal class PluginFileManager
    {
        private static readonly IAlgoLogger _logger = AlgoLoggerFactory.GetLogger<PluginFileManager>();

        private readonly AlgoServerPrivate _server;


        public PluginFileManager(AlgoServerPrivate server)
        {
            _server = server;
        }


        public PluginFolderInfo GetFolderInfo(PluginFolderInfoRequest request)
        {
            var path = GetFolderPath(request);

            var res = new PluginFolderInfo
            {
                PluginId = request.PluginId,
                FolderId = request.FolderId,
                Path = path,
            };

            if (Directory.Exists(path))
            {
                var dInfo = new DirectoryInfo(path);
                res.Files.AddRange(dInfo.GetFiles().Select(fInfo => new PluginFileInfo { Name = fInfo.Name, Size = fInfo.Length }));
            }

            return res;
        }

        public void ClearFolder(ClearPluginFolderRequest request)
        {
            var path = GetFolderPath(request);

            if (Directory.Exists(path))
            {
                try
                {
                    var dInfo = new DirectoryInfo(path);
                    dInfo.Delete(true);

                    Directory.CreateDirectory(path); //restore base folder
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, $"Failed to clear directory {path}");
                    throw;
                }
            }
        }

        public void DeleteFile(DeletePluginFileRequest request)
        {
            var path = GetFolderPath(request);

            File.Delete(Path.Combine(path, request.FileName));
        }

        public string GetFileReadPath(DownloadPluginFileRequest request)
        {
            var fileName = request.FileName;
            if (!PathHelper.IsFileNameValid(fileName))
                throw Errors.PluginFileIncorrectName(fileName);

            var path = GetFolderPath(request);
            var filePath = Path.Combine(path, fileName);
            if (!File.Exists(filePath))
                throw Errors.PluginFileNotFound(fileName);

            return filePath;
        }

        public string GetFileWritePath(UploadPluginFileRequest request)
        {
            if (request.FolderId == PluginFolderInfo.Types.PluginFolderId.BotLogs)
                throw Errors.PluginLogsFolderUploadForbidden();

            var fileName = request.FileName;
            if (!PathHelper.IsFileNameValid(fileName))
                throw Errors.PluginFileIncorrectName(fileName);

            var path = GetFolderPath(request);
            return Path.Combine(path, fileName);
        }


        private string GetFolderPath(IPluginFolderRequest request)
        {
            var pluginId = request.PluginId;
            return request.FolderId == PluginFolderInfo.Types.PluginFolderId.BotLogs
                ? _server.Env.GetPluginLogsFolder(pluginId)
                : _server.Env.GetPluginWorkingFolder(pluginId);
        }
    }
}
