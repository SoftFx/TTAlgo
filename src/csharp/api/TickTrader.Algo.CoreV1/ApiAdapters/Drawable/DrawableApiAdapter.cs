using TickTrader.Algo.Api;

namespace TickTrader.Algo.CoreV1
{
    internal class DrawableApiAdapter : IDrawableApi
    {
        public IDrawableCollection LocalCollection { get; } = new DrawableCollection();
    }
}
