using Microsoft.Extensions.Options;
using System.IO;
using System.Linq;
using System.Reflection;
using TickTrader.Algo.Core;
using TickTrader.DedicatedServer.Extensions;
using Microsoft.Extensions.Logging;
using TickTrader.DedicatedServer.DS.Models;
using System.Threading;
using System.Collections.Generic;
using System;
using TickTrader.DedicatedServer.DS.Exceptions;

namespace TickTrader.DedicatedServer.DS.Repository
{
    public class PackageStorage
    {
        private readonly string packageTemplate = "*.ttalgo";
        private readonly string _storageDir;
        private readonly ILogger<PackageStorage> _logger;
        private readonly object _syncObj;
        private readonly Dictionary<string, PackageModel> _packages;

        public PackageStorage(ILoggerFactory loggerFactory, object syncObj)
        {
            _syncObj = syncObj;
            _logger = loggerFactory.CreateLogger<PackageStorage>();
            _storageDir = GetFullPathToStorage("AlgoRepository/");
            _packages = new Dictionary<string, PackageModel>();

            EnsureStorageDirectoryCreated();
            InitStorage();
        }

        private void InitStorage()
        {
            var storageDir = new DirectoryInfo(_storageDir);
            var files = storageDir.GetFiles(packageTemplate);
            files.AsParallel().ForAll(f =>
            {
                var package = ReadPackage(f);
                lock (_packages)
                    _packages.Add(GetPackageKey(package.Name), package);
            });
        }

        public PackageModel Update(byte[] packageContent, string packageName)
        {
            lock (_syncObj)
            {
                //Validate(packageName);

                EnsureStorageDirectoryCreated();

                var key = GetPackageKey(packageName);
                var existing = _packages.GetOrDefault(key);
                if (existing != null)
                    RemovePackage(existing);

                var packageFileInfo = SavePackage(packageName, packageContent);
                var package = ReadPackage(packageFileInfo);
                _packages.Add(key, package);

                if (existing == null)
                    PackageChanged?.Invoke(package, ChangeAction.Added);

                return package;
            }
        }

        public PackageModel[] GetAll()
        {
            lock (_syncObj)
                return _packages.Values.ToArray();
        }

        public PackageModel Get(string name)
        {
            lock (_syncObj) return GetByName(name);
        }

        public void Replace()
        {
        }

        public void Remove(string packageName)
        {
            lock (_syncObj)
            {
                PackageModel package;
                if (_packages.TryGetValue(GetPackageKey(packageName), out package))
                {
                    RemovePackage(package);
                    PackageChanged?.Invoke(package, ChangeAction.Removed);
                }
            }
        }

        public event Action<IPackage, ChangeAction> PackageChanged;

        #region Private Methods

        private static void CheckLock(PackageModel package)
        {
            if (package.IsLocked)
                throw new PackageLockedException("Cannot remove package: one or more trade robots from this package is being executed! Please stop all robots and try again!");
        }

        private PackageModel GetByName(string name)
        {
            return _packages.GetOrDefault(GetPackageKey(name));
        }

        public static string GetPackageKey(string packageName)
        {
            return packageName.ToLower();
        }

        private void RemovePackage(PackageModel package)
        {
            CheckLock(package);

            try
            {
                File.Delete(Path.Combine(_storageDir, package.Name));
                _packages.Remove(GetPackageKey(package.Name));
            }
            catch
            {
                _logger.LogWarning($"Error deleting file package '{package.Name}'");
                throw;
            }

            try
            {
                package.Dispose();
            }
            catch
            {
                _logger.LogWarning($"Error disposing package '{package.Name}'");
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
            return PathExtensions.IsPathAbsolute(path) ? path :
              Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), path);
        }

        private string GetFullPathToPackage(string fileName)
        {
            return Path.Combine(_storageDir, fileName);
        }

        private PackageModel ReadPackage(FileInfo fileInfo)
        {
            try
            {
                var container = PluginContainer.Load(fileInfo.FullName);
                return new PackageModel(fileInfo.Name, fileInfo.CreationTime, container);
            }
            catch (Exception ex)
            {
                //_logger.LogWarning($"PACKAGE_STORAGE: Failed to read package {fileInfo.Name}");
                _logger.LogError($"Failed to read package {fileInfo.Name}: {ex}");

                return new PackageModel(fileInfo.Name, fileInfo.CreationTime, null);
            }
        }

        private FileInfo SavePackage(string packageName, byte[] packageContent)
        {
            var packagePath = GetFullPathToPackage(packageName);
            File.WriteAllBytes(packagePath, packageContent);
            return new FileInfo(packagePath);
        }

        //private void Validate(string packageName)
        //{
        //    if (_packages.ContainsKey(GetPackageKey(packageName)))
        //        throw new DuplicatePackageException($"Package with the same name '{packageName}' already exists");
        //}

        #endregion
    }
}
