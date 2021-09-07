namespace TickTrader.Algo.Domain
{
    public partial class FileFilterEntry
    {
        public FileFilterEntry(string name, string mask)
        {
            FileTypeName = name;
            FileMask = mask;
        }
    }
}
