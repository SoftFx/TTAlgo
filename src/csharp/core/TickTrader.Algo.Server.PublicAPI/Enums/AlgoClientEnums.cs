namespace TickTrader.Algo.Server.PublicAPI
{
    public enum ClientStates
    {
        Offline,
        Online,
        Connecting,
        Disconnecting,
        LoggingIn,
        LoggingOut,
        Initializing,
        Deinitializing,
        LoggingIn2FA,
    };

    internal enum ClientEvents
    {
        Started,
        Connected,
        Disconnected,
        ConnectionError,
        LoggedIn,
        LoggedOut,
        LoginReject,
        Initialized,
        Deinitialized,
        LogoutRequest,
        Requires2FA,
    }
}
