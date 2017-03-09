using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Metadata;

namespace TickTrader.Algo.Core.Repository
{
    public class ExtCollection
    {
        private List<ReductionDescriptor> barToDouble = new List<ReductionDescriptor>();
        private List<ReductionDescriptor> quoteToDouble = new List<ReductionDescriptor>();
        private List<ReductionDescriptor> fullbarToDouble = new List<ReductionDescriptor>();
        private List<ReductionDescriptor> fullbarToBar = new List<ReductionDescriptor>();

        public IEnumerable<ReductionDescriptor> BarToDoubleReductions => barToDouble;
        public IEnumerable<ReductionDescriptor> QuoteToDoubleReductions => quoteToDouble;
        public IEnumerable<ReductionDescriptor> FullBarToDoubleReductions => fullbarToDouble;
        public IEnumerable<ReductionDescriptor> FullBarToBarReductions => fullbarToBar;

        public void Add(ReductionDescriptor reduction)
        {
            switch (reduction.Type)
            {
                case ReductionType.BarToDouble: barToDouble.Add(reduction); break;
                case ReductionType.FullBarToDouble: fullbarToDouble.Add(reduction); break;
                case ReductionType.QuoteToDouble: quoteToDouble.Add(reduction); break;
                case ReductionType.FullBarToBar: fullbarToBar.Add(reduction); break;
            }
        }

        public void LoadExtentions(string path, IAlgoCoreLogger logger)
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

                    logger.Info("Loadded extensions from " + file);
                }
                catch (Exception ex)
                {
                    logger.Error("Cannot extensions from " + file + "!", ex);
                }
            }
        }
    }
}
