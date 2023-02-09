using System;
using TickTrader.Algo.Api;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.CoreV1
{
    internal class DrawableObjectAnchorsAdapter : DrawablePropsChangedBase, IDrawableObjectAnchors
    {
        private readonly DrawableObjectAnchorsInfo _info;


        public bool IsSupported => _info != null;

        public int Count => _info.Price.Count;


        public DrawableObjectAnchorsAdapter(DrawableObjectAnchorsInfo info, IDrawableChangedWatcher watcher) : base(watcher)
        {
            _info = info;
        }


        public double GetPrice(int index) => _info.Price[index];

        public DateTime GetTime(int index) => _info.GetTime(index).ToUtcDateTime();

        public void SetPrice(int index, double price)
        {
            _info.Price[index] = price;
            OnChanged();
        }

        public void SetTime(int index, DateTime time)
        {
            _info.SetTime(index, new UtcTicks(time));
            OnChanged();
        }
    }
}
