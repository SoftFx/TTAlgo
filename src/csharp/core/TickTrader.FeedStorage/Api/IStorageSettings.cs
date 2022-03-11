namespace TickTrader.FeedStorage.Api
{
    public enum StorageFolderOptions
    {
        NoHierarchy, // places history right into specified folder
        ServerHierarchy, // creates subfolder for server
        ServerClientHierarchy // creates subfolder for server and nested subfolder for client.
    }


    public interface IOnlineStorageSettings
    {
        string Login { get; }

        string Server { get; }

        string FolderPath { get; }

        StorageFolderOptions Options { get; }
    }


    public interface ICustomStorageSettings
    {
        string FolderPath { get; }
    }
}
