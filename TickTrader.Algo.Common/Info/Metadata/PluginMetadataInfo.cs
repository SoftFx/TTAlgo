using System.Collections.Generic;
using TickTrader.Algo.Core.Metadata;

namespace TickTrader.Algo.Common.Info
{
    public class PluginMetadataInfo
    {
        public string ApiVersion { get; set; }

        public string DisplayName { get; set; }

        public string Description { get; set; }

        public string Category { get; set; }

        public string Version { get; set; }

        public string Copyright { get; set; }

        public bool SetupMainSymbol { get; set; }

        public AlgoTypes Type { get; set; }

        public AlgoMetadataErrors? Error { get; set; }

        /// <summary>
        /// Display name with version if present
        /// </summary>
        public string UiDisplayName { get; set; }

        public bool IsValid => Error == null;

        public List<ParameterMetadataInfo> Parameters { get; set; }

        public List<InputMetadataInfo> Inputs { get; set; }

        public List<OutputMetadataInfo> Outputs { get; set; }
    }
}
