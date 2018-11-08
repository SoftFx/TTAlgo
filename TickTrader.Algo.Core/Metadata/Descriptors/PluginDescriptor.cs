using System;
using System.Collections.Generic;

namespace TickTrader.Algo.Core.Metadata
{
    public enum AlgoTypes
    {
        Unknown = 0,
        Indicator = 1,
        Robot = 2,
    }


    public enum AlgoMetadataErrors
    {
        None = 0,
        Unknown = 1,
        HasInvalidProperties = 2,
        UnknownBaseType = 3,
        IncompatibleApiVersion = 4,
    }


    [Serializable]
    public class PluginDescriptor
    {
        public string ApiVersionStr { get; set; }

        public string Id { get; set; }

        public string DisplayName { get; set; }

        public AlgoTypes Type { get; set; }

        public AlgoMetadataErrors Error { get; set; }

        public bool IsValid => Error == AlgoMetadataErrors.None;

        public string UiDisplayName { get; set; }

        public string Category { get; set; }

        public string Version { get; set; }

        public string Description { get; set; }

        public string Copyright { get; set; }

        public bool SetupMainSymbol { get; set; }

        public List<ParameterDescriptor> Parameters { get; set; }

        public List<InputDescriptor> Inputs { get; set; }

        public List<OutputDescriptor> Outputs { get; set; }

        public PluginDescriptor()
        {
            Parameters = new List<ParameterDescriptor>();
            Inputs = new List<InputDescriptor>();
            Outputs = new List<OutputDescriptor>();
        }
    }
}
