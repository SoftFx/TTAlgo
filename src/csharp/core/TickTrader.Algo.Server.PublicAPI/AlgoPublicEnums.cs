﻿namespace TickTrader.Algo.Server.PublicAPI
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
        Deinitializing
    };

    public enum ClientEvents
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
        LogoutRequest
    }
}
