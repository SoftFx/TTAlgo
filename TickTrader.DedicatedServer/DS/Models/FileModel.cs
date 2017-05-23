namespace TickTrader.DedicatedServer.DS.Models
{
    public class FileModel
    {
        public FileModel(string name, long size)
        {
            Name = name;
            Size = size;
        }

        public long Size { get; private set; }
        public string Name { get; private set; }
    }
}
