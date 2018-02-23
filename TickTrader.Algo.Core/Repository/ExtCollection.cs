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
        private List<ReductionDescriptor> _barToDouble = new List<ReductionDescriptor>();
        private List<ReductionDescriptor> _fullbarToDouble = new List<ReductionDescriptor>();
        private List<ReductionDescriptor> _fullbarToBar = new List<ReductionDescriptor>();
        private List<ReductionDescriptor> _quoteToDouble = new List<ReductionDescriptor>();
        private List<ReductionDescriptor> _quoteToBar = new List<ReductionDescriptor>();


        public IEnumerable<ReductionDescriptor> BarToDoubleReductions => _barToDouble;
        public IEnumerable<ReductionDescriptor> FullBarToDoubleReductions => _fullbarToDouble;
        public IEnumerable<ReductionDescriptor> FullBarToBarReductions => _fullbarToBar;
        public IEnumerable<ReductionDescriptor> QuoteToDoubleReductions => _quoteToDouble;
        public IEnumerable<ReductionDescriptor> QuoteToBarReductions => _quoteToBar;


        public ExtCollection(IAlgoCoreLogger logger)
        {
            _logger = logger;
        }


        public void Add(ReductionDescriptor reduction)
        {
            switch (reduction.Type)
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
            var plugins = System.IO.Directory.GetFiles(path, "*.ttalgo");

            foreach (var file in plugins)
            {
                try
                {
                    using (var st = File.OpenRead(file))
                    {
                        var pckg = Algo.Core.Package.Load(st);
                        if (!string.IsNullOrEmpty(pckg.Metadata.MainBinaryFile))
                        {
                            var extFile = pckg.GetFile(pckg.Metadata.MainBinaryFile);
                            if (extFile != null)
                            {
                                var extAssembly = Assembly.Load(extFile, null);
                                var reductions = ReductionDescriptor.InspectAssembly(extAssembly);
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
                var reductions = ReductionDescriptor.InspectAssembly(extAssembly);
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
