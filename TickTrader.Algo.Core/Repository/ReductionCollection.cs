using System;
using System.Collections.Generic;
using System.Reflection;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Core.Metadata;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Ext;

namespace TickTrader.Algo.Core.Repository
{
    public class ReductionCollection
    {
        public const string EmbeddedReductionsAssemblyName = "TickTrader.Algo.Ext";


        private IAlgoLogger _logger;
        private List<ReductionInfo> _barToDouble;
        private List<ReductionInfo> _fullbarToDouble;
        private List<ReductionInfo> _fullbarToBar;
        private List<ReductionInfo> _quoteToDouble;
        private List<ReductionInfo> _quoteToBar;


        public IReadOnlyList<ReductionInfo> BarToDoubleReductions => _barToDouble;
        public IReadOnlyList<ReductionInfo> FullBarToDoubleReductions => _fullbarToDouble;
        public IReadOnlyList<ReductionInfo> FullBarToBarReductions => _fullbarToBar;
        public IReadOnlyList<ReductionInfo> QuoteToDoubleReductions => _quoteToDouble;
        public IReadOnlyList<ReductionInfo> QuoteToBarReductions => _quoteToBar;


        public ReductionCollection(IAlgoLogger logger)
        {
            _logger = logger;

            _barToDouble = new List<ReductionInfo>();
            _fullbarToDouble = new List<ReductionInfo>();
            _fullbarToBar = new List<ReductionInfo>();
            _quoteToDouble = new List<ReductionInfo>();
            _quoteToBar = new List<ReductionInfo>();

            AddAssembly(typeof(BarOpenReduction).Assembly);
        }


        public void Add(ReductionKey key, ReductionMetadata metadata)
        {
            var reduction = new ReductionInfo { Key = key, Descriptor_ = metadata.Descriptor };
            switch (metadata.Descriptor.Type)
            {
                case Domain.Metadata.Types.ReductionType.BarToDouble: _barToDouble.Add(reduction); break;
                case Domain.Metadata.Types.ReductionType.FullBarToDouble: _fullbarToDouble.Add(reduction); break;
                case Domain.Metadata.Types.ReductionType.FullBarToBar: _fullbarToBar.Add(reduction); break;
                case Domain.Metadata.Types.ReductionType.QuoteToDouble: _quoteToDouble.Add(reduction); break;
                case Domain.Metadata.Types.ReductionType.QuoteToBar: _quoteToBar.Add(reduction); break;
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
                _logger.Error(ex, $"Cannot load extensions from {extAssemblyName}!");
            }
        }

        public void AddAssembly(Assembly extAssembly)
        {
            try
            {
                var packageId = PackageHelper.GetPackageIdFromPath(SharedConstants.EmbeddedRepositoryId, extAssembly.Location);
                var reductions = AlgoAssemblyInspector.FindReductions(extAssembly);
                foreach (var r in reductions)
                    Add(new ReductionKey(packageId, r.Id), r);

                _logger.Info($"Loaded extensions from {extAssembly.FullName}");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Cannot load extensions from {extAssembly.FullName}!");
            }
        }
    }
}
