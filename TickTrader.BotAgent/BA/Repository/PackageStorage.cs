using NLog;
using System;
using System.IO;
using System.Reflection;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Domain.ServerControl;
using TickTrader.Algo.Package;
using TickTrader.Algo.Server;
using TickTrader.BotAgent.BA.Models;
using TickTrader.BotAgent.Extensions;

namespace TickTrader.BotAgent.BA.Repository
{
    public class PackageStorage
    {
        private static ILogger _logger = LogManager.GetLogger(nameof(ServerModel));


        private readonly string _storageDir;

        private ReductionCollection _reductions;


        public LocalAlgoLibrary Library { get; }

        public MappingCollectionInfo Mappings { get; }


        public event Action<PackageInfo, ChangeAction> PackageChanged;
        public event Action<PackageInfo> PackageStateChanged;


        public PackageStorage(AlgoServer algoServer)
        {
            _storageDir = GetFullPathToStorage("AlgoRepository");

            EnsureStorageDirectoryCreated();

            algoServer.PackageStorage.RegisterRepositoryLocation(SharedConstants.LocalRepositoryId, _storageDir, true);
            Library = new LocalAlgoLibrary(algoServer.PackageStorage);

            _reductions = new ReductionCollection();
            _reductions.LoadDefaultReductions();
            Mappings = _reductions.CreateMappings();
        }


        public static string GetPackageId(string packageName)
        {
            return PackageId.Pack(SharedConstants.LocalRepositoryId, packageName);
        }

        public AlgoPackageRef GetPackageRef(string packageId)
        {
            return Library.GetPackageRef(packageId);
        }


        #region Private Methods

        private void EnsureStorageDirectoryCreated()
        {
            if (!Directory.Exists(_storageDir))
            {
                var dinfo = Directory.CreateDirectory(_storageDir);
            }
        }

        private string GetFullPathToStorage(string path)
        {
            return path.IsPathAbsolute() ? path :
              Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), path);
        }

        private string GetFullPathToPackage(string fileName)
        {
            return Path.Combine(_storageDir, fileName);
        }

        #endregion
    }
}
