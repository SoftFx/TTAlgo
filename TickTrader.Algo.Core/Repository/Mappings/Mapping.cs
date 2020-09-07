using System;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Core.Repository
{
    [Serializable]
    public abstract class Mapping
    {
        public MappingKey Key { get; protected set; }

        public string DisplayName { get; protected set; }


        protected Mapping()
        {
        }

        internal Mapping(ReductionKey primaryReductionKey, string primaryReductionDisplayName)
        {
            Key = new MappingKey(primaryReductionKey);
            DisplayName = primaryReductionDisplayName;
        }

        internal Mapping(ReductionKey primaryReductionKey, string primaryReductionDisplayName, ReductionKey secondaryReductionKey, string secondaryReductionDisplayName)
        {
            Key = new MappingKey(primaryReductionKey, secondaryReductionKey);
            DisplayName = $"{primaryReductionDisplayName}.{secondaryReductionDisplayName}";
        }


        public abstract void MapInput(IPluginSetupTarget target, string inputName, string symbol);
    }
}
