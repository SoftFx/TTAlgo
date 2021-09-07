namespace TickTrader.Algo.Server.PublicAPI
{
    public interface IClientSessionSettings
    {
        string ServerAddress { get; }

        int ServerPort { get; }

        string Login { get; }

        string Password { get; }

        string LogDirectory { get; }

        bool LogMessages { get; }
    }
}
