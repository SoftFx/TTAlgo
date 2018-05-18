using System;
using System.Linq;
using System.Runtime.Serialization;
using TickTrader.Algo.Api.Ext;

namespace TickTrader.Algo.Core.Metadata
{
    [Serializable]
    public class ReductionMetadata : IDeserializationCallback
    {
        [NonSerialized]
        private Version _apiVersion;
        [NonSerialized]
        private Type _reflectionInfo;


        public ReductionDescriptor Descriptor { get; set; }

        public string Id => Descriptor.Id;


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

            var type = ReductionType.Unknown;
            if (typeof(BarToDoubleReduction).IsAssignableFrom(reflectionInfo))
                type = ReductionType.BarToDouble;
            else if (typeof(FullBarToDoubleReduction).IsAssignableFrom(reflectionInfo))
                type = ReductionType.FullBarToDouble;
            else if (typeof(FullBarToBarReduction).IsAssignableFrom(reflectionInfo))
                type = ReductionType.FullBarToBar;
            else if (typeof(QuoteToDoubleReduction).IsAssignableFrom(reflectionInfo))
                type = ReductionType.QuoteToDouble;
            else if (typeof(QuoteToBarReduction).IsAssignableFrom(reflectionInfo))
                type = ReductionType.QuoteToBar;
            Descriptor.Type = type;
        }


        public T CreateInstance<T>()
        {
            if (_reflectionInfo == null)
                throw new Exception("This metadata does not belong to current AppDomain. Cannot set value!");

            return (T)Activator.CreateInstance(_reflectionInfo);
        }


        void IDeserializationCallback.OnDeserialization(object sender)
        {
            if (Descriptor.ApiVersionStr != null)
                _apiVersion = new Version(Descriptor.ApiVersionStr);
        }
    }
}
