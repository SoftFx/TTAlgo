using NLog;
using TickTrader.Algo.Domain.ServerControl;
using TickTrader.Algo.Server.Common;

namespace TickTrader.Algo.ServerControl.Model
{
    internal sealed class SessionInfo
    {
        public string Id { get; }

        public string UserId { get; }

        public VersionSpec VersionSpec { get; }

        public AccessManager AccessManager { get; }

        public ILogger Logger { get; }


        public SessionInfo(string id, string userId, int minorVersion, ClientClaims.Types.AccessLevel accessLevel, ILogger logger)
        {
            Id = id;
            UserId = userId;
            VersionSpec = new VersionSpec(minorVersion);
            AccessManager = new AccessManager(accessLevel);
            Logger = logger;
        }


        public JwtPayload GetJwtPayload()
        {
            return new JwtPayload
            {
                SessionId = Id,
                Username = UserId,
                MinorVersion = VersionSpec.MinorVersion,
                AccessLevel = AccessManager.Level,
            };
        }
    }
}
