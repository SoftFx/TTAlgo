using TickTrader.Algo.Server.Common;

namespace TickTrader.Algo.Server.PublicAPI
{
    public sealed class ApiVersionSpec : VersionSpec, IVersionSpec
    {
        int IVersionSpec.MajorVersion => MajorVersion;

        int IVersionSpec.MinorVersion => MinorVersion;


        public ApiVersionSpec() : base() { }

        public ApiVersionSpec(int currentVersion) : base(currentVersion) { }
    }
}
