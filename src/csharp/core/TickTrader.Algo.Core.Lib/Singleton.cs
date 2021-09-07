namespace TickTrader.Algo.Core.Lib
{
    public class Singleton<T> where T : class, new()
    {
        public static readonly T Instance = new T();
    }
}
