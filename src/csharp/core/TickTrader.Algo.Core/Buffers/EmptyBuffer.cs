namespace TickTrader.Algo.Core
{
    public class EmptyBuffer<T> : IPluginDataBuffer<T>
    {
        public int Count { get { return 0; } }
        public int VirtualPos { get { return 0; } }
        public T this[int index] { get { return default(T); } set { } }
    }
}
