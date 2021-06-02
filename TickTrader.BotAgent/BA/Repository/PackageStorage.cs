using NLog;
using System;
using System.IO;
using System.Reflection;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Domain.ServerControl;
using TickTrader.Algo.Package;
using TickTrader.Algo.Server;
using TickTrader.BotAgent.BA.Exceptions;
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

            Library = new LocalAlgoLibrary(algoServer.PackageStorage);
            Library.RegisterRepositoryLocation(SharedConstants.LocalRepositoryId, _storageDir, true);
            Library.PackageUpdated += LibraryOnPackageUpdated;
            Library.PackageStateChanged += LibraryOnPackageStateChanged;

            _reductions = new ReductionCollection();
            _reductions.LoadDefaultReductions();
            Mappings = _reductions.CreateMappings();
        }


        public static string GetPackageId(string packageName)
        {
            return PackageId.Pack(SharedConstants.LocalRepositoryId, packageName);
        }


        public void Update(byte[] packageContent, string packageName)
        {
            EnsureStorageDirectoryCreated();

            var packageRef = Library.GetPackageRef(GetPackageId(packageName));
            if (packageRef != null)
            {
                if (packageRef.IsLocked)
                    throw new PackageLockedException($"Cannot update Algo package '{packageName}': one or more trade bots from this package is being executed! Please stop all bots and try again!");

                RemovePackage(packageRef);
            }

            SavePackage(packageName, packageContent);
        }

        public byte[] GetPackageBinary(string packageId)
        {
            var packageRef = Library.GetPackageRef(packageId);
            if (packageRef == null)
                throw new ArgumentException("Algo package not found");

            return File.ReadAllBytes(packageRef.Identity.FilePath);
        }

        public AlgoPackageRef GetPackageRef(string packageId)
        {
            return Library.GetPackageRef(packageId);
        }

        public void Remove(string packageId)
        {
            var packageRef = Library.GetPackageRef(packageId);
            if (packageRef != null)
            {
                RemovePackage(packageRef);
            }
        }

        public string GetPackageReadPath(DownloadPackageRequest request)
        {
            var packageRef = Library.GetPackageRef(request.PackageId);
            if (packageRef == null)
                throw new ArgumentException("Algo package not found");

            return packageRef.Identity.FilePath;
        }

        public string GetPackageWritePath(UploadPackageRequest request)
        {
            if (!string.IsNullOrEmpty(request.PackageId) && !string.IsNullOrEmpty(request.Filename))
                throw new ArgumentException($"Both {nameof(request.PackageId)} and {nameof(request.Filename)} can't be specified");

            EnsureStorageDirectoryCreated();

            if (!string.IsNullOrEmpty(request.PackageId)) // update package
            {
                var packageId = request.PackageId;
                var packageRef = Library.GetPackageRef(packageId);

                if (packageRef == null)
                    throw new ArgumentException($"Package '{packageId}' doesn't exist");
                if (packageRef.IsLocked)
                    throw new PackageLockedException($"Cannot update Algo package '{packageId}': one or more trade bots from this package is being executed! Please stop all bots and try again!");

                return packageRef?.Identity.FilePath;
            }

            if (!string.IsNullOrEmpty(request.Filename))
            {
                var packageId = PackageId.Pack(SharedConstants.LocalRepositoryId, request.Filename);
                var packageRef = Library.GetPackageRef(packageId);

                if (packageRef != null)
                {
                    if (packageRef.IsLocked)
                        throw new PackageLockedException($"Cannot update Algo package '{packageId}': one or more trade bots from this package is being executed! Please stop all bots and try again!");

                    return packageRef.Identity.FilePath; // update package
                }

                return GetFullPathToPackage(request.Filename); // add packa
            }

            throw new ArgumentException($"{nameof(request.PackageId)} or {nameof(request.Filename)} should be specified");
        }


        #region Private Methods

        private static void CheckLock(AlgoPackageRef package)
        {
            if (package.IsLocked)
                throw new PackageLockedException("Cannot remove Algo package: one or more trade robots from this package is being executed! Please stop all robots and try again!");
        }

        private void RemovePackage(AlgoPackageRef package)
        {
            CheckLock(package);

            try
            {
                File.Delete(package.Identity.FilePath);
            }
            catch
            {
                _logger.Warn($"Error deleting file Algo package '{package.Id}'");
                throw;
            }
        }

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

        private void SavePackage(string packageName, byte[] packageContent)
        {
            try
            {
                File.WriteAllBytes(GetFullPathToPackage(packageName), packageContent);
            }
            catch
            {
                _logger.Warn($"Error saving file Algo package '{packageName}'");
                throw;
            }
        }

        private void LibraryOnPackageUpdated(UpdateInfo<PackageInfo> update)
        {
            PackageChanged?.Invoke(update.Value, update.Type.Convert());
        }

        private void LibraryOnPackageStateChanged(PackageInfo package)
        {
            PackageStateChanged?.Invoke(package);
        }

        #endregion
    }
}
