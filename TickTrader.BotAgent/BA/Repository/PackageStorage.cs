using System.IO;
using System.Linq;
using System.Reflection;
using TickTrader.Algo.Core;
using TickTrader.BotAgent.Extensions;
using TickTrader.BotAgent.BA.Models;
using System.Collections.Generic;
using System;
using TickTrader.BotAgent.BA.Exceptions;
using NLog;

namespace TickTrader.BotAgent.BA.Repository
{
    public class PackageStorage
    {
        private static ILogger _logger = LogManager.GetLogger(nameof(ServerModel));

        private readonly string packageTemplate = "*.ttalgo";
        private readonly string _storageDir;
        private readonly Dictionary<string, PackageModel> _packages;

        public PackageStorage()
        {
            _storageDir = GetFullPathToStorage("AlgoRepository/");
            _packages = new Dictionary<string, PackageModel>();

            EnsureStorageDirectoryCreated();
            InitStorage();
        }

        public IEnumerable<PackageModel> Packages => _packages.Values;

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
            //Validate(packageName);

            EnsureStorageDirectoryCreated();

            var key = GetPackageKey(packageName);
            var existing = _packages.GetOrDefault(key);
            if (existing != null)
            {
                if (existing.IsLocked)
                    throw new PackageLockedException($"Cannot update package '{packageName}': one or more trade bots from this package is being executed! Please stop all bots and try again!");

                RemovePackage(existing);
            }

            var packageFileInfo = SavePackage(packageName, packageContent);
            var package = ReadPackage(packageFileInfo);
            _packages.Add(key, package);

            if (existing == null)
                PackageChanged?.Invoke(package, ChangeAction.Added);
            else
                PackageChanged?.Invoke(package, ChangeAction.Modified);

            return package;
        }

        public PackageModel Get(string name)
        {
            return GetByName(name);
        }

        public void Replace()
        {
        }

        public void Remove(string packageName)
        {
            PackageModel package;
            if (_packages.TryGetValue(GetPackageKey(packageName), out package))
            {
                RemovePackage(package);
                PackageChanged?.Invoke(package, ChangeAction.Removed);
            }
        }

        public event Action<PackageModel, ChangeAction> PackageChanged;

        #region Private Methods
        private bool TryDisposePackage(PackageModel package)
        {
            try
            {
                package.Dispose();
                return true;
            }
            catch
            {
                _logger.Warn($"Error disposing package '{package.Name}'");
                return false;
            }
        }

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
                _logger.Warn($"Error deleting file package '{package.Name}'");
                throw;
            }

            try
            {
                package.Dispose();
            }
            catch
            {
                _logger.Warn($"Error disposing package '{package.Name}'");
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

        private PackageModel ReadPackage(FileInfo fileInfo)
        {
            try
            {
                var container = PluginContainer.Load(fileInfo.FullName);
                return new PackageModel(fileInfo.Name, fileInfo.LastWriteTime, container);
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to read package {fileInfo.Name}: {ex}");

                return new PackageModel(fileInfo.Name, fileInfo.LastWriteTime, null);
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
