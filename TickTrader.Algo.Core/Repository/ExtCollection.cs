using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using TickTrader.Algo.Core.Metadata;

namespace TickTrader.Algo.Core.Repository
{
    public class ExtCollection
    {
        private IAlgoCoreLogger _logger;
        private List<ReductionMetadata> _barToDouble = new List<ReductionMetadata>();
        private List<ReductionMetadata> _fullbarToDouble = new List<ReductionMetadata>();
        private List<ReductionMetadata> _fullbarToBar = new List<ReductionMetadata>();
        private List<ReductionMetadata> _quoteToDouble = new List<ReductionMetadata>();
        private List<ReductionMetadata> _quoteToBar = new List<ReductionMetadata>();


        public IEnumerable<ReductionMetadata> BarToDoubleReductions => _barToDouble;
        public IEnumerable<ReductionMetadata> FullBarToDoubleReductions => _fullbarToDouble;
        public IEnumerable<ReductionMetadata> FullBarToBarReductions => _fullbarToBar;
        public IEnumerable<ReductionMetadata> QuoteToDoubleReductions => _quoteToDouble;
        public IEnumerable<ReductionMetadata> QuoteToBarReductions => _quoteToBar;


        public ExtCollection(IAlgoCoreLogger logger)
        {
            _logger = logger;
        }


        public void Add(ReductionMetadata reduction)
        {
            switch (reduction.Descriptor.Type)
            {
                case ReductionType.BarToDouble: _barToDouble.Add(reduction); break;
                case ReductionType.FullBarToDouble: _fullbarToDouble.Add(reduction); break;
                case ReductionType.FullBarToBar: _fullbarToBar.Add(reduction); break;
                case ReductionType.QuoteToDouble: _quoteToDouble.Add(reduction); break;
                case ReductionType.QuoteToBar: _quoteToBar.Add(reduction); break;
            }
        }

        public void LoadExtentions(string path)
        {
            var plugins = Directory.GetFiles(path, "*.ttalgo");

            foreach (var file in plugins)
            {
                try
                {
                    using (var st = File.OpenRead(file))
                    {
                        var pckg = Package.Load(st);
                        if (!string.IsNullOrEmpty(pckg.Metadata.MainBinaryFile))
                        {
                            var extFile = pckg.GetFile(pckg.Metadata.MainBinaryFile);
                            if (extFile != null)
                            {
                                var extAssembly = Assembly.Load(extFile, null);
                                var reductions = AlgoAssemblyInspector.FindReductions(extAssembly);
                                foreach (var r in reductions)
                                    Add(r);
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
                var reductions = AlgoAssemblyInspector.FindReductions(extAssembly);
                foreach (var r in reductions)
                    Add(r);

                _logger.Info($"Loaded extensions from {extAssemblyName}");
            }
            catch (Exception ex)
            {
                _logger.Error($"Cannot load extensions from {extAssemblyName}!", ex);
            }
        }
    }
}
