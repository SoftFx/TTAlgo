using TickTrader.Algo.Server.Common;

namespace TickTrader.Algo.Server.PublicAPI
{
    internal sealed class ApiVersionSpec : VersionSpec, IVersionSpec
    {
        int IVersionSpec.MajorVersion => MajorVersion;

        int IVersionSpec.MinorVersion => MinorVersion;


        public ApiVersionSpec(int currentVersion) : base(currentVersion) { }
    }
}
