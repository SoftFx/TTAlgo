using System;
using System.Linq;
using TickTrader.Algo.Api.Ext;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.CoreV1.Metadata
{
    public class ReductionMetadata
    {
        private Version _apiVersion;
        private Type _reflectionInfo;


        public ReductionDescriptor Descriptor { get; set; }

        public string Id => Descriptor.Id;

        public string DisplayName => Descriptor.DisplayName;


        public ReductionMetadata(Type reflectionInfo, ReductionAttribute reductionAttr)
        {
            _reflectionInfo = reflectionInfo;

            Descriptor = new ReductionDescriptor
            {
                Id = reflectionInfo.FullName,
                DisplayName = reflectionInfo.Name,
            };

            if (!string.IsNullOrEmpty(reductionAttr.DisplayName))
                Descriptor.DisplayName = reductionAttr.DisplayName;
            Descriptor.DisplayName = Descriptor.DisplayName.Replace('.', '_');

            var refs = reflectionInfo.Assembly.GetReferencedAssemblies();
            var apiref = refs.FirstOrDefault(a => a.Name == "TickTrader.Algo.Api");
            _apiVersion = apiref?.Version;
            Descriptor.ApiVersionStr = _apiVersion?.ToString();

            var type = Domain.Metadata.Types.ReductionType.UnknownReductionType;
            if (typeof(BarToDoubleReduction).IsAssignableFrom(reflectionInfo))
                type = Domain.Metadata.Types.ReductionType.BarToDouble;
            else if (typeof(FullBarToDoubleReduction).IsAssignableFrom(reflectionInfo))
                type = Domain.Metadata.Types.ReductionType.FullBarToDouble;
            else if (typeof(FullBarToBarReduction).IsAssignableFrom(reflectionInfo))
                type = Domain.Metadata.Types.ReductionType.FullBarToBar;
            else if (typeof(QuoteToDoubleReduction).IsAssignableFrom(reflectionInfo))
                type = Domain.Metadata.Types.ReductionType.QuoteToDouble;
            else if (typeof(QuoteToBarReduction).IsAssignableFrom(reflectionInfo))
                type = Domain.Metadata.Types.ReductionType.QuoteToBar;
            Descriptor.Type = type;
        }


        public T CreateInstance<T>()
        {
            if (_reflectionInfo == null)
                throw new Exception("This metadata does not belong to current AppDomain. Cannot set value!");

            return (T)Activator.CreateInstance(_reflectionInfo);
        }
    }
}
