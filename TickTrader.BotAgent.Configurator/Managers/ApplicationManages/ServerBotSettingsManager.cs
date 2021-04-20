using System;
using System.IO;
using System.IO.Compression;

namespace TickTrader.BotAgent.Configurator
{
    public class ServerBotSettingsManager
    {
        private const string RawFileName = "RawArchiveFolder";
        public const string ServerBotSettingsProperty = "ManagingBotSettingsOnServer";

        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly string[] _workingFolders = new string[3] { "AlgoData", "AlgoRepository", "server.config.xml" };

        private readonly RegistryManager _registry;

        public RegistryNode CurrentServer => _registry.CurrentServer;

        public string RawArchivePath => GetServerFolderPath(RawFileName);


        public ServerBotSettingsManager(RegistryManager registry)
        {
            _registry = registry;
        }

        public bool CreateAlgoServerBotSettingZip(string archiveFolderPath) => InitRawArchiveFolder(() =>
        {
            RemoveFile(archiveFolderPath);

            foreach (var key in _workingFolders)
            {
                var serverBotPath = GetServerFolderPath(key);
                var archiveBotPath = GetArchiveFolderPath(key);

                if (Directory.Exists(serverBotPath))
                    CopyAll(new DirectoryInfo(serverBotPath), InitEmptyDirectory(archiveBotPath));
                else
                    MoveFile(serverBotPath, archiveBotPath);
            }

            ZipFile.CreateFromDirectory(RawArchivePath, archiveFolderPath);
        });

        public bool LoadAlgoServerBotSettingZip(string archiveFolderPath) => InitRawArchiveFolder(() =>
        {
            ZipFile.ExtractToDirectory(archiveFolderPath, RawArchivePath);

            foreach (var key in _workingFolders)
            {
                var serverBotPath = GetServerFolderPath(key);
                var archiveBotPath = GetArchiveFolderPath(key);

                if (Directory.Exists(archiveBotPath))
                {
                    ClearOrRestoreFolder(serverBotPath);

                    foreach (var archiveSubFolder in new DirectoryInfo(archiveBotPath).GetDirectories())
                        CopyAll(archiveSubFolder, InitEmptyDirectory(Path.Combine(serverBotPath, archiveSubFolder.Name)));
                }
                else
                    MoveFile(archiveBotPath, serverBotPath);
            }
        });

        private bool InitRawArchiveFolder(Action archiveAction)
        {
            try
            {
                ClearOrRestoreFolder(RawArchivePath, true);

                archiveAction();
            }
            catch (Exception ex)
            {
                _logger.Error(ex);

                return false;
            }
            finally
            {
                ClearOrRestoreFolder(RawArchivePath);
            }

            return true;
        }

        private string GetArchiveFolderPath(string folder) => Path.Combine(RawArchivePath, folder);

        private string GetServerFolderPath(string folder) => Path.Combine(CurrentServer.FolderPath, folder);

        private static void MoveFile(string source, string target)
        {
            if (File.Exists(source))
            {
                if (File.Exists(target))
                    File.Delete(target);

                File.Copy(source, target);
            }
        }

        private static DirectoryInfo InitEmptyDirectory(string path)
        {
            ClearOrRestoreFolder(path, true);

            return new DirectoryInfo(path);
        }

        private static void RemoveFile(string filePath)
        {
            if (File.Exists(filePath))
                File.Delete(filePath);
        }

        private static void ClearOrRestoreFolder(string directoryPath, bool restore = false)
        {
            if (Directory.Exists(directoryPath))
                //sometimes Directory.Delete behaves incorretly. Throws a folder delete error
                ForceCleanFolder(new DirectoryInfo(directoryPath));

            if (restore)
                Directory.CreateDirectory(directoryPath);
        }

        private static void CopyAll(DirectoryInfo source, DirectoryInfo target)
        {
            foreach (var fileInfo in source.GetFiles())
                fileInfo.CopyTo(Path.Combine(target.FullName, fileInfo.Name), true);

            foreach (var subFolderInfo in source.GetDirectories())
                CopyAll(subFolderInfo, target.CreateSubdirectory(subFolderInfo.Name));

            if (target.GetFiles().Length + target.GetDirectories().Length == 0)
                Directory.Delete(target.FullName);
        }

        private static void ForceCleanFolder(DirectoryInfo directoryInfo)
        {
            foreach (var file in directoryInfo.GetFiles())
                file.Delete();

            foreach (var subDirectory in directoryInfo.GetDirectories())
            {
                ForceCleanFolder(subDirectory);
                subDirectory.Delete(true);
            }
        }
    }
}
