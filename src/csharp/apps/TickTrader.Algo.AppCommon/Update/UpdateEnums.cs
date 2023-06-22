namespace TickTrader.Algo.AppCommon.Update
{
    public enum UpdateErrorCodes : int
    {
        NoError = 0,
        InitError = 1,
        InvalidAppType = 2,
        UnexpectedAppType = 3,
        AppPathNotFound = 4,
        CurrentVersionNotFound = 5,
        UpdateVersionNotFound = 6,
        UpdateVersionMissingExe = 7,
    }

    public enum UpdateAppTypes
    {
        Terminal = 0,
        Server = 1,
    }

    public enum UpdateStatusCodes
    {
        Completed = 0,
        InitError = 1,
        UpdateError = 2,
        Rollback = 3,
    }
}
