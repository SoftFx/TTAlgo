﻿using ActorSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Channels;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Util;

namespace TickTrader.Algo.Package
{
    public class PackageStorage
    {
        private static readonly IAlgoLogger _logger = AlgoLoggerFactory.GetLogger<PackageStorage>();

        private readonly Ref<Impl> _impl;


        public event Action<PackageUpdate> PackageUpdated;


        public PackageStorage()
        {
            _impl = Actor.SpawnLocal<Impl>(null, nameof(PackageStorage));

            _impl.Send(a => a.Init());

            _impl.Send(a => Task.Factory.StartNew(() => SendPackageUpdates(a.PackageUpdates.Reader)));
        }

        public Task RegisterRepositoryLocation(string locationId, string path)
        {
            return _impl.Call(a => a.RegisterRepository(locationId, path));
        }

        public Task RegisterAssemblyAsPackage(Assembly assembly)
        {
            return _impl.Call(a => a.RegisterAssembly(assembly));
        }

        public Task<AlgoPackageRef> GetPackageRef(string pkgId)
        {
            return _impl.Call(a => a.GetPackageRef(pkgId));
        }

        public Task Stop()
        {
            return _impl.Call(a => a.Stop());
        }

        public Task WaitLoaded()
        {
            var taskSrc = new TaskCompletionSource<object>();
            _impl.Send(a => a.WhenLoaded(() => taskSrc.TrySetResult(null)));
            return taskSrc.Task;
        }


        private async Task SendPackageUpdates(ChannelReader<PackageUpdate> reader)
        {
            await Task.Yield();

            while (true)
            {
                var hasData = await reader.WaitToReadAsync();
                if (!hasData)
                    break;

                for (var i = 0; i < 10 && reader.TryRead(out var update); i++)
                {
                    try
                    {
                        PackageUpdated?.Invoke(update);
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex, $"Failed to dispatch PackageUpdate");
                    }
                }
            }
        }


        private class Impl : Actor
        {
            private readonly Dictionary<string, PackageRepository> _repositories = new Dictionary<string, PackageRepository>();
            private readonly AsyncChannelProcessor<PackageFileUpdate> _fileUpdateProcessor = AsyncChannelProcessor<PackageFileUpdate>.CreateUnbounded($"{nameof(PackageStorage)} package loop", true);
            private readonly Dictionary<string, int> _packageVersions = new Dictionary<string, int>();
            private readonly Dictionary<string, AlgoPackageRef> _packageMap = new Dictionary<string, AlgoPackageRef>();


            public Channel<PackageUpdate> PackageUpdates { get; private set; }


            public void Init()
            {
                _fileUpdateProcessor.Start(HandlePackageUpdate);

                PackageUpdates = Channel.CreateUnbounded<PackageUpdate>(new UnboundedChannelOptions { AllowSynchronousContinuations = false, SingleReader = true, SingleWriter = true });
            }

            public async Task Stop()
            {
                _logger.Debug("Stopping...");

                await _fileUpdateProcessor.Stop(false);
                _logger.Debug("File update processor stopped");
                PackageUpdates.Writer.TryComplete();
                _logger.Debug("Marked update stream complete");
                await Task.WhenAll(_repositories.Values.Select(r => r.Stop()));
                _logger.Debug("Package repositories stopped");

                _logger.Debug("Stopped");
            }

            public void WhenLoaded(Action continuation)
            {
                Task.WhenAll(_repositories.Values.Select(r => r.WaitLoaded()))
                    .ContinueWith(t => continuation());
            }

            public void RegisterRepository(string locationId, string path)
            {
                if (_repositories.ContainsKey(locationId))
                    throw new ArgumentException($"Cannot register multiple paths for location '{locationId}'");

                var repo = new PackageRepository(path, locationId, _fileUpdateProcessor);
                _repositories[locationId] = repo;
                repo.Start(); // fire and forget
            }

            public void RegisterAssembly(Assembly assembly)
            {
                var assemblyPath = assembly.Location;
                var pkgId = PackageId.FromPath(SharedConstants.EmbeddedRepositoryId, assembly.Location);

                var pkgInfo = PackageExplorer.ScanAssembly(pkgId, assembly);

                pkgInfo.Identity = PackageIdentity.CreateInvalid(assemblyPath);
                try
                {
                    var fileInfo = new FileInfo(assemblyPath);
                    pkgInfo.Identity = PackageIdentity.Create(fileInfo, assembly.FullName);
                    pkgInfo.IsValid = true;
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, $"Failed to get assembly file info '{assemblyPath}'");
                }

                HandlePackageUpdate(new PackageFileUpdate(pkgId, UpdateAction.Upsert, pkgInfo, null));
            }

            public AlgoPackageRef GetPackageRef(string pkgId)
            {
                _packageMap.TryGetValue(pkgId, out var pkgRef);
                pkgRef?.IncrementRef();
                return pkgRef;
            }


            private void HandlePackageUpdate(PackageFileUpdate update)
            {
                var pkgId = update.PkgId;
                if (_packageMap.TryGetValue(pkgId, out var pkgRef))
                    pkgRef?.SetObsolete();

                switch (update.Action)
                {
                    case UpdateAction.Upsert:
                        var refId = GeneratePackageRefId(pkgId);
                        _packageMap[pkgId] = new AlgoPackageRef(refId, update.PkgInfo, update.PkgBytes);
                        PackageUpdates.Writer.TryWrite(new PackageUpdate { Action = Domain.Package.Types.UpdateAction.Upsert, Id = pkgId, Package = update.PkgInfo, });
                        break;
                    case UpdateAction.Remove:
                        _packageMap.Remove(pkgId);
                        PackageUpdates.Writer.TryWrite(new PackageUpdate { Action = Domain.Package.Types.UpdateAction.Removed, Id = pkgId, });
                        break;
                }
            }

            private string GeneratePackageRefId(string packageId)
            {
                if (!_packageVersions.TryGetValue(packageId, out var currentVersion))
                    currentVersion = -1;

                currentVersion++;
                _packageVersions[packageId] = currentVersion;
                return $"{packageId}/{currentVersion}";
            }
        }
    }
}
