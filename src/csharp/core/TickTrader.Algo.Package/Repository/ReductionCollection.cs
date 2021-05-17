using System;
using System.Collections.Generic;
using System.Reflection;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Package.Repository
{
    public class ReductionCollection
    {
        private static readonly IAlgoLogger _logger = AlgoLoggerFactory.GetLogger<ReductionCollection>();

        private readonly object _lock = new object();

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


        public ReductionCollection()
        {
            _barToDouble = new List<ReductionInfo>();
            _fullbarToDouble = new List<ReductionInfo>();
            _fullbarToBar = new List<ReductionInfo>();
            _quoteToDouble = new List<ReductionInfo>();
            _quoteToBar = new List<ReductionInfo>();
        }


        public void AddReductions(PackageInfo pkg)
        {
            return; // We should support loading custom reductions into runtime context before allowing this

            //AddReductionsInternal(pkg);
        }

        public void AddFromAssembly(Assembly extAssembly)
        {
            try
            {
                var packageId = PackageId.FromPath(SharedConstants.EmbeddedRepositoryId, extAssembly.Location);
                var pkg = PackageExplorer.ExamineAssembly(packageId, extAssembly);
                
                AddReductionsInternal(pkg);

                _logger.Info($"Loaded extensions from {extAssembly.FullName}");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Cannot load extensions from {extAssembly.FullName}!");
            }
        }


        private void AddReductionsInternal(PackageInfo pkg)
        {
            lock (_lock)
            {
                foreach (var r in pkg.Reductions)
                    Add(r);
            }
        }

        private void Add(ReductionInfo metadata)
        {
            switch (metadata.Descriptor_.Type)
            {
                case Metadata.Types.ReductionType.BarToDouble: _barToDouble.Add(metadata); break;
                case Metadata.Types.ReductionType.FullBarToDouble: _fullbarToDouble.Add(metadata); break;
                case Metadata.Types.ReductionType.FullBarToBar: _fullbarToBar.Add(metadata); break;
                case Metadata.Types.ReductionType.QuoteToDouble: _quoteToDouble.Add(metadata); break;
                case Metadata.Types.ReductionType.QuoteToBar: _quoteToBar.Add(metadata); break;
            }
        }
    }
}
