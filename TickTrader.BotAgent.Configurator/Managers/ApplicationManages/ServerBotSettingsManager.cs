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

        private readonly string[] _archiveFolders = new string[] { "AlgoData", "AlgoRepository", "Settings" };
        private readonly string[] _archiveFiles = new string[] { "server.config.xml" };

        private readonly RegistryManager _registry;
        private readonly DirectoryInfo _rawFolderInfo;


        public RegistryNode CurrentServer => _registry.CurrentServer;

        private string BackupArchivePath => Path.Combine(ConfiguratorManager.BackupFolder, CurrentServer.BackupArchiveName);


        public ServerBotSettingsManager(RegistryManager registry)
        {
            _registry = registry;
            _rawFolderInfo = new DirectoryInfo(ServerPath(RawFileName));
        }


        public bool CreateAlgoServerSnapshot(string archiveFolderPath) => InitRawArchiveFolder(() =>
        {
            File.Delete(archiveFolderPath); //delete archive this the same name if that exist

            MoveArchiveFolders(_archiveFolders, ServerPath, ArchivePath);
            MoveArchiveFiles(_archiveFiles, ServerPath, ArchivePath);

            ZipFile.CreateFromDirectory(_rawFolderInfo.FullName, archiveFolderPath);
        });

        public bool LoadAlgoServerShapshot(string archiveFolderPath)
        {
            bool result = CreateAlgoServerSnapshot(BackupArchivePath);

            return result & InitRawArchiveFolder(() => RestoreArchiveShapshot(archiveFolderPath));
        }

        private void RestoreArchiveShapshot(string archivePath)
        {
            ZipFile.ExtractToDirectory(archivePath, _rawFolderInfo.FullName);

            MoveArchiveFolders(_archiveFolders, ArchivePath, ServerPath);
            MoveArchiveFiles(_archiveFiles, ArchivePath, ServerPath);
        }

        private static DirectoryInfo FolderInfo(string folder) => new DirectoryInfo(folder);

        private string ArchivePath(string path) => Path.Combine(_rawFolderInfo.FullName, path);

        private string ServerPath(string path) => Path.Combine(CurrentServer.FolderPath, path);


        private static void MoveArchiveFolders(string[] archiveFolders, Func<string, string> soursePath, Func<string, string> targetPath)
        {
            foreach (var folder in archiveFolders)
                FolderInfo(soursePath(folder)).MergeTo(FolderInfo(targetPath(folder)));
        }

        private static void MoveArchiveFiles(string[] archiveFiles, Func<string, string> soursePath, Func<string, string> targetPath)
        {
            foreach (var file in archiveFiles)
                MoveFile(soursePath(file), targetPath(file));
        }

        private static void MoveFile(string source, string target)
        {
            if (File.Exists(source))
                File.Copy(source, target, overwrite: true);
        }

        private bool InitRawArchiveFolder(Action archiveAction)
        {
            try
            {
                _rawFolderInfo.Clear();

                archiveAction();
            }
            catch (Exception ex)
            {
                _logger.Error(ex);

                return false;
            }
            finally
            {
                _rawFolderInfo.CheckAndDelete();
            }

            return true;
        }
    }
}
