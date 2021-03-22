using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using TickTrader.Algo.Core.Metadata;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Ext;

namespace TickTrader.Algo.Core.Repository
{
    public class ReductionCollection
    {
        public const string EmbeddedReductionsAssemblyName = "TickTrader.Algo.Ext";


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

            AddAssembly(typeof(BarOpenReduction).Assembly);
        }


        public void Add(ReductionKey key, ReductionMetadata metadata)
        {
            switch (metadata.Descriptor.Type)
            {
                case Domain.Metadata.Types.ReductionType.BarToDouble: _barToDouble.Add(key, metadata); break;
                case Domain.Metadata.Types.ReductionType.FullBarToDouble: _fullbarToDouble.Add(key, metadata); break;
                case Domain.Metadata.Types.ReductionType.FullBarToBar: _fullbarToBar.Add(key, metadata); break;
                case Domain.Metadata.Types.ReductionType.QuoteToDouble: _quoteToDouble.Add(key, metadata); break;
                case Domain.Metadata.Types.ReductionType.QuoteToBar: _quoteToBar.Add(key, metadata); break;
            }
        }

        public void LoadReductions(string path, string locationId)
        {
            return; // We should support loading custom reductions in separate app domains before allowing this

            //var plugins = Directory.GetFiles(path, "*.ttalgo");

            //foreach (var file in plugins)
            //{
            //    try
            //    {
            //        using (var st = File.OpenRead(file))
            //        {
            //            var packageName = Path.GetFileName(file).ToLowerInvariant();
            //            var pckg = Package.Load(st);
            //            if (!string.IsNullOrEmpty(pckg.Metadata.MainBinaryFile))
            //            {
            //                var extFile = pckg.GetFile(pckg.Metadata.MainBinaryFile);
            //                if (extFile != null)
            //                {
            //                    var extAssembly = Assembly.Load(extFile, null);
            //                    var reductions = AlgoAssemblyInspector.FindReductions(extAssembly);
            //                    foreach (var r in reductions)
            //                        Add(new ReductionKey(packageName, location, r.Id), r);
            //                }
            //            }
            //        }

            //        _logger.Info("Loadded extensions from " + file);
            //    }
            //    catch (Exception ex)
            //    {
            //        _logger.Error("Cannot load extensions from " + file + "!", ex);
            //    }
            //}
        }

        public void AddAssembly(string extAssemblyName)
        {
            try
            {
                var extAssembly = Assembly.Load(extAssemblyName);
                AddAssembly(extAssembly);
            }
            catch (Exception ex)
            {
                _logger.Error($"Cannot load extensions from {extAssemblyName}!", ex);
            }
        }

        public void AddAssembly(Assembly extAssembly)
        {
            try
            {
                var packageId = PackageHelper.GetPackageIdFromPath(string.Empty, Path.GetFileName(extAssembly.Location).ToLowerInvariant());
                var reductions = AlgoAssemblyInspector.FindReductions(extAssembly);
                foreach (var r in reductions)
                    Add(new ReductionKey(packageId, r.Id), r);

                _logger.Info($"Loaded extensions from {extAssembly.FullName}");
            }
            catch (Exception ex)
            {
                _logger.Error($"Cannot load extensions from {extAssembly.FullName}!", ex);
            }
        }
    }
}
