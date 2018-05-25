namespace TickTrader.Algo.Common.Info
{
    public enum UpdateType
    {
        Added = 0,
        Updated = 1,
        Removed = 2,
    }


    public class UpdateInfo<T>
    {
        public UpdateType Type { get; set; }

        public T Value { get; set; }


        public UpdateInfo() { }

        public UpdateInfo(UpdateType type, T value)
        {
            Type = type;
            Value = value;
        }
    }
}
