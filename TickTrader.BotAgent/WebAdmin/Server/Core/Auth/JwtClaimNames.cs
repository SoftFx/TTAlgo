namespace TickTrader.BotAgent.WebAdmin.Server.Core.Auth
{
    public static class JwtClaimNames
    {
        public const string CredsHashClaim = "creds_hash";
        public const string MinorVersionClaim = "protocol_minor_version";
        public const string AccessLevelClaim = "protocol_access_level";
    }
}
