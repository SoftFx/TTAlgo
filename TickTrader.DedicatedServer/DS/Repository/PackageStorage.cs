using Microsoft.Extensions.Options;
using System.IO;
using System.Linq;
using System.Reflection;
using TickTrader.Algo.Core;
using TickTrader.DedicatedServer.Extensions;
using System;
using Microsoft.Extensions.Logging;
using TickTrader.DedicatedServer.DS.Models;
using TickTrader.DedicatedServer.DS.Exceptions;
using System.Threading;
using System.Collections.Generic;

namespace TickTrader.DedicatedServer.DS.Repository
{
    public class PackageStorage : IPackageStorage
    {
        private readonly string packageTemplate = "*.ttalgo";
        private readonly string _storageDir;
        private readonly ILogger<PackageStorage> _logger;
        private readonly ReaderWriterLockSlim _storageLock = new ReaderWriterLockSlim();
        private readonly Dictionary<string, PackageModel> _packages;

        public PackageStorage(IOptions<PackageStorageSettings> config, ILogger<PackageStorage> logger)
        {
            _logger = logger;
            _storageDir = GetFullPathToStorage(config.Value.Path);
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
                _packages.Add(package.Name, package);
            });
        }

        public PackageModel Add(byte[] packageContent, string packageName)
        {
            _storageLock.EnterWriteLock();
            try
            {
                Validate(packageName);

                EnsureStorageDirectoryCreated();

                var packageFileInfo = SavePackage(packageName, packageContent);
                var package = ReadPackage(packageFileInfo);
                _packages.Add(package.Name, package);

                return package;
            }
            finally
            {
                _storageLock.ExitWriteLock();
            }
        }

        public PackageModel[] GetAll()
        {
            _storageLock.EnterReadLock();
            try
            {
                return _packages.Select(x => x.Value).ToArray();
            }
            finally
            {
                _storageLock.ExitReadLock();
            }
        }

        public PackageModel Get(string name)
        {
            _storageLock.EnterReadLock();
            try
            {
                PackageModel package;
                return _packages.TryGetValue(name, out package) ? package : null;
            }
            finally
            {
                _storageLock.ExitReadLock();
            }
        }

        public void Remove(string packageName)
        {
            _storageLock.EnterWriteLock();
            try
            {
                PackageModel package;
                if (_packages.TryGetValue(packageName, out package))
                {
                    _packages.Remove(packageName);
                    try
                    {
                        package.Dispose();
                    }
                    catch
                    {
                        _logger.LogWarning($"Error when disposing package '{packageName}'");
                    }
                    try
                    {
                        File.Delete(Path.Combine(_storageDir, packageName));
                    }
                    catch
                    {
                        _logger.LogWarning($"Error when deleting file package '{packageName}'");
                    }
                }
            }
            finally
            {
                _storageLock.ExitWriteLock();
            }
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
                using (var container = PluginContainer.Load(fileInfo.FullName))
                {
                    return new PackageModel(fileInfo.Name, fileInfo.CreationTime, container);
                }
            }
            catch
            {
                _logger.LogWarning($"PACKAGE_STORAGE: Failed to read package {fileInfo.Name}");

                return new PackageModel(fileInfo.Name, fileInfo.CreationTime, null);
            }
        }
        private FileInfo SavePackage(string packageName, byte[] packageContent)
        {
            var packagePath = GetFullPathToPackage(packageName);
            File.WriteAllBytes(packagePath, packageContent);
            return new FileInfo(packagePath);

        }
        private void Validate(string packageName)
        {
            if (_packages.ContainsKey(packageName))
                throw new DuplicatePackageException($"Package with the same name '{packageName}' already exists");
        }

        #endregion
    }
}
