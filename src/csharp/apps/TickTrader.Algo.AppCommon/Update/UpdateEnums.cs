namespace TickTrader.Algo.AppCommon.Update
{
    public enum UpdateExitCodes : int
    {
        NoError = 0,
        InvalidArgs = 1,
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
}
