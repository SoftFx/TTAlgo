using System.IO;
using System.Reflection;
using TickTrader.Algo.Core;
using TickTrader.BotAgent.Extensions;
using TickTrader.BotAgent.BA.Models;
using System;
using TickTrader.BotAgent.BA.Exceptions;
using NLog;
using TickTrader.Algo.Common.Model;
using TickTrader.Algo.Common.Model.Library;
using TickTrader.Algo.Common.Info;
using TickTrader.Algo.Core.Repository;
using System.Collections.Generic;
using System.Linq;

namespace TickTrader.BotAgent.BA.Repository
{
    public class PackageStorage
    {
        private static ILogger _logger = LogManager.GetLogger(nameof(ServerModel));


        private readonly string _storageDir;

        private ReductionCollection _reductions;


        public LocalAlgoLibrary Library { get; }

        public MappingCollection Mappings { get; }


        public event Action<PackageInfo, ChangeAction> PackageChanged;


        public PackageStorage()
        {
            _storageDir = GetFullPathToStorage("AlgoRepository");

            EnsureStorageDirectoryCreated();

            Library = new LocalAlgoLibrary(CoreLoggerFactory.GetLogger("AlgoRepository"));
            Library.RegisterRepositoryLocation(RepositoryLocation.LocalRepository, _storageDir, true);
            Library.PackageUpdated += LibraryOnPackageUpdated;

            _reductions = new ReductionCollection(CoreLoggerFactory.GetLogger("Extensions"));
            _reductions.AddAssembly("TickTrader.Algo.Ext");
            Mappings = new MappingCollection(_reductions);
        }


        public static PackageKey GetPackageKey(string packageName)
        {
            return new PackageKey(packageName.ToLower(), RepositoryLocation.LocalRepository);
        }


        public void Update(byte[] packageContent, string packageName)
        {
            EnsureStorageDirectoryCreated();

            var packageRef = Library.GetPackageRef(GetPackageKey(packageName));
            if (packageRef != null)
            {
                if (packageRef.IsLocked)
                    throw new PackageLockedException($"Cannot update package '{packageName}': one or more trade bots from this package is being executed! Please stop all bots and try again!");

                RemovePackage(packageRef);
            }

            SavePackage(packageName, packageContent);
        }

        public byte[] GetPackageBinary(PackageKey package)
        {
            var packageRef = Library.GetPackageRef(package);
            if (packageRef == null)
                throw new ArgumentException("Package not found");

            return File.ReadAllBytes(packageRef.Identity.FilePath);
        }

        public AlgoPackageRef GetPackageRef(string packageName)
        {
            return GetPackageRef(GetPackageKey(packageName));
        }

        public AlgoPackageRef GetPackageRef(PackageKey packageKey)
        {
            return Library.GetPackageRef(packageKey);
        }

        public void Remove(string packageName)
        {
            Remove(GetPackageKey(packageName));
        }

        public void Remove(PackageKey packageKey)
        {
            var packageRef = Library.GetPackageRef(packageKey);
            if (packageRef != null)
            {
                RemovePackage(packageRef);
            }
        }

        public Stream GetPackageReadStream(PackageKey package)
        {
            var packageRef = Library.GetPackageRef(package);
            if (packageRef == null)
                throw new ArgumentException("Package not found");

            return File.OpenRead(packageRef.Identity.FilePath);
        }

        public Stream GetPackageWriteStream(PackageKey package)
        {
            if (package.Location != RepositoryLocation.LocalRepository)
                throw new ArgumentException($"Package location '{package.Location}' is not defined");

            EnsureStorageDirectoryCreated();

            var packageRef = Library.GetPackageRef(GetPackageKey(package.Name));
            if (packageRef != null)
            {
                if (packageRef.IsLocked)
                    throw new PackageLockedException($"Cannot update package '{package.Name}': one or more trade bots from this package is being executed! Please stop all bots and try again!");

                RemovePackage(packageRef);
            }

            return File.OpenWrite(GetFullPathToPackage(package.Name));
        }


        #region Private Methods

        private static void CheckLock(AlgoPackageRef package)
        {
            if (package.IsLocked)
                throw new PackageLockedException("Cannot remove package: one or more trade robots from this package is being executed! Please stop all robots and try again!");
        }

        private void RemovePackage(AlgoPackageRef package)
        {
            CheckLock(package);

            try
            {
                File.Delete(Path.Combine(_storageDir, package.Name));
            }
            catch
            {
                _logger.Warn($"Error deleting file package '{package.Name}'");
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
                _logger.Warn($"Error saving file package '{packageName}'");
                throw;
            }
        }

        private void LibraryOnPackageUpdated(UpdateInfo<PackageInfo> update)
        {
            PackageChanged?.Invoke(update.Value, update.Type.Convert());
        }

        #endregion
    }
}
