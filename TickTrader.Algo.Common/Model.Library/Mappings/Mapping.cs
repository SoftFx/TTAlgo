using TickTrader.Algo.Common.Info;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Metadata;

namespace TickTrader.Algo.Common.Model.Library
{
    public abstract class Mapping
    {
        private ReductionMetadata _barReduction;
        private ReductionMetadata _doubleReduction;


        public MappingKey Key { get; protected set; }

        public string DisplayName { get; protected set; }


        protected Mapping()
        {
        }

        internal Mapping(ReductionKey primaryReductionKey, ReductionMetadata primaryReduction)
        {
            Key = new MappingKey(primaryReductionKey);
            DisplayName = primaryReduction.Descriptor.DisplayName;
        }

        internal Mapping(ReductionKey primaryReductionKey, ReductionMetadata primaryReduction, ReductionKey secondaryReductionKey, ReductionMetadata secondaryReduction)
        {
            Key = new MappingKey(primaryReductionKey, secondaryReductionKey);
            DisplayName = $"{primaryReduction.Descriptor.DisplayName}.{secondaryReduction.Descriptor.DisplayName}";
        }


        internal abstract void MapInput(IPluginSetupTarget target, string inputName, string symbol);
    }
}
