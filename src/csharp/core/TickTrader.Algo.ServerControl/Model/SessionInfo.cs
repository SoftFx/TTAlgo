using NLog;
using System;
using TickTrader.Algo.Domain.ServerControl;
using TickTrader.Algo.Server.Common;

namespace TickTrader.Algo.ServerControl.Model
{
    internal sealed class SessionInfo
    {
        public string Id { get; private set; }

        public string UserId { get; private set; }

        public VersionSpec VersionSpec { get; private set; }

        public AccessManager AccessManager { get; private set; }

        public ILogger Logger { get; private set; }


        public static SessionInfo Create(string userId, int minorVersion, ClientClaims.Types.AccessLevel accessLevel, LogFactory logFactory)
        {
            var id = Guid.NewGuid().ToString("N");
            return new SessionInfo
            {
                Id = id,
                UserId = userId,
                VersionSpec = new VersionSpec(Math.Min(minorVersion, VersionSpec.MinorVersion)),
                AccessManager = new AccessManager(accessLevel),
                Logger = LoggerHelper.GetSessionLogger(logFactory, id),
            };
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
