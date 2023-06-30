namespace TickTrader.Algo.AppCommon.Update
{
    public enum UpdateErrorCodes : int
    {
        NoError = 0,
        InitError = 1,
        MissingParams = 2,
        IncorrectStatus = 3,
        UnexpectedAppType = 4,
        AppPathNotFound = 5,
        CurrentVersionNotFound = 6,
        UpdateVersionNotFound = 7,
        UpdateVersionMissingExe = 8,
        RegistryIdNotFound = 9,
        ServiceIdNotFound = 10,
        PlatformNotSupported = 11,
    }

    public enum UpdateAppTypes
    {
        Terminal = 0,
        Server = 1,
    }

    public enum UpdateStatusCodes
    {
        Pending = 0,
        Completed = 1,
        InitFailed = 2,
        UpdateError = 3,
        RollbackCompleted = 4,
        RollbackFailed = 5,
    }
}
