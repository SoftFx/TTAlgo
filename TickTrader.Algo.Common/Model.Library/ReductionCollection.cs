using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using TickTrader.Algo.Common.Info;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Metadata;
using TickTrader.Algo.Core.Repository;

namespace TickTrader.Algo.Common.Model
{
    public class ReductionCollection
    {
        private IAlgoCoreLogger _logger;
        private Dictionary<ReductionKey, ReductionMetadata> _barToDouble;
        private Dictionary<ReductionKey, ReductionMetadata> _fullbarToDouble;
        private Dictionary<ReductionKey, ReductionMetadata> _fullbarToBar;
        private Dictionary<ReductionKey, ReductionMetadata> _quoteToDouble;
        private Dictionary<ReductionKey, ReductionMetadata> _quoteToBar;


        public IReadOnlyDictionary<ReductionKey, ReductionMetadata> BarToDoubleReductions => _barToDouble;
        public IReadOnlyDictionary<ReductionKey, ReductionMetadata> FullBarToDoubleReductions => _fullbarToDouble;
        public IReadOnlyDictionary<ReductionKey, ReductionMetadata> FullBarToBarReductions => _fullbarToBar;
        public IReadOnlyDictionary<ReductionKey, ReductionMetadata> QuoteToDoubleReductions => _quoteToDouble;
        public IReadOnlyDictionary<ReductionKey, ReductionMetadata> QuoteToBarReductions => _quoteToBar;


        public ReductionCollection(IAlgoCoreLogger logger)
        {
            _logger = logger;

            _barToDouble = new Dictionary<ReductionKey, ReductionMetadata>();
            _fullbarToDouble = new Dictionary<ReductionKey, ReductionMetadata>();
            _fullbarToBar = new Dictionary<ReductionKey, ReductionMetadata>();
            _quoteToDouble = new Dictionary<ReductionKey, ReductionMetadata>();
            _quoteToBar = new Dictionary<ReductionKey, ReductionMetadata>();
        }


        public void Add(ReductionKey key, ReductionMetadata metadata)
        {
            switch (metadata.Descriptor.Type)
            {
                case ReductionType.BarToDouble: _barToDouble.Add(key, metadata); break;
                case ReductionType.FullBarToDouble: _fullbarToDouble.Add(key, metadata); break;
                case ReductionType.FullBarToBar: _fullbarToBar.Add(key, metadata); break;
                case ReductionType.QuoteToDouble: _quoteToDouble.Add(key, metadata); break;
                case ReductionType.QuoteToBar: _quoteToBar.Add(key, metadata); break;
            }
        }

        public void LoadReductions(string path, RepositoryLocation location)
        {
            var plugins = Directory.GetFiles(path, "*.ttalgo");

            foreach (var file in plugins)
            {
                try
                {
                    using (var st = File.OpenRead(file))
                    {
                        var packageName = Path.GetFileName(file).ToLowerInvariant();
                        var pckg = Package.Load(st);
                        if (!string.IsNullOrEmpty(pckg.Metadata.MainBinaryFile))
                        {
                            var extFile = pckg.GetFile(pckg.Metadata.MainBinaryFile);
                            if (extFile != null)
                            {
                                var extAssembly = Assembly.Load(extFile, null);
                                var reductions = AlgoAssemblyInspector.FindReductions(extAssembly);
                                foreach (var r in reductions)
                                    Add(new ReductionKey(packageName, location, r.Id), r);
                            }
                        }
                    }

                    _logger.Info("Loadded extensions from " + file);
                }
                catch (Exception ex)
                {
                    _logger.Error("Cannot load extensions from " + file + "!", ex);
                }
            }
        }

        public void AddAssembly(string extAssemblyName)
        {
            try
            {
                var extAssembly = Assembly.Load(extAssemblyName);
                var packageKey = new PackageKey(extAssembly.GetName().Name, RepositoryLocation.Embedded);
                var reductions = AlgoAssemblyInspector.FindReductions(extAssembly);
                foreach (var r in reductions)
                    Add(new ReductionKey(packageKey, r.Id), r);

                _logger.Info($"Loaded extensions from {extAssemblyName}");
            }
            catch (Exception ex)
            {
                _logger.Error($"Cannot load extensions from {extAssemblyName}!", ex);
            }
        }
    }
}
