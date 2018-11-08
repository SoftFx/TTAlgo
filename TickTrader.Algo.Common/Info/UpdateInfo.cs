namespace TickTrader.Algo.Common.Info
{
    public enum UpdateType
    {
        Added = 0,
        Replaced = 1,
        Removed = 2,
    }


    public class UpdateInfo
    {
        public UpdateType Type { get; set; }
    }


    public class UpdateInfo<T> : UpdateInfo
    {
        public T Value { get; set; }


        public UpdateInfo() { }

        public UpdateInfo(UpdateType type, T value)
        {
            Type = type;
            Value = value;
        }
    }
}
